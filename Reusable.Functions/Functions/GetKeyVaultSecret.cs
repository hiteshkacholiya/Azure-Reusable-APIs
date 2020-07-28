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
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.KeyVault;

namespace Reusable.Functions
{
    /// <summary>
    /// This function is to fetch secret value for a given secret name
    /// </summary>
    public static class GetKeyVaultSecret
    {
        //Name of the function endpoint
        [FunctionName("GetKeyVaultSecret")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string secretValue = string.Empty;
            log.LogInformation("GetKeyVaultSecret Function is called");

            try
            {
                //Get secret Name from the query string
                string secretName = req.Query["SecretName"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                secretName = secretName ?? data?.SecretName;

                if (!string.IsNullOrEmpty(secretName))
                {
                    log.LogInformation("Secret Vaule Requested For : " + secretName);
                    
                    //Create new service token provider using System Assigned Managed Identity
                    AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                    
                    //Create new Key Vault Client
                    KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                    
                    //Get Key Vault Name from application configuration
                    string keyVaultName = ConstantsHelper.GetEnvironmentVariable(ConstantsHelper.keyVaultName);
                    log.LogInformation("Fetching Details from KeyVault : " + keyVaultName);
                   
                    //Fetching the Key Vault URI
                    string keyVaultUri = String.Format(Convert.ToString(ConstantsHelper.keyVaultUri), keyVaultName);
                    log.LogInformation("KeyVaultUri : " + keyVaultUri);
                    
                    //Get secret from Key Vault
                    SecretBundle secretBundle = await keyVaultClient.GetSecretAsync(keyVaultUri, secretName);

                    if(secretBundle != null)
                    {
                        //Get Secret Value to be returned
                        secretValue = secretBundle.Value;
                        log.LogInformation("Details are fetched from KeyVault");
                        return new OkObjectResult(secretValue);
                    }
                    else
                    {
                        log.LogInformation("No such key name present in KeyVault");
                        //Return no key found message
                        return new NotFoundObjectResult("No such key name present in KeyVault");
                    }
                }
                else
                {
                    log.LogInformation("secretName is missing in request");
                    return new BadRequestObjectResult("secretName is missing in request");
                }
            }
            catch (Exception ex)
            {
                log.LogInformation($"GetKeyVaultSecret got an exception \n Time: { DateTime.Now} \n Exception{ ex.Message}");
                return new NotFoundObjectResult($"\n GetKeyVaultSecret got an exception \n Time: { DateTime.Now} \n Exception{ ex.Message}");
            }
            
        }
    }
}
