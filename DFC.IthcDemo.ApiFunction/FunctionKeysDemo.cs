using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.IO;
using Newtonsoft.Json;

namespace Logion.Function
{
    public static class FunctionKeysDemo
    {
        [FunctionName("FunctionKeysDemoDfc")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            const string vaultLink = "https://my-dfe-test-key-vault.vault.azure.net/";

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string secretValue = string.Empty;
            if (!string.IsNullOrEmpty(name))
            {
                var client = new SecretClient(new Uri(vaultLink), new DefaultAzureCredential());

                KeyVaultSecret secret = client.GetSecret(name);

                secretValue = secret.Value;

                log.LogInformation("C# HTTP trigger function: I got my secret from the vault.");
            }


            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body to search for a secret."
                : $"The secret for {name} is {secretValue}.";

            log.LogInformation($"{responseMessage}");

            log.LogInformation("C# HTTP trigger function processed a request.");

            return new OkObjectResult(secretValue);
        }
    }
}
