using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using My.DAO;
using Orgref.PostgreSqlDao;

[assembly: FunctionsStartup(typeof(My.Functions.Startup))]
namespace My.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<DAOOptions>()
                .Configure<IConfiguration>((settings, configurationBinder) => {
                    configurationBinder.GetSection("DAOOptions").Bind(settings);
                });
            builder.Services.AddSingleton<OrgrefDAO, OrgrefPostgreSQLDAO>();
        }
    }
}
