using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Reusable.Functions
{
    public class LogAnalyticsHelper
    {
        /// <summary>
        /// Get LogAnalytics Specific Details
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStampField()
        {
            return ConstantsHelper.TimeStampField;
        }

        /// <summary>
        /// Validate User Input Json
        /// </summary>
        /// <param name="strInput"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static bool IsValidJson(string strInput, ILogger log)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    log.LogInformation($"\n IsValidJson method got Exception \n Time: { DateTime.Now} \n Exception{ jex.Message}");
                    return false;
                }
                catch (JsonException je)
                {
                    //Exception in parsing json
                    log.LogInformation($"\n IsValidJson method got Exception \n Time: { DateTime.Now} \n Exception{ je.Message}");
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    //Exception in parsing json
                    log.LogInformation($"\n IsValidJson method got Exception \n Time: { DateTime.Now} \n Exception{ ex.Message}");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// To push logs in Log Analytics Workspace
        /// </summary>
        /// <param name="json"></param>
        /// <param name="workspaceId"></param>
        /// <param name="sharedKey"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static async Task<bool> PushLogsToLogAnalytics(string json, string logFileName, string workspaceId, string sharedKey, ILogger log)
        {
            try
            {
                // Create a hash for the API signature
                var datestring = DateTime.UtcNow.ToString("r");
                var jsonBytes = Encoding.UTF8.GetBytes(json);
                string stringToHash = "POST\n" + jsonBytes.Length + "\napplication/json\n" + "x-ms-date:" + datestring + "\n/api/logs";
                string hashedString = LogAnalyticsHelper.BuildSignature(stringToHash, sharedKey, log);
                log.LogInformation($"HashedString : {hashedString}");
                string signature = "SharedKey " + workspaceId + ":" + hashedString;
                log.LogInformation($"Signature : " + signature);
                bool ingestionStatus = await LogAnalyticsHelper.IngestToLogAnalytics(signature, datestring, json, logFileName, workspaceId, log);
                return ingestionStatus;
            }
            catch (Exception e)
            {
                log.LogInformation($"PushLogsToLogAnalytics got Exception \n  Time: {DateTime.Now} \n Exception{e.Message} and complete Exception:{e}");
                return false;
            }

        }

        //Build the API signature
        /// <summary>
        /// To build signature for log data
        /// </summary>
        /// <param name="message"></param>
        /// <param name="secret"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static string BuildSignature(string message, string secret, ILogger log)
        {
            log.LogInformation($"Begin BuildSignature \n Start Time: {DateTime.Now}");
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = Convert.FromBase64String(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hash = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// To Ingest Into Log Analytics
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="date"></param>
        /// <param name="datajson"></param>
        /// <param name="workspaceId"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static async Task<bool> IngestToLogAnalytics(string signature, string date, string datajson, string logFile, string workspaceId, ILogger log)
        {

            try
            {
                string url = "https://" + workspaceId + ".ods.opinsights.azure.com/api/logs?api-version=2016-04-01";

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Log-Type", logFile);
                client.DefaultRequestHeaders.Add("Authorization", signature);
                client.DefaultRequestHeaders.Add("x-ms-date", date);
                client.DefaultRequestHeaders.Add("time-generated-field", GetTimeStampField());


                HttpContent httpContent = new StringContent(datajson, Encoding.UTF8);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.PostAsync(new Uri(url), httpContent);



                if (response.IsSuccessStatusCode)
                {
                    HttpContent responseContent = response.Content;
                    var result = await responseContent.ReadAsStringAsync().ConfigureAwait(false);
                    log.LogInformation("Ingestion of Logs is completed with status code : " + response.StatusCode);
                    return true;
                }
                else
                {
                    HttpContent responseContent = response.Content;
                    string result = await responseContent.ReadAsStringAsync().ConfigureAwait(false);
                    log.LogInformation("Ingestion of Logs has failed with status code : " + response.StatusCode);
                    return false;
                }

            }
            catch (Exception e)
            {
                log.LogInformation($"IngestToLogAnalytics got Exception \n  Time: {DateTime.Now} \n Exception{e.Message} and complete Exception:{e}");
                return false;
            }
        }       
    }
}
