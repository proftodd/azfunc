using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using My.DAO;
using My.Models;

namespace My.Functions
{
    [TestFixture]
    public class QueryOrgrefTests
    {
        Mock<OrgrefDAO> mockDao;
        QueryOrgref sut;
        private readonly SearchResult sr = new SearchResult();
        private readonly string inchi = "an inchi";

        [SetUp]
        public void Init()
        {
            mockDao = new Mock<OrgrefDAO>();
            mockDao.Setup(md => md.GetSubstances(It.IsAny<string []>())).ReturnsAsync(sr);
            mockDao.Setup(md => md.GetStructure(It.IsAny<string>())).ReturnsAsync(inchi);
            sut = new QueryOrgref(mockDao.Object);
        }

        [Test]
        public async Task query_substances_returns_string_if_no_search_terms_are_provided()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Query = new QueryCollection(new Dictionary<string, StringValues>());

            IActionResult result = await sut.QuerySubstance(request, new Mock<ILogger>().Object);
            var okresult = result as OkObjectResult;
            string response = okresult.Value as string;

            Assert.False(string.IsNullOrEmpty(response));
            mockDao.Verify(md => md.GetSubstances(It.IsAny<string []>()), Times.Never);
        }

        [Test]
        public async Task query_substances_returns_string_if_search_terms_given_as_query_parameters()
        {
            string [] searchTerms = new string [] {"hobt", "h2o"};
            var request = new DefaultHttpContext().Request;
            request.Query = new QueryCollection(new Dictionary<string, StringValues> {
                {"st", new StringValues(searchTerms)}
            } );

            var okResult = (await sut.QuerySubstance(request, new Mock<ILogger>().Object)) as OkObjectResult;
            string response = okResult.Value as string;

            Assert.False(string.IsNullOrEmpty(response));
            var observedSearchResult = JsonSerializer.Deserialize<SearchResult>(response);
            Assert.IsInstanceOf<SearchResult>(observedSearchResult);
            mockDao.Verify(md => md.GetSubstances(searchTerms), Times.Once);
        }

        [Test]
        public async Task query_substances_returns_string_if_search_terms_given_in_body()
        {
            var searchTerms = new string [] {"water", "h2o"};
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            var data = new RequestBody {
                searchTerms = searchTerms
            };
            var json = JsonSerializer.Serialize(data);
            sw.Write(json);
            sw.Flush();
            ms.Position = 0;
            var request = new DefaultHttpContext().Request;
            request.Body = ms;

            var okResult = (await sut.QuerySubstance(request, new Mock<ILogger>().Object)) as OkObjectResult;
            string response = okResult.Value as string;

            Assert.False(string.IsNullOrEmpty(response));
            var observedSearchResult = JsonSerializer.Deserialize<SearchResult>(response);
            Assert.IsInstanceOf<SearchResult>(observedSearchResult);
            mockDao.Verify(md => md.GetSubstances(searchTerms), Times.Once);
        }

        [Test]
        public async Task query_substances_returns_error_to_caller_on_exception()
        {
            mockDao = new Mock<OrgrefDAO>();
            mockDao.Setup(md => md.GetSubstances(It.IsAny<string []>())).ThrowsAsync(new Exception());
            sut = new QueryOrgref(mockDao.Object);

            string [] searchTerms = new string [] {"hobt", "h2o"};
            var request = new DefaultHttpContext().Request;
            request.Query = new QueryCollection(new Dictionary<string, StringValues> {
                {"st", new StringValues(searchTerms)}
            } );

            var errorResult = (await sut.QuerySubstance(request, new Mock<ILogger>().Object)) as ExceptionResult;
            Assert.NotNull(errorResult);
        }

        [Test]
        public async Task query_structure_returns_string_if_no_structure_key_is_provided()
        {
            var okResult = (await sut.QueryStructure(null, null, new Mock<ILogger>().Object)) as OkObjectResult;
            string response = okResult.Value as string;

            Assert.False(string.IsNullOrEmpty(response));
            mockDao.Verify(md => md.GetStructure(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task query_structure_returns_string_if_structure_key_is_provided()
        {
            var inchiKey = "an inchi key";
            var okResult = (await sut.QueryStructure(null, inchiKey, new Mock<ILogger>().Object)) as OkObjectResult;
            string response = okResult.Value as string;

            Assert.AreEqual(inchi, response);
            mockDao.Verify(md => md.GetStructure(inchiKey), Times.Once);
        }

        [Test]
        public async Task query_structure_returns_error_to_caller_on_exception()
        {
            mockDao = new Mock<OrgrefDAO>();
            mockDao.Setup(md => md.GetStructure(It.IsAny<string>())).ThrowsAsync(new Exception());
            sut = new QueryOrgref(mockDao.Object);

            var errorResult = (await sut.QueryStructure(null, "error", new Mock<ILogger>().Object)) as ExceptionResult;
            Assert.NotNull(errorResult);
        }
    }
}