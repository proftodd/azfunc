using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using My.DAO;
using My.Models;

namespace My.Functions
{
    class RequestBody
    {
        public string [] searchTerms { get; set; }
    }

    public class QueryOrgref
    {

        private readonly OrgrefDAO dao;

        public QueryOrgref(OrgrefDAO dao)
        {
            this.dao = dao;
        }

        [FunctionName("QuerySubstance")]
        public async Task<IActionResult> QuerySubstance(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "orgref/search")] HttpRequest request,
            ILogger log
        )
        {
            log.LogInformation("C# HTTP trigger function processed a substance request.");

            string [] searchTerms = request.Query["st"];

            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(requestBody))
            {
                dynamic data = JsonSerializer.Deserialize<RequestBody>(requestBody);
                searchTerms = searchTerms.Length > 0 ? searchTerms: data.searchTerms;
            }

            string responseMessage = searchTerms.Length == 0
                ? "This HTTP triggered function executed successfully. Pass one or more search terms (st=?) in the query or in the request body for more search hits."
                : JsonSerializer.Serialize<SearchResult>(await dao.GetSubstances(searchTerms));
            
            return new OkObjectResult(responseMessage);
        }

        [FunctionName("QueryStructure")]
        public async Task<IActionResult> QueryStructure(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orgref/structure/{inchiKey?}")] HttpRequest request,
            string inchiKey,
            ILogger log
        )
        {
            log.LogInformation("C# HTTP trigger function processed a structure request.");

            string responseMessage = string.IsNullOrEmpty(inchiKey)
                ? "This HTTP triggered function executed successfully. Pass an InChIKey in the HTTP route to query the Orgref database."
                : await dao.GetStructure(inchiKey) ?? "No match found!";

            return new OkObjectResult(responseMessage);            
        }
    }
}