using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using My.DAO;

namespace My.Functions
{
    [TestFixture]
    public class QueryOrgrefTests
    {
        Mock<OrgrefDAO> mockDao;
        QueryOrgref sut;

        [SetUp]
        public void Init()
        {
            mockDao = new Mock<OrgrefDAO>();
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
        public async Task query_structure_returns_string_if_no_structure_key_is_provided()
        {
            var okResult = (await sut.QueryStructure(null, null, new Mock<ILogger>().Object)) as OkObjectResult;
            string response = okResult.Value as string;

            Assert.False(string.IsNullOrEmpty(response));
            mockDao.Verify(md => md.GetStructure(It.IsAny<string>()), Times.Never);
        }
    }
}