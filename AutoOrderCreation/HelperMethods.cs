using AutoOrderCreation.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AutoOrderCreation
{
    public class HelperMethods
    {
        /// <summary>
        /// Calls the web service with the passed in uri path and data without using converters
        /// </summary>
        /// <param name="uriPath">Url of web service to be called</param>        
        /// <param name="jsonData">serialized view of the request</param>        
        /// <returns>json response</returns>
        internal static string CallWebService(string uriPath, string jsonData, string accessToken, string method)
        {
            // Trust all certs for testing
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;

            string response = string.Empty;
            try
            {
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Encoding = System.Text.Encoding.UTF8;
                    client.Headers.Add("Authorization", "Bearer " + accessToken);

                    if (method == "GET")
                    {
                        response = client.DownloadString(uriPath);
                    }
                    else
                    {
                        response = client.UploadString(uriPath, "POST", jsonData);
                    }
                }
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse)wex.Response)
                    {
                        using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            response = reader.ReadToEnd();
                            throw new Exception(response);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                response = e.Message;
                throw new Exception(e.Message);
            }

            return response;
        }

        public static async Task<AccessToken> GetAccessToken()
        {
            string clientId = "id";
            string clientSecret = "secret";
            string audience = "Mobile";
            string url = "https://info3.regiscorpqa.com/tokengenerator/api/oauth2/token";

            string credentials = String.Format("{0}:{1}", clientId, clientSecret);

            using (var client = new HttpClient())
            {
                //Define Headers
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));

                //Prepare Request Body
                List<KeyValuePair<string, string>> requestData = new List<KeyValuePair<string, string>>();
                requestData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
                requestData.Add(new KeyValuePair<string, string>("audience", audience));

                FormUrlEncodedContent requestBody = new FormUrlEncodedContent(requestData);

                //Request Token
                var request = await client.PostAsync(url, requestBody);
                var response = await request.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<AccessToken>(response);
            }
        }

    }
}
