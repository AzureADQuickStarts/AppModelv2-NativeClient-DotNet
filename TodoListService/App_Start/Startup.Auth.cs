using System.Configuration;
using Owin;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Microsoft.IdentityModel.Tokens;

namespace TodoListService
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];

        public void ConfigureAuth(IAppBuilder app)
        {
            // NOTE: The usual WindowsAzureActiveDirectoryBearerAuthentication middleware uses a
            // metadata endpoint which is not supported by the Microsoft identity platform endpoint.  Instead, this 
            // OpenIdConnectSecurityTokenProvider implementation can be used to fetch & use the OpenIdConnect
            // metadata document - which for the identity platform endpoint is https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                AccessTokenFormat = new JwtFormat(
                    new TokenValidationParameters
                    {
                        // Check if the audience is intended to be this application
                        ValidAudiences = new [] { clientId, $"api://{clientId}" },

                        // Change below to 'true' if you want this Web API to accept tokens issued to one Azure AD tenant only (single-tenant)
                        // Note that this is a simplification for the quickstart here. You should validate the issuer. For details, 
                        // see https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore
                        ValidateIssuer = false,

                    },
                    new OpenIdConnectSecurityKeyProvider("https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration")
                ),
            });
        }
    }
}
