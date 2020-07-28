using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Reusable.Functions
{
    public static class GenerateBearerToken
    {
        [FunctionName("GenerateBearerToken")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string token = string.Empty;
            log.LogInformation("CreateBearerToken Function is called");

            try
            {
                string resourceUri = req.Query["ResourceUri"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                resourceUri = resourceUri ?? data?.ResourceUri;

                if (!string.IsNullOrEmpty(resourceUri))
                {
                    log.LogInformation("Fetching details from KeyVault");
                    //Fetching UAMI Client Id and Tenant Id from Key Vaults
                    string clientId_UAMI = await KeyVaultHelper.FetchKeyVaultSecret(ConstantsHelper.GetEnvironmentVariable(ConstantsHelper.clientId_UAMI), log);
                    string tenantId = await KeyVaultHelper.FetchKeyVaultSecret(ConstantsHelper.GetEnvironmentVariable(ConstantsHelper.tenantId), log);
                    token = await TokenHelper.GetToken(clientId_UAMI, tenantId, resourceUri, log);
                    

                    if (!string.IsNullOrEmpty(token))
                    {
                        return new OkObjectResult("Bearer " + token);
                    }
                    else
                    {
                        return new OkObjectResult("[Error] Exception has been occured in generating token.Please check Function logs under Monitor");

                    }
                }
                else
                {
                    return new BadRequestObjectResult("[Warning] Resource Uri is missing in request");
                }

            }
            catch (Exception ex)
            {
                return new NotFoundObjectResult($"\n GenerateBearerToken got an exception \n Time: { DateTime.Now} \n Exception{ ex.Message}");

            }
        }
    }
}
