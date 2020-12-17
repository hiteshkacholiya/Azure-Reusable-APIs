using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Services.AppAuthentication;
using System.Net.Http;

namespace Reusable.Helpers
{
    public class APIHelper
    {
        public static async Task<string> GetAPIResponse(string Uri, string token, ILogger log)
        {
            string value = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", token);
                    var response = await client.GetAsync(Uri).ConfigureAwait(false);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        value = await response.Content.ReadAsStringAsync();
                        return value;
                    }
                    else
                    {
                        log.LogInformation("GetAPIResponse is failed with status code : " + response.StatusCode);
                        return value;
                    }
                }

            }
            catch (Exception ex)
            {
                log.LogInformation($"GetAPIResponse function got \n Exception Time: {DateTime.Now} \n Exception{ ex.Message}");
                return value;
            }

        }
    }
}
