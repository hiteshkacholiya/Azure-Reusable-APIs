# Azure-Reusable-APIs
This repository contains few reusable APIs that can be used to accomplish some functionalities. 
These APIs can be used in a micro service based architecture. All APIs are independently executed.

### 1. [API to Fetch Azure Key Vault Secrets](https://www.c-sharpcorner.com/blogs/creating-an-azure-api-to-fetch-key-vault-secrets_)
This API is basically to fetch a secret value for a given valid Secret Name as query string to the API. For example, if there is a secret value stored with secret name as "TestSecret" in the Key Vault, you can fetch its secret value using this API in below format 
https://Your-API-Management-Resource-Name.azure-api.net/GetKeyVaultSecret?SecretName=TestSecret

### 2. [API to Generate Bearer Token using User Assigned Managed Identity](https://www.c-sharpcorner.com/blogs/creating-an-azure-api-to-generate-bearer-token-against-azure-ad-using)
This API is to generate a bearer token for a given resource Uri, against Azure AD. To generate a bearer token for https://management.azure.com resource Uri, you can use this API in following format so that you can use generated token to invoke any Azure management API to access Azure resources on which used User Assigned Managed Identity has access. 
https://Your-API-Management-Resource-Name.azure-api.net/GenerateBearerToken?ResourceUri=https://management.azure.com

### 3. [API to Push Logs to Log Analytics](https://www.c-sharpcorner.com/blogs/creating-an-azure-api-for-custom-logging-in-azure-log-analytics)
This API provides the capability to developers to store unstructured or desired custom logs into a single place i.e. Log Analytics Workspace. In any development project, there are teams who work on different modules of applications and logs their failures or custom events into their respective logging repository. This API gives an opportunity to store their custom logs at one common place instead of creating multiple custom logs files.

### 4. [API to Create a new Key Vault Secret]()
This API gives you capability to create new secrets into given key vault using User Assigned Managed Identity. With this API, you can create secrets into any given key vault on which permissions are given to configured identity. It is very usefull if integrated with automations where you need to create secrets for storing confidential informations very frequently. This API requires three inputs on Post method - 
  - KeyVaultName : Vault name where secret is to be created
  - SecretName : Unique key name for secret
  - SecretValue : Confidential data string that is to be stored

### 5. [API to generate a random password with enforced complexity which any company will force]()
This API is to generate a random string fulfilling the complexity which any company may force. To generate a random string through this API, we have considered below complexities:
-	It should have more than 30 characters
-	It should have alphanumeric characters
This API works on a Get method and gives you a random string which can be used for passwords or any other use.

### 5. [API to generate a SSH Keys using SshKeyGenerator]()
This API is to generate SSH keys using SshKeyGenerator library of .Net. It generates both kind of keys – private and public, and these keys can be used for any Linux based Azure VM. This API can be integrated with any automation where SSH key generation is to be done dynamically. It requires Virtual Machine Name (VMName) as input to generate the SSH key.
