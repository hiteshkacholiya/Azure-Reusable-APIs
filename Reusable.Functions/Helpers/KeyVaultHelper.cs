using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Functions
{
    public class KeyVaultHelper
    {

        /// <summary>
        /// To fetch secret from Key Vault
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static async Task<string> FetchKeyVaultSecret(string secretName, ILogger log)
        {
            string value = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    //Invoking FetchSecretFromKeyVaultAPI to fetch secret value
                    string Uri = ConstantsHelper.GetEnvironmentVariable(ConstantsHelper.FetchSecretFromKeyVaultAPI) + "?SecretName=" + secretName;
                    
                    //Adding subscription key header to the request
                    client.DefaultRequestHeaders.Add(ConstantsHelper.ocp_Apim_Subscription_Key, ConstantsHelper.GetEnvironmentVariable(ConstantsHelper.ocp_Apim_Subscription_Key));
                    
                    //Get response
                    var response = await client.GetAsync(Uri).ConfigureAwait(false);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        value = await response.Content.ReadAsStringAsync();
                        return value;
                    }
                    else
                    {
                        log.LogInformation("FetchKeyVaultSecret is failed with status code : " + response.StatusCode);
                        return value;
                    }
                }

            }
            catch (Exception ex)
            {
                log.LogInformation($"CreateBearerTokenV3 got \n Exception Time: {DateTime.Now} \n Exception{ ex.Message}");
                return value;
            }
            }
    }
}
