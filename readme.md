# Function connecting to Storage account using a Service Principal

Note: This function uses a service principal to connect to a storage account.  The Storage account does not need to be in the same tenant as the function. 

Create a resource group

    az group create -n "<resourcegroup>" --location "<location>"

Create a storage account in Azure

    az storage account create -n "<storageaccountname>" -g "<resourcegroup>"

Create one or more containers in the storage account

    az storage container create -n "<containername>" --auth-mode login --public-access off

Create the function app plan (note if running the storage account in a different tenant you will need to create a seperate storage account for the function)

    az functionapp create --name <functionname> --os-type Windows --resource-group <resourcegroup> --runtime dotnet --storage-account "<storageaccountname>"

Create a service principal in the same tenant as the storage account (this sp is limited to only the reader role on the storage account)

    scope=$(az storage account show --name "<storageaccountname>" --resource-group "<resourcegroup>" --query id -o tsv)
    az ad sp create-for-rbac -n <serviceprincipalname> --role "Storage Blob Data Reader" --scopes $scope

This will output something like this, save these values...

    {
      "appId": "<appId>",
      "displayName": "<serviceprincipalname>",
      "name": "http://<serviceprincipalname>",
      "password": "<appPassword>",
      "tenant": "<tenantId>"
    }

Update the function app settings

    az functionapp config appsettings set --name BlobAuthFunc --resource-group BlobAuthFunc --settings "ActiveDirectoryAuthEndpoint=https://login.microsoftonline.com"

    az functionapp config appsettings set --name BlobAuthFunc --resource-group BlobAuthFunc --settings "TenantId=<tenantId>"

    az functionapp config appsettings set --name BlobAuthFunc --resource-group BlobAuthFunc --settings "ApplicationId=<appId>"

    az functionapp config appsettings set --name BlobAuthFunc --resource-group BlobAuthFunc --settings "ApplicationSecret=<appPassword>"

    az functionapp config appsettings set --name BlobAuthFunc --resource-group BlobAuthFunc --settings "ActiveDirectoryBlobUri=https://<storageaccountname>.blob.core.windows.net/"

Deploy the function app code to the function and run.