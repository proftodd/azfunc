using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using My.DAO;

[assembly: FunctionsStartup(typeof(My.Functions.Startup))]
namespace My.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<OrgrefDAO>(s => new OrgrefPostgreSQLDAO());
        }
    }
}
