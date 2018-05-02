using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Owin.Security;
using Owin;
using System.IdentityModel.Tokens;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;

namespace TodoListService
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];

        public void ConfigureAuth(IAppBuilder app)
        {
            // NOTE: The usual WindowsAzureActiveDirectoryBearerAuthentication middleware uses a
            // metadata endpoint which is not supported by the v2.0 endpoint.  Instead, this 
            // OpenIdConnectSecurityTokenProvider implementation can be used to fetch & use the OpenIdConnect
            // metadata document - which for the v2 endpoint is https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                AccessTokenFormat = new JwtFormat(
                    new TokenValidationParameters
                    {
                        // Check if the audience is intended to be this application
                        ValidAudience = clientId,

                        // Change below to 'true' if you want this Web API to accept tokens issued to one Azure AD tenant only (single-tenant)
                        ValidateIssuer = false,

                    },
                    new OpenIdConnectSecurityTokenProvider("https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration")
                ),
            });
        }
    }
}
