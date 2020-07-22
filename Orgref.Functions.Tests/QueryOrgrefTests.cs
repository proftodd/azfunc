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
        public async Task it_returns_string_if_no_search_terms_are_provided()
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
    }
}