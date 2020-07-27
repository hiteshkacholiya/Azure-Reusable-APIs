using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Services.AppAuthentication;
using System.Net.Http;

namespace Reusable.Functions
{
    public class TokenHelper
    {

        /// <summary>
        /// To fetch bearer token 
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="tenantID"></param>
        /// <param name="resourceUri"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static async Task<string> GetToken(string clientID, string tenantID, string resourceUri, ILogger log)
        {
            string accessToken = string.Empty;
            try
            {
                var connectionString = "RunAs=App;AppId=" + clientID + "; TenantId="+tenantID+"";
                var azureServiceTokenProviderUAMI = new AzureServiceTokenProvider(connectionString);
                accessToken = await azureServiceTokenProviderUAMI.GetAccessTokenAsync(resourceUri);
                System.Threading.Thread.Sleep(2000);
                return accessToken;
            }
            catch (Exception e)
            {
                log.LogInformation($"GetToken got Exception \n  Time: {DateTime.Now} \n Exception{e.Message}");
                return accessToken;
            }
            
        }
    }
}