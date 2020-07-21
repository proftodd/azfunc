using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using My.DAO;
using My.Models;

namespace My.Functions
{
    public static class QueryOrgref
    {

        private static readonly OrgrefDAO dao = new OrgrefPostgreSQLDAO();

        [FunctionName("QuerySubstance")]
        public static async Task<IActionResult> QuerySubstance(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "orgref/search")] HttpRequest request,
            ILogger log
        )
        {
            log.LogInformation("C# HTTP trigger function processed a substance request.");

            string [] searchTerms = request.Query["st"];

            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            searchTerms = searchTerms.Length > 0 ? searchTerms: data.searchTerms.ToObject<string[]>();

            string responseMessage = searchTerms.Length == 0
                ? "This HTTP triggered function executed successfully. Pass one or more search terms (st=?) in the query or in the request body for more search hits."
                : System.Text.Json.JsonSerializer.Serialize<SearchResult>(dao.GetSubstances(searchTerms));
            
            return new OkObjectResult(responseMessage);
        }

        [FunctionName("QueryStructure")]
        public static async Task<IActionResult> QueryStructure(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orgref/structure/{inchiKey?}")] HttpRequest request,
            string inchiKey,
            ILogger log
        )
        {
            log.LogInformation("C# HTTP trigger function processed a structure request.");

            string responseMessage = string.IsNullOrEmpty(inchiKey)
                ? "This HTTP triggered function executed successfully. Pass an InChIKey in the HTTP route to query the Orgref database."
                : dao.GetStructure(inchiKey) ?? "No match found!";

            return new OkObjectResult(responseMessage);            
        }
    }
}