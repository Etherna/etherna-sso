// Copyright 2021-present Etherna SA
// This file is part of Etherna Sso.
// 
// Etherna Sso is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Sso is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Sso.
// If not, see <https://www.gnu.org/licenses/>.


using Etherna.SSOServer.Configs.Identity;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.SSOServer.Extensions
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test method naming convention.")]
    public class AuthenticationBuilderExtensionsTest
    {
        // Consts.
        private const string Audience = "userApi";
        private const string Authority = "https://sso.test";
        private const string Scheme = "testJwt";
        private const string UserId = "64f0c1a2b3d4e5f601234567";

        // Tests.
        [Fact]
        public async Task AddSsoJwtBearer_WithUserAccessToken_LetsTheUserManagerResolveTheSubject()
        {
            // Arrange.
            using var rsa = RSA.Create(2048);
            var signingKey = new RsaSecurityKey(rsa);
            var accessToken = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Audience = Audience,
                Claims = new Dictionary<string, object> { ["sub"] = UserId },
                Issuer = Authority,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256)
            });

            using var host = await new HostBuilder()
                .ConfigureWebHost(webHost => webHost
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddAuthentication()
                            .AddSsoJwtBearer(Scheme, Audience, Authority, true);
                        //validate with the key of the test, instead of the discovery document of a running authority
                        services.PostConfigure<JwtBearerOptions>(Scheme, options =>
                        {
                            var configuration = new OpenIdConnectConfiguration { Issuer = Authority };
                            configuration.SigningKeys.Add(signingKey);
                            options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(configuration);
                        });

                        //Identity as Program composes it: the claim types set by IdentityServer, and the custom user manager
                        services.AddIdentityCore<UserBase>();
                        services.AddIdentityServer()
                            .AddAspNetIdentity<UserBase>();
                        services.AddSingleton(Mock.Of<IUserStore<UserBase>>());
                        services.Replace(ServiceDescriptor.Scoped<UserManager<UserBase>, CustomUserManager>());
                    })
                    .Configure(app => app.Run(async context =>
                    {
                        var authentication = await context.AuthenticateAsync(Scheme);
                        if (!authentication.Succeeded)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return;
                        }

                        var userManager = context.RequestServices.GetRequiredService<UserManager<UserBase>>();
                        await context.Response.WriteAsync(userManager.GetUserId(authentication.Principal) ?? "");
                    })))
                .StartAsync();
            using var client = host.GetTestClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/", UriKind.Relative));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Action.
            using var response = await client.SendAsync(request);
            var resolvedUserId = await response.Content.ReadAsStringAsync();

            // Assert.
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(UserId, resolvedUserId);
        }
    }
}
