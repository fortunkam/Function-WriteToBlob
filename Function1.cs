using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using System.Linq;

//Create sp using 
//scope=$(az storage account show --name "mfstoragedemo" --resource-group "CrossTenantStorage" --query id -o tsv)
//az ad sp create-for-rbac -n FuncStorageUser --role "Storage Blob Data Reader" --scopes $scope



namespace WriteToBlob
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true) // <- This gives you access to your application settings in your local development environment
                .AddEnvironmentVariables() // <- This is what actually gets you the application settings in Azure
                .Build();

            TokenCredential credential = new ClientSecretCredential(config["TenantId"],
                config["ApplicationId"], config["ApplicationSecret"],
                new TokenCredentialOptions() { 
                    AuthorityHost = new Uri(config["ActiveDirectoryAuthEndpoint"])
                });

            var client = new BlobServiceClient(new Uri(config["ActiveDirectoryBlobUri"]), credential);

            var containers = client.GetBlobContainers();

            var responseMessage = containers.Select(r=>r.Name).ToArray();

            return new OkObjectResult(responseMessage);
        }
    }
}
