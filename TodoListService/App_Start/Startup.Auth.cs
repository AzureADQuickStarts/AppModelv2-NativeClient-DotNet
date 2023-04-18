using Microsoft.Identity.Web;
using Microsoft.Identity.Web.OWIN;
using Owin;
using System.Configuration;

namespace TodoListService
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];

        public void ConfigureAuth(IAppBuilder app)
        {
            OwinTokenAcquirerFactory factory = TokenAcquirerFactory.GetDefaultInstance<OwinTokenAcquirerFactory>();
            app.AddMicrosoftIdentityWebApi(factory);

            // You could add more services if you want to call Microsoft Graph, or
            // a downstream API
            /*
            factory.Services
                .AddMicrosoftGraph()
                .AddDownstreamApi("DownstreamAPI", factory.Configuration.GetSection("DownstreamAPI"));
            */
            factory.Build();

        }
    }
}
