using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Reusable.Functions;

namespace Reusable
{
    /// <summary>
    /// This function is to generate random password 
    /// </summary>
    /// 
    public static class GenerateRandomPassword
    {
        [FunctionName("GeneratePassword")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string passwordGenerated = string.Empty;
            dynamic jObject = new JObject();
            jObject.LogFileName = ConstantsHelper.GetEnvironmentVariable(ConstantsHelper.logName);
            jObject.AutomationName = "Reusable";
            jObject.ModuleName = "GeneratePassword";
            dynamic logJObject = new JObject();

            log.LogInformation("GeneratePassword Function is called");
            logJObject.LogInformation = "GeneratePassword Function is called";
            
            try
            {
                var generator = new PasswordHelper();
                passwordGenerated = generator.Generate(log);

                if (!string.IsNullOrEmpty(passwordGenerated))
                {
                    logJObject.LogInformation += "\n Password has been generated.";
                    return new OkObjectResult(passwordGenerated);
                }
                else
                {
                   logJObject.LogInformation += "\n Exception has been occured in GeneratePassword. Please check Function logs under Monitor.";
                    return new NotFoundObjectResult(passwordGenerated);
                }
                
            }
            catch(Exception ex)
            {
                logJObject.LogInformation += $"\n GeneratePassword got Exception \n Time: { DateTime.Now} \n Exception{ ex.Message}";
                return new NotFoundObjectResult("");
            }
            finally
            {
                using(var client = new HttpClient())
                {
                    string logJson = logJObject.ToString(Newtonsoft.Json.Formatting.None);
                    jObject.LogData = logJson;
                    string myJson = jObject.ToString(Newtonsoft.Json.Formatting.None);
                    //Invoking PushLogsToLogAnalytics API for logging in Log Analytics
                    client.DefaultRequestHeaders.Add(ConstantsHelper.ocp_Apim_Subscription_Key, ConstantsHelper.GetEnvironmentVariable(ConstantsHelper.ocp_Apim_Subscription_Key));
                    var response = await client.PostAsync(ConstantsHelper.GetEnvironmentVariable(ConstantsHelper.PushLogsToLogAnalyticsAPI), new StringContent(myJson, System.Text.Encoding.UTF8, "application/json"));
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        
                        log.LogInformation("Logging is completed successfully with status code : " +response.StatusCode);
                    }
                    else
                    {
                        log.LogInformation("Logging is failed with status code : " + response.StatusCode);
                    }
                }
            }
        }
    }
}
