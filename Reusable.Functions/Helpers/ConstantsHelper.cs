using System;


namespace Reusable.Functions
{

    /// <summary>
    /// Constants Helper to support constant variables
    /// </summary>
    public static class ConstantsHelper
    {
		#region Public methods
		
        // Method to fetch environmental variables from application settings of app service/function app
		public static string GetEnvironmentVariable(string name)
        {
        return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
        
		#endregion
		
		// GetKeyVaultSecret Constants 
        public const string keyVaultName = "keyVaultName";        
        public const string keyVaultUri = "https://{0}.vault.azure.net/";
       
        //GenerateBearerToken Constants
        public const string tenantId = "tenantId";
        public const string clientId_UAMI = "clientId_UAMI";
        public const string ocp_Apim_Subscription_Key = "Ocp-Apim-Subscription-Key";
        public static string FetchSecretFromKeyVaultAPI = "FetchSecretFromKeyVaultAPI";

        //PushLogsToLogAnalytics Constants
        #region Log Analytics Constants 
        public static string logAnalyticsWorkspaceSharedKey = "LogAnalyticsWorkspaceSharedKey";
        public static string logAnalyticsWorkspaceID = "LogAnalyticsWorkspaceID";
        public static string TimeStampField = "";
        #endregion
    }
}