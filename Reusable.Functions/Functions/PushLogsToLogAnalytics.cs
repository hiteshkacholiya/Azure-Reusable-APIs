using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;


namespace Reusable.Functions
{

    /// <summary>
    /// This function is to inject logs into Log Analytics Workspace
    /// </summary>
    public static class PushLogsToLogAnalytics
    {
        [FunctionName("PushLogsToLogAnalytics")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            try
            {
                log.LogInformation("PushLogsToLogAnalytics Function Called");
                //Get Request Data
                dynamic data = await req.Content.ReadAsAsync<object>();
                string customLogFile = data.LogFileName;
                string automationName = data.AutomationName;
                string moduleName = data.ModuleName;
                string logData = Convert.ToString(data.LogData);

                //Parsing provided logData Json
                JObject logDataObj = JObject.Parse(logData);
                string logDataJson = logDataObj.ToString(Newtonsoft.Json.Formatting.Indented);
                
                //Preparing Final Json for Log Analytics Injection
                dynamic obj = new JObject();
                obj.AutomationName = automationName;
                obj.ModuleName = moduleName;
                obj.Log = logDataJson;
                string myJson = obj.ToString(Newtonsoft.Json.Formatting.Indented);
                log.LogInformation("PreparedFinalJson : " + myJson);

                //Validating Json - User provided Log Data Json and prepared final Json
                bool isChildJsonValid = LogAnalyticsHelper.IsValidJson(logDataJson, log);
                bool isParentJsonValid = LogAnalyticsHelper.IsValidJson(myJson, log);

                if (isChildJsonValid && isParentJsonValid)
                {
                    log.LogInformation("Fetching details from KeyVault");
                    log.LogInformation("Invoking FetchKeyVaultSecret method");
                    string workspaceId = await KeyVaultHelper.FetchKeyVaultSecret(ConstantsHelper.GetEnvironmentVariable(ConstantsHelper.logAnalyticsWorkspaceID), log);
                    string primaryKey = await KeyVaultHelper.FetchKeyVaultSecret(ConstantsHelper.GetEnvironmentVariable(ConstantsHelper.logAnalyticsWorkspaceSharedKey), log);
                    log.LogInformation("FetchKeyVaultSecret executed successfully");

                    //Invoking PushLogsToLogAnalytics method to ingest the logs into workspace
                    bool status = await LogAnalyticsHelper.PushLogsToLogAnalytics(myJson, customLogFile, workspaceId, primaryKey, log);
                    if (status)
                    {
                        log.LogInformation("Ingestion of log analytics is completed.");
                        return req.CreateResponse(HttpStatusCode.OK, "[Info] Ingestion of log analytics is completed.");
                    }
                    else
                    {
                        log.LogInformation("Ingestion of log analytics is failed");
                        return req.CreateResponse(HttpStatusCode.BadRequest, "[Error] Ingestion of log analytics is failed");
                    }
                }
                else
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest, $"[Warning] Invalid Json Provided");
                }


            }
            catch (System.Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.NotFound, $"{ex.Message}");
            }

        }
    }
}