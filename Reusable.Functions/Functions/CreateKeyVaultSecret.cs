using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using System.Net.Http;
using System.Net;
using Reusable;

namespace Reusable.Functions
{
    public static class CreateKeyVaultSecret
    {
        [FunctionName("CreateKeyVaultSecret")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            string assignedSecretValue = string.Empty;
            log.LogInformation("CreateKeyVaultSecret Funtion is called");

            try
            {
                // Awaiting the dynamic data for Secret name and value
                dynamic data = await req.Content.ReadAsAsync<object>();
                string keyVaultName = data.KeyVaultName;
                string secretName = data.SecretName;
                string secretValue = data.SecretValue;


                if (!(string.IsNullOrEmpty(secretName) || string.IsNullOrEmpty(secretValue) || string.IsNullOrEmpty(keyVaultName)))
                {
                    log.LogInformation("Secret Name Requested For : " + secretName + "\n Secret Value Requested For : " + secretValue + "\n Key Vault Name Requested For : " + keyVaultName);
                    
                    //Creation of service token provider to use System Assigned Managed Identity
                    AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                    KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                   //string keyVaultName = ConstantsHelper.GetEnvironmentVariable(ConstantsHelper.keyVaultName);
                    log.LogInformation("Fetching Details from KeyVault : " + keyVaultName);
                    string keyVaultUri = String.Format(Convert.ToString(ConstantsHelper.keyVaultUri), keyVaultName);
                    log.LogInformation("KeyVaultUri : " + keyVaultUri);
                    
                    // Await SetSecretAsync
                    SecretBundle secretBundle = await keyVaultClient.SetSecretAsync(keyVaultUri, secretName, secretValue);
                    log.LogInformation(secretBundle.ToString());

                    // Read back the secret name and value
                    SecretBundle getSecretBundle = await keyVaultClient.GetSecretAsync(keyVaultUri, secretName);
                    log.LogInformation(getSecretBundle.ToString());

                    // Log to verify if the secret successfully added to the key vault
                    if (getSecretBundle != null)
                    {
                        log.LogInformation("Secret is added to the KeyVault");
                        return req.CreateResponse(HttpStatusCode.OK, "[Info] Given secret key and value have been added successfully");
                    }

                    // Log to verify if the secret is not added to the key vault
                    else
                    {
                        log.LogInformation("Operation is not succeeded due to either missing key vault or incorrect format passed as secret name or value");
                        return req.CreateResponse(HttpStatusCode.NotFound, "[Info] Given secret addition has been failed");
                    }

                }

                // Log to verify if the secret name and value is missing in the request
                else
                {
                    log.LogInformation("secretName or secretValue is missing in request");
                    return req.CreateResponse(HttpStatusCode.BadRequest, "[Warning] Either Key Vault Name or Secret Name or Value is missing in request");
                }
            }
            catch (Exception ex)
            {
                log.LogInformation($"CreateKeyVaultSecret got an exception \n Time: { DateTime.Now} \n Exception{ ex.Message}");
                return req.CreateResponse(HttpStatusCode.NotFound, $"\n [Error] CreateKeyVaultSecret got an exception \n Time: { DateTime.Now} \n Exception{ ex.Message}");
            }           
        }
    }
}
