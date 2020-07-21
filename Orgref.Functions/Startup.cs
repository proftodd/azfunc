using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using My.DAO;

[assembly: FunctionsStartup(typeof(My.Functions.Startup))]
namespace My.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var _configuration = serviceProvider.GetService<IConfiguration>();
            var username = _configuration.GetValue<string>("username");
            var password = _configuration.GetValue<string>("password");
            var host = _configuration.GetValue<string>("host");
            builder.Services.AddSingleton<OrgrefDAO>(s => new OrgrefPostgreSQLDAO(host: host, username: username, password: password));
        }
    }
}
