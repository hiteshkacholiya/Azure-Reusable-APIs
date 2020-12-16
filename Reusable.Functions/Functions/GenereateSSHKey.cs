using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Reusable.Functions;

namespace Reusable
{
    /// <summary>
    /// This function is to generate both kind of SSH keys - Private and Public 
    /// </summary>
    /// 
    public static class GenerateSSHKey
    {
        [FunctionName("GenerateNewSSHKeys")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string passwordGenerated = string.Empty;
            /* Uncomment below code if log analytics push is to be done*/
            #region dynamic jobject creation for log analytics logs
            // dynamic jObject = new JObject();
            // jObject.LogFileName = ConstantsHelper.GetEnvironmentVariable(ConstantsHelper.logName);
            // jObject.AutomationName = "Reusable";
            // jObject.ModuleName = "GenerateSSHKey";
            // dynamic logJObject = new JObject();
            #endregion

            log.LogInformation("GenerateSSHKey Function is called");
            /* Uncomment below code if log analytics push is to be done*/
            //logJObject.LogInformation = "GenerateSSHKey Function is called";
            
            try
            {
                int keyBits = 2048;
                //get VM Name for which key needs to be generated
                string keyComment = req.Query["VMName"];
                if(!String.IsNullOrEmpty(keyComment))
                {
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    dynamic data = JsonConvert.DeserializeObject(requestBody);
                    keyComment = keyComment ?? data?.VMName;
                    keyComment = keyComment + "-" + DateTime.Now.ToShortDateString();

                    //Generate new SSH Keys
                    var generator = new SshKeyGenerator.SshKeyGenerator(keyBits);

                    if (generator != null)
                    {
                        SSHKeyPair generatedPair = new SSHKeyPair();
                        generatedPair.SSHPrivateKey = generator.ToPrivateKey();  
                        generatedPair.SSHPublicKey= generator.ToRfcPublicKey(keyComment);
                        log.LogInformation("Keys have been generated.");
                        /* Uncomment below code if log analytics push is to be done*/
                        //logJObject.LogInformation += "\n Keys have been generated.";
                        return new OkObjectResult(JsonConvert.SerializeObject(generatedPair));
                    }
                    else
                    {
                       log.LogInformation("Exception has been occured in GenerateSSHKey. Please check Function logs under Monitor.");
                       /* Uncomment below code if log analytics push is to be done*/
                       //logJObject.LogInformation += "\n Exception has been occured in GenerateSSHKey. Please check Function logs under Monitor.";
                       return new NotFoundObjectResult("error result");
                    }
                }
                else
                {
                    return new OkObjectResult(JsonConvert.SerializeObject("Please provide a VM Name for which SSH Keys are to be generated."));
                }
            }
            catch(Exception ex)
            {
                log.LogInformation($"GenerateSSHKey got Exception Time: { DateTime.Now} Exception{ ex.Message}");
                /* Uncomment below code if log analytics push is to be done*/
                //logJObject.LogInformation += $"\n GenerateSSHKey got Exception Time: { DateTime.Now} Exception{ ex.Message}";
                return new NotFoundObjectResult("");
            }
            /* Uncomment below code if log analytics push is to be done*/
            #region finally block for pushing logs into log analytics workspace
            // finally
            // {
            //     using(var client = new HttpClient())
            //     {
            //         string logJson = logJObject.ToString(Newtonsoft.Json.Formatting.None);
            //         jObject.LogData = logJson;
            //         string myJson = jObject.ToString(Newtonsoft.Json.Formatting.None);
            //         //Invoking PushLogsToLogAnalytics API for logging in Log Analytics
            //         client.DefaultRequestHeaders.Add(ConstantsHelper.ocp_Apim_Subscription_Key, ConstantsHelper.GetEnvironmentVariable(ConstantsHelper.ocp_Apim_Subscription_Key));
            //         var response = await client.PostAsync(ConstantsHelper.GetEnvironmentVariable(ConstantsHelper.PushLogsToLogAnalyticsAPI), new StringContent(myJson, System.Text.Encoding.UTF8, "application/json"));
            //         if (response.StatusCode == System.Net.HttpStatusCode.OK)
            //         {
                        
            //             log.LogInformation("Logging is completed successfully with status code : " +response.StatusCode);
            //         }
            //         else
            //         {
            //             log.LogInformation("Logging is failed with status code : " + response.StatusCode);
            //         }
            //     }
            // }
            #endregion
        }
    }

    public class SSHKeyPair
    {
        public string SSHPrivateKey {get;set;}
        public string SSHPublicKey {get;set;}
    }
}
