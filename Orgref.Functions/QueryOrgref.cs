using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using My.DAO;
using My.Models;

namespace My.Functions
{
    public class RequestBody
    {
        public string [] searchTerms { get; set; }
    }

    public class QueryOrgref
    {

        private readonly OrgrefDAO dao;
        private readonly JsonSerializerOptions options;

        public QueryOrgref(OrgrefDAO dao)
        {
            this.dao = dao;
            options = new JsonSerializerOptions()
            {
                MaxDepth = 0,
                IgnoreNullValues = true,
                IgnoreReadOnlyProperties = true
            };
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

            IActionResult result;
            if (searchTerms.Length == 0)
            {
                string responseMessage = "This HTTP triggered function executed successfully. Pass one or more search terms (st=?) in the query or in the request body for more search hits.";
                result = new OkObjectResult(responseMessage);
            } else
            {
                try {
                    var searchResult = await dao.GetSubstances(searchTerms);
                    string responseMessage = JsonSerializer.Serialize<SearchResult>(searchResult, options);
                    result = new OkObjectResult(responseMessage);
                } catch (Exception e)
                {
                    log.LogError($"an exception was thrown: {e}");
                    result = new ExceptionResult(e, false);
                }
            }
            
            return result;
        }

        [FunctionName("QueryStructure")]
        public async Task<IActionResult> QueryStructure(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orgref/structure/{inchiKey?}")] HttpRequest request,
            string inchiKey,
            ILogger log
        )
        {
            log.LogInformation("C# HTTP trigger function processed a structure request.");

            IActionResult result;
            if (string.IsNullOrEmpty(inchiKey))
            {
                var responseMessage = "This HTTP triggered function executed successfully. Pass an InChIKey in the HTTP route to query the Orgref database.";
                result = new OkObjectResult(responseMessage);
            } else
            {
                try {
                    var responseMessage = await dao.GetStructure(inchiKey) ?? "No match found!";
                    result = new OkObjectResult(responseMessage);
                } catch (Exception e)
                {
                    log.LogError($"an exception was thrown: {e}");
                    result = new ExceptionResult(e, false);
                }
            }
            return result;
        }
    }
}