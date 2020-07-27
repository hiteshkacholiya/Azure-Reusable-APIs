using System;


namespace Reusable.Functions
{

    /// <summary>
    /// Constants Helper to support constant variables
    /// </summary>
    public static class ConstantsHelper
    {
        public const string keyVaultName = "keyVaultName";
        public const string opsKeyVaultName = "opsKeyVaultName";
        public const string keyVaultUri = "https://{0}.vault.azure.net/";
        public const string tenantId = "tenantId";
        public const string clientId_UAMI = "clientId_UAMI";
        public const string ocp_Apim_Subscription_Key = "Ocp-Apim-Subscription-Key";
        public static string FetchSecretFromKeyVaultAPI = "FetchSecretFromKeyVaultAPI";
        public static string mgmtResourceUri = "https://management.azure.com";
        public static string mgmtGetSubscriptionsAPI = "https://management.azure.com/subscriptions?api-version=2020-01-01";
        public static string getResourceAPI = "https://management.azure.com/subscriptions/{0}/resources?$filter=name eq '{1}'&api-version=2019-10-01";

        #region Public methods

        public static string GetEnvironmentVariable(string name)
        {
        return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
        #endregion
    }
}