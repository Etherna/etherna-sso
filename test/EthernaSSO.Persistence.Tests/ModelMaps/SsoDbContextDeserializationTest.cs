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

using Duende.IdentityServer.Models;
using Etherna.DomainEvents;
using Etherna.MongoDB.Bson.IO;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Driver;
using Etherna.MongODM.Core.Serialization.Serializers;
using Etherna.MongODM.Core.Utility;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.ClientAppAgg;
using Etherna.SSOServer.Domain.Models.UserAgg;
using Etherna.SSOServer.Persistence.Helpers;
using Etherna.SSOServer.Persistence.Settings;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Etherna.SSOServer.Persistence.ModelMaps
{
    [SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test method naming convention.")]
    public class SsoDbContextDeserializationTest
    {
        // Fields.
        private readonly SsoDbContext dbContext;
        private readonly Mock<IEventDispatcher> eventDispatcherMock = new();
        private readonly Mock<IMongoDatabase> mongoDatabaseMock = new();

        // Constructor.
        public SsoDbContextDeserializationTest()
        {
            var serviceProviderMock = new Mock<IServiceProvider>();
            var ssoDbSeedSettingsMock = new Mock<SsoDbSeedSettings>();

            // Setup dbContext.
            dbContext = new SsoDbContext(eventDispatcherMock.Object, ssoDbSeedSettingsMock.Object, serviceProviderMock.Object);

            DbContextMockHelper.InitializeDbContextMock(dbContext, mongoDatabaseMock);
        }

        // Data.
        public static IEnumerable<object[]> ClientAppDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<ClientApp, SsoDbContext>>();

                // "1f980b1f-74d9-4a44-96c6-b45bfaeb6886" - WebApp
                {
                    var sourceDocument =
                        @"{
                            ""_id"" : ObjectId(""69fcaca3133dda7d424cda52""),
                            ""_m"" : ""1f980b1f-74d9-4a44-96c6-b45bfaeb6886"",
                            ""CreationDateTime"" : ISODate(""2026-05-07T15:15:47.764+0000""),
                            ""AccessTokenType"" : ""Jwt"",
                            ""AllowedCorsOrigins"" : [],
                            ""AllowedGrantTypes"" : [ ""authorization_code"" ],
                            ""AllowedScopes"" : [ ""openid"", ""profile"", ""ether_accounts"", ""role"", ""userApi.credit"", ""userApi.gateway"", ""userApi.sso"" ],
                            ""AllowOfflineAccess"" : true,
                            ""AlwaysIncludeUserClaimsInIdToken"" : true,
                            ""ClientId"" : ""dev_myNLAbCUAYRpxoIfS6py"",
                            ""ClientName"" : ""MyWebApplication"",
                            ""ClientType"" : ""WebApp"",
                            ""ClientSecrets"" : [
                                {
                                    ""_m"" : ""adf2f702-0f84-41bb-ad5d-f485372af6ef"",
                                    ""Created"" : ISODate(""2026-05-07T15:15:47.771+0000""),
                                    ""Description"" : ""Auto-generated secret"",
                                    ""Expiration"" : null,
                                    ""Value"" : ""FRd8ZbmyDZ5JwxDBq4M1D72lAxpouRIYPiKVsGtcWtM=""
                                }
                            ],
                            ""Description"" : ""A new web application"",
                            ""Enabled"" : true,
                            ""Owner"" : {
                                ""_m"" : ""a1976133-bb21-40af-b6de-3a0f7f7dc676"",
                                ""_t"" : ""UserWeb2"",
                                ""_id"" : ObjectId(""69fcabfb133dda7d424cda4e"")
                            },
                            ""PostLogoutRedirectUris"" : [ ""https://myapplication.com/logout"", ""https://myapplication.net/logout"" ],
                            ""RedirectUris"" : [ ""https://myapplication.com/redirect"", ""https://myapplication.net/redirect"" ],
                            ""RefreshTokenUsage"" : ""OneTimeOnly"",
                            ""RequireClientSecret"" : true,
                            ""RequireConsent"" : false,
                            ""RequirePkce"" : false
                        }";

                    var expectedDocumentMock = new Mock<ClientApp>();
                    expectedDocumentMock.Setup(c => c.Id).Returns("69fcaca3133dda7d424cda52");
                    expectedDocumentMock.Setup(c => c.CreationDateTime).Returns(new DateTime(2026, 05, 07, 15, 15, 47, 764));
                    expectedDocumentMock.Setup(c => c.AccessTokenType).Returns(AccessTokenType.Jwt);
                    expectedDocumentMock.Setup(c => c.AllowedCorsOrigins).Returns(Array.Empty<string>());
                    expectedDocumentMock.Setup(c => c.AllowedGrantTypes).Returns(["authorization_code"]);
                    expectedDocumentMock.Setup(c => c.AllowedScopes).Returns(["openid", "profile", "ether_accounts", "role", "userApi.credit", "userApi.gateway", "userApi.sso"]);
                    expectedDocumentMock.Setup(c => c.AllowOfflineAccess).Returns(true);
                    expectedDocumentMock.Setup(c => c.AlwaysIncludeUserClaimsInIdToken).Returns(true);
                    expectedDocumentMock.Setup(c => c.ClientId).Returns("dev_myNLAbCUAYRpxoIfS6py");
                    expectedDocumentMock.Setup(c => c.ClientName).Returns("MyWebApplication");
                    expectedDocumentMock.Setup(c => c.ClientType).Returns(ClientAppType.WebApp);
                    {
                        var secretMock = new Mock<ClientSecret>();
                        secretMock.Setup(s => s.Created).Returns(new DateTime(2026, 05, 07, 15, 15, 47, 771));
                        secretMock.Setup(s => s.Description).Returns("Auto-generated secret");
                        secretMock.Setup(s => s.Expiration).Returns((DateTime?)null);
                        secretMock.Setup(s => s.Value).Returns("FRd8ZbmyDZ5JwxDBq4M1D72lAxpouRIYPiKVsGtcWtM=");
                        expectedDocumentMock.Setup(c => c.ClientSecrets).Returns([secretMock.Object]);
                    }
                    expectedDocumentMock.Setup(c => c.Description).Returns("A new web application");
                    expectedDocumentMock.Setup(c => c.Enabled).Returns(true);
                    {
                        var ownerMock = new Mock<UserBase>();
                        ownerMock.Setup(u => u.Id).Returns("69fcabfb133dda7d424cda4e");
                        expectedDocumentMock.Setup(c => c.Owner).Returns(ownerMock.Object);
                    }
                    expectedDocumentMock.Setup(c => c.PostLogoutRedirectUris).Returns(["https://myapplication.com/logout", "https://myapplication.net/logout"]);
                    expectedDocumentMock.Setup(c => c.RedirectUris).Returns(["https://myapplication.com/redirect", "https://myapplication.net/redirect"]);
                    expectedDocumentMock.Setup(c => c.RefreshTokenUsage).Returns(TokenUsage.OneTimeOnly);
                    expectedDocumentMock.Setup(c => c.RequireClientSecret).Returns(true);
                    expectedDocumentMock.Setup(c => c.RequireConsent).Returns(false);
                    expectedDocumentMock.Setup(c => c.RequirePkce).Returns(false);

                    tests.Add(new(sourceDocument, expectedDocumentMock.Object));
                }

                // "1f980b1f-74d9-4a44-96c6-b45bfaeb6886" - NativeApp
                {
                    var sourceDocument =
                        @"{
                            ""_id"" : ObjectId(""69fcad52133dda7d424cda53""),
                            ""_m"" : ""1f980b1f-74d9-4a44-96c6-b45bfaeb6886"",
                            ""CreationDateTime"" : ISODate(""2026-05-07T15:18:42.269+0000""),
                            ""AccessTokenType"" : ""Jwt"",
                            ""AllowedCorsOrigins"" : [ ""native.com"" ],
                            ""AllowedGrantTypes"" : [ ""authorization_code"" ],
                            ""AllowedScopes"" : [ ""openid"", ""role"", ""userApi.credit"", ""userApi.index"" ],
                            ""AllowOfflineAccess"" : true,
                            ""AlwaysIncludeUserClaimsInIdToken"" : true,
                            ""ClientId"" : ""dev_x7gxOuz18R5xtZafodRa"",
                            ""ClientName"" : ""MyNative"",
                            ""ClientType"" : ""NativeApp"",
                            ""ClientSecrets"" : [],
                            ""Description"" : ""A new native application"",
                            ""Enabled"" : true,
                            ""Owner"" : {
                                ""_m"" : ""a1976133-bb21-40af-b6de-3a0f7f7dc676"",
                                ""_t"" : ""UserWeb2"",
                                ""_id"" : ObjectId(""69fcabfb133dda7d424cda4e"")
                            },
                            ""PostLogoutRedirectUris"" : [ ""http://native.com"" ],
                            ""RedirectUris"" : [ ""http://native.com"" ],
                            ""RefreshTokenUsage"" : ""OneTimeOnly"",
                            ""RequireClientSecret"" : false,
                            ""RequireConsent"" : false,
                            ""RequirePkce"" : true
                        }";

                    var expectedDocumentMock = new Mock<ClientApp>();
                    expectedDocumentMock.Setup(c => c.Id).Returns("69fcad52133dda7d424cda53");
                    expectedDocumentMock.Setup(c => c.CreationDateTime).Returns(new DateTime(2026, 05, 07, 15, 18, 42, 269));
                    expectedDocumentMock.Setup(c => c.AccessTokenType).Returns(AccessTokenType.Jwt);
                    expectedDocumentMock.Setup(c => c.AllowedCorsOrigins).Returns(["native.com"]);
                    expectedDocumentMock.Setup(c => c.AllowedGrantTypes).Returns(["authorization_code"]);
                    expectedDocumentMock.Setup(c => c.AllowedScopes).Returns(["openid", "role", "userApi.credit", "userApi.index"]);
                    expectedDocumentMock.Setup(c => c.AllowOfflineAccess).Returns(true);
                    expectedDocumentMock.Setup(c => c.AlwaysIncludeUserClaimsInIdToken).Returns(true);
                    expectedDocumentMock.Setup(c => c.ClientId).Returns("dev_x7gxOuz18R5xtZafodRa");
                    expectedDocumentMock.Setup(c => c.ClientName).Returns("MyNative");
                    expectedDocumentMock.Setup(c => c.ClientType).Returns(ClientAppType.NativeApp);
                    expectedDocumentMock.Setup(c => c.ClientSecrets).Returns(Array.Empty<ClientSecret>());
                    expectedDocumentMock.Setup(c => c.Description).Returns("A new native application");
                    expectedDocumentMock.Setup(c => c.Enabled).Returns(true);
                    {
                        var ownerMock = new Mock<UserBase>();
                        ownerMock.Setup(u => u.Id).Returns("69fcabfb133dda7d424cda4e");
                        expectedDocumentMock.Setup(c => c.Owner).Returns(ownerMock.Object);
                    }
                    expectedDocumentMock.Setup(c => c.PostLogoutRedirectUris).Returns(["http://native.com"]);
                    expectedDocumentMock.Setup(c => c.RedirectUris).Returns(["http://native.com"]);
                    expectedDocumentMock.Setup(c => c.RefreshTokenUsage).Returns(TokenUsage.OneTimeOnly);
                    expectedDocumentMock.Setup(c => c.RequireClientSecret).Returns(false);
                    expectedDocumentMock.Setup(c => c.RequireConsent).Returns(false);
                    expectedDocumentMock.Setup(c => c.RequirePkce).Returns(true);

                    tests.Add(new(sourceDocument, expectedDocumentMock.Object));
                }

                // "1f980b1f-74d9-4a44-96c6-b45bfaeb6886" - ClientCredential
                {
                    var sourceDocument =
                        @"{
                            ""_id"" : ObjectId(""69fcad8c133dda7d424cda54""),
                            ""_m"" : ""1f980b1f-74d9-4a44-96c6-b45bfaeb6886"",
                            ""CreationDateTime"" : ISODate(""2026-05-07T15:19:40.070+0000""),
                            ""AccessTokenType"" : ""Jwt"",
                            ""AllowedCorsOrigins"" : [],
                            ""AllowedGrantTypes"" : [ ""client_credentials"" ],
                            ""AllowedScopes"" : [ ""openid"", ""userApi.gateway"", ""userApi.index"", ""userApi.sso"" ],
                            ""AllowOfflineAccess"" : false,
                            ""AlwaysIncludeUserClaimsInIdToken"" : false,
                            ""ClientId"" : ""dev_eGCxsQuCBo7zeHDyvlwD"",
                            ""ClientName"" : ""Machine2machine"",
                            ""ClientType"" : ""ClientCredential"",
                            ""ClientSecrets"" : [
                                {
                                    ""_m"" : ""adf2f702-0f84-41bb-ad5d-f485372af6ef"",
                                    ""Created"" : ISODate(""2026-05-07T15:19:40.070+0000""),
                                    ""Description"" : ""Auto-generated secret"",
                                    ""Expiration"" : null,
                                    ""Value"" : ""unK/QiBKwvuiMLJij2Lkebeo/InWRCCga+yUslzFU0o=""
                                }
                            ],
                            ""Description"" : ""My machine 2 machine app"",
                            ""Enabled"" : true,
                            ""Owner"" : {
                                ""_m"" : ""a1976133-bb21-40af-b6de-3a0f7f7dc676"",
                                ""_t"" : ""UserWeb2"",
                                ""_id"" : ObjectId(""69fcabfb133dda7d424cda4e"")
                            },
                            ""PostLogoutRedirectUris"" : [],
                            ""RedirectUris"" : [],
                            ""RefreshTokenUsage"" : ""ReUse"",
                            ""RequireClientSecret"" : true,
                            ""RequireConsent"" : false,
                            ""RequirePkce"" : false
                        }";

                    var expectedDocumentMock = new Mock<ClientApp>();
                    expectedDocumentMock.Setup(c => c.Id).Returns("69fcad8c133dda7d424cda54");
                    expectedDocumentMock.Setup(c => c.CreationDateTime).Returns(new DateTime(2026, 05, 07, 15, 19, 40, 070));
                    expectedDocumentMock.Setup(c => c.AccessTokenType).Returns(AccessTokenType.Jwt);
                    expectedDocumentMock.Setup(c => c.AllowedCorsOrigins).Returns(Array.Empty<string>());
                    expectedDocumentMock.Setup(c => c.AllowedGrantTypes).Returns(["client_credentials"]);
                    expectedDocumentMock.Setup(c => c.AllowedScopes).Returns(["openid", "userApi.gateway", "userApi.index", "userApi.sso"]);
                    expectedDocumentMock.Setup(c => c.AllowOfflineAccess).Returns(false);
                    expectedDocumentMock.Setup(c => c.AlwaysIncludeUserClaimsInIdToken).Returns(false);
                    expectedDocumentMock.Setup(c => c.ClientId).Returns("dev_eGCxsQuCBo7zeHDyvlwD");
                    expectedDocumentMock.Setup(c => c.ClientName).Returns("Machine2machine");
                    expectedDocumentMock.Setup(c => c.ClientType).Returns(ClientAppType.ClientCredential);
                    {
                        var secretMock = new Mock<ClientSecret>();
                        secretMock.Setup(s => s.Created).Returns(new DateTime(2026, 05, 07, 15, 19, 40, 070));
                        secretMock.Setup(s => s.Description).Returns("Auto-generated secret");
                        secretMock.Setup(s => s.Expiration).Returns((DateTime?)null);
                        secretMock.Setup(s => s.Value).Returns("unK/QiBKwvuiMLJij2Lkebeo/InWRCCga+yUslzFU0o=");
                        expectedDocumentMock.Setup(c => c.ClientSecrets).Returns([secretMock.Object]);
                    }
                    expectedDocumentMock.Setup(c => c.Description).Returns("My machine 2 machine app");
                    expectedDocumentMock.Setup(c => c.Enabled).Returns(true);
                    {
                        var ownerMock = new Mock<UserBase>();
                        ownerMock.Setup(u => u.Id).Returns("69fcabfb133dda7d424cda4e");
                        expectedDocumentMock.Setup(c => c.Owner).Returns(ownerMock.Object);
                    }
                    expectedDocumentMock.Setup(c => c.PostLogoutRedirectUris).Returns(Array.Empty<string>());
                    expectedDocumentMock.Setup(c => c.RedirectUris).Returns(Array.Empty<string>());
                    expectedDocumentMock.Setup(c => c.RefreshTokenUsage).Returns(TokenUsage.ReUse);
                    expectedDocumentMock.Setup(c => c.RequireClientSecret).Returns(true);
                    expectedDocumentMock.Setup(c => c.RequireConsent).Returns(false);
                    expectedDocumentMock.Setup(c => c.RequirePkce).Returns(false);

                    tests.Add(new(sourceDocument, expectedDocumentMock.Object));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        public static IEnumerable<object[]> AlphaPassRequestDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<AlphaPassRequest, SsoDbContext>>();

                // "cdfb69bd-b70c-4736-9210-737b675333bc" - v0.3.22
                {
                    var sourceDocument =
                        @"{
                            ""_id"" : ObjectId(""6328dcf4955896e143e25f4c""),
                            ""_m"" : ""cdfb69bd-b70c-4736-9210-737b675333bc"",
                            ""CreationDateTime"" : ISODate(""2022-09-19T21:19:48.210+0000""),
                            ""IsEmailConfirmed"" : true,
                            ""IsInvitationSent"" : true,
                            ""NormalizedEmail"" : ""WOW@EMAIL.COM"",
                            ""Secret"" : ""MXJZZ3FD6G""
                        }";

                    var expectedDocumentMock = new Mock<AlphaPassRequest>();
                    expectedDocumentMock.Setup(d => d.Id).Returns("6328dcf4955896e143e25f4c");
                    expectedDocumentMock.Setup(d => d.CreationDateTime).Returns(new DateTime(2022, 09, 19, 21, 19, 48, 210));
                    expectedDocumentMock.Setup(d => d.IsEmailConfirmed).Returns(true);
                    expectedDocumentMock.Setup(d => d.IsInvitationSent).Returns(true);
                    expectedDocumentMock.Setup(d => d.NormalizedEmail).Returns("WOW@EMAIL.COM");
                    expectedDocumentMock.Setup(d => d.Secret).Returns("MXJZZ3FD6G");

                    tests.Add(new(sourceDocument, expectedDocumentMock.Object));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        public static IEnumerable<object[]> ApiKeyDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<ApiKey, SsoDbContext>>();

                // "4c9f5ecd-37b7-425e-8cc4-96ec97ef443b" - v0.3.25
                {
                    var sourceDocument =
                        @"{
                            ""_id"" : ObjectId(""6328dcf4955896e143e25f4c""),
                            ""_m"" : ""4c9f5ecd-37b7-425e-8cc4-96ec97ef443b"",
                            ""CreationDateTime"" : ISODate(""2023-05-31T15:40:27.853+0000""),
                            ""EndOfLife"" : ISODate(""2024-05-31T15:40:27.853+0000""),
                            ""KeyHash"" : BinData(0, ""z5RIUYPqGLvSK1enR5iJmTHQ3sz99cTjhOR0s+STBjs=""),
                            ""Label"" : ""test"",
                            ""Owner"" : {
                                ""_m"" : ""a1976133-bb21-40af-b6de-3a0f7f7dc676"",
                                ""_t"" : ""UserWeb2"",
                                ""_id"" : ObjectId(""61cdeb676b35d8905b1d68cf"")
                            }
                        }";

                    var expectedDocumentMock = new Mock<ApiKey>();
                    expectedDocumentMock.Setup(k => k.Id).Returns("6328dcf4955896e143e25f4c");
                    expectedDocumentMock.Setup(k => k.CreationDateTime).Returns(new DateTime(2023, 05, 31, 15, 40, 27, 853));
                    expectedDocumentMock.Setup(k => k.EndOfLife).Returns(new DateTime(2024, 05, 31, 15, 40, 27, 853));
                    expectedDocumentMock.Setup(k => k.KeyHash).Returns(new byte[] { 207, 148, 72, 81, 131, 234, 24, 187, 210, 43, 87, 167, 71, 152, 137, 153, 49, 208, 222, 204, 253, 245, 196, 227, 132, 228, 116, 179, 228, 147, 6, 59 });
                    expectedDocumentMock.Setup(k => k.Label).Returns("test");
                    {
                        var userMock = new Mock<UserBase>();
                        userMock.Setup(u => u.Id).Returns("61cdeb676b35d8905b1d68cf");
                        expectedDocumentMock.Setup(d => d.Owner).Returns(userMock.Object);
                    }

                    tests.Add(new(sourceDocument, expectedDocumentMock.Object));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        public static IEnumerable<object[]> DailyStatsDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<DailyStats, SsoDbContext>>();

                // "375a3f26-9219-4ae4-86cf-32b9ba0ac703" - v0.3.22
                {
                    var sourceDocument =
                        @"{
                            ""_id"" : ObjectId(""620efda68550c578b85d0d3f""),
                            ""_m"" : ""375a3f26-9219-4ae4-86cf-32b9ba0ac703"",
                            ""CreationDateTime"" : ISODate(""2022-02-18T02:00:06.818+0000""),
                            ""ActiveUsersInLast30Days"" : NumberLong(2),
                            ""ActiveUsersInLast60Days"" : NumberLong(2),
                            ""ActiveUsersInLast180Days"" : NumberLong(2),
                            ""TotalUsers"" : NumberLong(4)
                        }";

                    var expectedDocumentMock = new Mock<DailyStats>();
                    expectedDocumentMock.Setup(c => c.Id).Returns("620efda68550c578b85d0d3f");
                    expectedDocumentMock.Setup(c => c.CreationDateTime).Returns(new DateTime(2022, 02, 18, 02, 00, 06, 818));
                    expectedDocumentMock.Setup(c => c.ActiveUsersInLast30Days).Returns(2);
                    expectedDocumentMock.Setup(c => c.ActiveUsersInLast60Days).Returns(2);
                    expectedDocumentMock.Setup(c => c.ActiveUsersInLast180Days).Returns(2);
                    expectedDocumentMock.Setup(c => c.TotalUsers).Returns(4);

                    tests.Add(new(sourceDocument, expectedDocumentMock.Object));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        public static IEnumerable<object[]> InvitationDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<Invitation, SsoDbContext>>();

                // "a51c7ca1-b53e-43d2-b1ab-7efb7f5e735b" - v0.3.22
                {
                    var sourceDocument =
                        @"{
                            ""_id"" : ObjectId(""63264cfe14393a3e01fdb5a8""),
                            ""_m"" : ""a51c7ca1-b53e-43d2-b1ab-7efb7f5e735b"",
                            ""CreationDateTime"" : ISODate(""2022-09-17T22:41:02.066+0000""),
                            ""Code"" : ""776d0338-2ea5-4171-b823-5b9b8064f7e4"",
                            ""Emitter"" : {
                                ""_m"" : ""a1976133-bb21-40af-b6de-3a0f7f7dc676"",
                                ""_t"" : ""UserWeb2"",
                                ""_id"" : ObjectId(""61cdeb676b35d8905b1d68cf"")
                            },
                            ""EndLife"" : ISODate(""2022-10-17T22:41:02.066+0000""),
                            ""IsFromAdmin"" : true,
                            ""IsSingleUse"" : true
                        }";

                    var expectedDocumentMock = new Mock<Invitation>();
                    expectedDocumentMock.Setup(c => c.Id).Returns("63264cfe14393a3e01fdb5a8");
                    expectedDocumentMock.Setup(c => c.CreationDateTime).Returns(new DateTime(2022, 09, 17, 22, 41, 02, 066));
                    expectedDocumentMock.Setup(c => c.Code).Returns("776d0338-2ea5-4171-b823-5b9b8064f7e4");
                    {
                        var userMock = new Mock<UserBase>();
                        userMock.Setup(u => u.Id).Returns("61cdeb676b35d8905b1d68cf");
                        expectedDocumentMock.Setup(d => d.Emitter).Returns(userMock.Object);
                    }
                    expectedDocumentMock.Setup(c => c.EndLife).Returns(new DateTime(2022, 10, 17, 22, 41, 02, 066));
                    expectedDocumentMock.Setup(c => c.IsFromAdmin).Returns(true);
                    expectedDocumentMock.Setup(c => c.IsSingleUse).Returns(true);

                    tests.Add(new(sourceDocument, expectedDocumentMock.Object));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        public static IEnumerable<object[]> RoleDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<Role, SsoDbContext>>();

                // "82413cc7-9f38-4ea2-a841-4d9479ab4f11" - v0.3.22
                {
                    var sourceDocument =
                        @"{
                            ""_id"" : ObjectId(""61cdeb616b13d8985b3d688d""),
                            ""_m"" : ""82413cc7-9f38-4ea2-a841-4d9479ab4f11"",
                            ""CreationDateTime"" : ISODate(""2021-12-30T17:24:49.603+0000""),
                            ""Name"" : ""Administrator"",
                            ""NormalizedName"" : ""ADMINISTRATOR""
                        }";

                    var expectedDocumentMock = new Mock<Role>();
                    expectedDocumentMock.Setup(d => d.Id).Returns("61cdeb616b13d8985b3d688d");
                    expectedDocumentMock.Setup(d => d.CreationDateTime).Returns(new DateTime(2021, 12, 30, 17, 24, 49, 603));
                    expectedDocumentMock.Setup(d => d.Name).Returns("Administrator");
                    expectedDocumentMock.Setup(d => d.NormalizedName).Returns("ADMINISTRATOR");

                    tests.Add(new(sourceDocument, expectedDocumentMock.Object));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        public static IEnumerable<object[]> UserDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<UserBase, SsoDbContext>>();

                // "2ccb567f-63cc-4fb3-b66e-a51fb4ff1bfe" - v0.3.22 - UserWeb2
                {
                    var sourceDocument =
                        @"{
                            ""_id"" : ObjectId(""62fd293ca12c0fd52db29c8d""),
                            ""_m"" : ""2ccb567f-63cc-4fb3-b66e-a51fb4ff1bfe"",
                            ""_t"" : ""UserWeb2"",
                            ""CreationDateTime"" : ISODate(""2022-08-17T17:45:32.876+0000""),
                            ""Claims"" : [
                                {
                                    ""_m"" : ""f7831985-dc0c-439f-b118-d7c511619a87"",
                                    ""Type"" : ""ether_address"",
                                    ""Value"" : ""0xDe87768A7B118aAA23Cd3552E4AD34B8F4566Bde""
                                },
                                {
                                    ""_m"" : ""f7831985-dc0c-439f-b118-d7c511619a87"",
                                    ""Type"" : ""ether_prev_addresses"",
                                    ""Value"" : ""[\""0xd6cEd4963410D5B99a90510Fe2DcAED517EAa03C\""]""
                                }
                            ],
                            ""Email"" : ""asdfg@sas.so"",
                            ""EtherAddress"" : ""0xDe87768A7B118aAA23Cd3552E4AD34B8F4566Bde"",
                            ""EtherPreviousAddresses"" : [
                                ""0xd6cEd4963410D5B99a90510Fe2DcAED517EAa03C""
                            ],
                            ""InvitedBy"" : {
                                ""_m"" : ""a1976133-bb21-40af-b6de-3a0f7f7dc676"",
                                ""_t"" : ""UserWeb2"",
                                ""_id"" : ObjectId(""61cd0b616b33d8785b9d34cf"")
                            },
                            ""InvitedByAdmin"" : true,
                            ""LastLoginDateTime"" : ISODate(""2022-09-17T14:22:06.601+0000""),
                            ""NormalizedEmail"" : ""ASDFG@SAS.SO"",
                            ""NormalizedUsername"" : ""ASDFGA"",
                            ""PhoneNumber"" : ""123-456-7890"",
                            ""PhoneNumberConfirmed"" : true,
                            ""Roles"" : [
                                {
                                    ""_m"" : ""cc9c6902-edd5-491d-acb5-07ca02fa71d0"",
                                    ""_id"" : ObjectId(""61cdeb616b13d8985b3d688d""),
                                    ""NormalizedName"" : ""ADMINISTRATOR""
                                }
                            ],
                            ""SecurityStamp"" : ""ZNB7TIG3GMD6SAWRIYPN2ST5Q734O7DV"",
                            ""SharedInfoId"" : ""62fd293ca12c0fd52db29c8c"",
                            ""Username"" : ""asdfga"",
                            ""AccessFailedCount"" : NumberInt(5),
                            ""AuthenticatorKey"" : ""SVKPXJRGFOAJGSJZOUUUJSSZZTYBXDPL"",
                            ""EtherManagedPrivateKey"" : ""e883fcbe10b59d63dc7f1bbed29dbd81f17a03fc65ea7d87461f45a6dfe76d0c"",
                            ""EtherLoginAddress"" : ""0xfeF78523191CC15e287b3F7ABFbd0c3d621f053b"",
                            ""Logins"" : [
                                {
                                    ""_m"" : ""6cec179b-807a-4ff9-977b-9314a60725a7"",
                                    ""LoginProvider"" : ""Twitter"",
                                    ""ProviderDisplayName"" : ""Twitter"",
                                    ""ProviderKey"" : ""2777002738532021212""
                                }
                            ],
                            ""PasswordHash"" : ""AQAAAAEAACcQAAAAELAZKxcX4rTHtVo4ZBbpZdaxfsiB4xaOM/3mEO86iq8vdUPtglbwyk7qa2jDajBWUA=="",
                            ""TwoFactorEnabled"" : true,
                            ""TwoFactorRecoveryCodes"" : [
                                ""q56k5c6s"",
                                ""l3rcwj6v"",
                                ""8y9b5lqv""
                            ]
                        }";

                    var expectedDocumentMock = new Mock<UserWeb2>();
                    expectedDocumentMock.Setup(d => d.Id).Returns("62fd293ca12c0fd52db29c8d");
                    expectedDocumentMock.Setup(d => d.CreationDateTime).Returns(new DateTime(2022, 08, 17, 17, 45, 32, 876));
                    expectedDocumentMock.Setup(d => d.Claims).Returns(
                    [
                        new UserClaim("ether_address", "0xDe87768A7B118aAA23Cd3552E4AD34B8F4566Bde"),
                        new UserClaim("ether_prev_addresses", "[\"0xd6cEd4963410D5B99a90510Fe2DcAED517EAa03C\"]")
                    ]);
                    expectedDocumentMock.Setup(d => d.Email).Returns("asdfg@sas.so");
                    expectedDocumentMock.Setup(d => d.EtherAddress).Returns("0xDe87768A7B118aAA23Cd3552E4AD34B8F4566Bde");
                    expectedDocumentMock.Setup(d => d.EtherPreviousAddresses).Returns(["0xd6cEd4963410D5B99a90510Fe2DcAED517EAa03C"]);
                    {
                        var userMock = new Mock<UserWeb2>();
                        userMock.Setup(u => u.Id).Returns("61cd0b616b33d8785b9d34cf");
                        expectedDocumentMock.Setup(d => d.InvitedBy).Returns(userMock.Object);
                    }
                    expectedDocumentMock.Setup(d => d.InvitedByAdmin).Returns(true);
                    expectedDocumentMock.Setup(d => d.LastLoginDateTime).Returns(new DateTime(2022, 09, 17, 14, 22, 06, 601));
                    expectedDocumentMock.Setup(d => d.MaxAllowedClients).Returns(UserBase.DefaultMaxAllowedClients);
                    expectedDocumentMock.Setup(d => d.NormalizedEmail).Returns("ASDFG@SAS.SO");
                    expectedDocumentMock.Setup(d => d.NormalizedUsername).Returns("ASDFGA");
                    expectedDocumentMock.Setup(d => d.PhoneNumber).Returns("123-456-7890");
                    expectedDocumentMock.Setup(d => d.PhoneNumberConfirmed).Returns(true);
                    {
                        var roleMock = new Mock<Role>();
                        roleMock.Setup(r => r.Id).Returns("61cdeb616b13d8985b3d688d");
                        expectedDocumentMock.Setup(d => d.Roles).Returns([roleMock.Object]);
                    }
                    expectedDocumentMock.Setup(d => d.SecurityStamp).Returns("ZNB7TIG3GMD6SAWRIYPN2ST5Q734O7DV");
                    expectedDocumentMock.Setup(d => d.SharedInfoId).Returns("62fd293ca12c0fd52db29c8c");
                    expectedDocumentMock.Setup(d => d.Username).Returns("asdfga");
                    expectedDocumentMock.Setup(d => d.AccessFailedCount).Returns(5);
                    expectedDocumentMock.Setup(d => d.AuthenticatorKey).Returns("SVKPXJRGFOAJGSJZOUUUJSSZZTYBXDPL");
                    expectedDocumentMock.Setup(d => d.EtherManagedPrivateKey).Returns("e883fcbe10b59d63dc7f1bbed29dbd81f17a03fc65ea7d87461f45a6dfe76d0c");
                    expectedDocumentMock.Setup(d => d.EtherLoginAddress).Returns("0xfeF78523191CC15e287b3F7ABFbd0c3d621f053b");
                    expectedDocumentMock.Setup(d => d.PasswordHash).Returns("AQAAAAEAACcQAAAAELAZKxcX4rTHtVo4ZBbpZdaxfsiB4xaOM/3mEO86iq8vdUPtglbwyk7qa2jDajBWUA==");
                    expectedDocumentMock.Setup(d => d.TwoFactorEnabled).Returns(true);
                    expectedDocumentMock.Setup(d => d.TwoFactorRecoveryCodes).Returns(["q56k5c6s", "l3rcwj6v", "8y9b5lqv"]);

                    tests.Add(new(sourceDocument, expectedDocumentMock.Object));
                }

                // "7d8804ab-217c-476a-a47f-977fe693fce3" - v0.3.22 - UserWeb3
                {
                    var sourceDocument =
                        @"{
                            ""_id"" : ObjectId(""62fd293ca12c0fd52db29c8d""),
                            ""_m"" : ""7d8804ab-217c-476a-a47f-977fe693fce3"",
                            ""_t"" : ""UserWeb3"",
                            ""CreationDateTime"" : ISODate(""2022-08-17T17:45:32.876+0000""),
                            ""Claims"" : [
                                {
                                    ""_m"" : ""f7831985-dc0c-439f-b118-d7c511619a87"",
                                    ""Type"" : ""ether_address"",
                                    ""Value"" : ""0xDe87768A7B118aAA23Cd3552E4AD34B8F4566Bde""
                                },
                                {
                                    ""_m"" : ""f7831985-dc0c-439f-b118-d7c511619a87"",
                                    ""Type"" : ""ether_prev_addresses"",
                                    ""Value"" : ""[\""0xd6cEd4963410D5B99a90510Fe2DcAED517EAa03C\""]""
                                }
                            ],
                            ""Email"" : ""asdfg@sas.so"",
                            ""EtherAddress"" : ""0xDe87768A7B118aAA23Cd3552E4AD34B8F4566Bde"",
                            ""EtherPreviousAddresses"" : [
                                ""0xd6cEd4963410D5B99a90510Fe2DcAED517EAa03C""
                            ],
                            ""InvitedBy"" : {
                                ""_m"" : ""a1976133-bb21-40af-b6de-3a0f7f7dc676"",
                                ""_t"" : ""UserWeb2"",
                                ""_id"" : ObjectId(""61cd0b616b33d8785b9d34cf"")
                            },
                            ""InvitedByAdmin"" : true,
                            ""LastLoginDateTime"" : ISODate(""2022-09-17T14:22:06.601+0000""),
                            ""NormalizedEmail"" : ""ASDFG@SAS.SO"",
                            ""NormalizedUsername"" : ""ASDFGA"",
                            ""PhoneNumber"" : ""123-456-7890"",
                            ""PhoneNumberConfirmed"" : true,
                            ""Roles"" : [
                                {
                                    ""_m"" : ""cc9c6902-edd5-491d-acb5-07ca02fa71d0"",
                                    ""_id"" : ObjectId(""61cdeb616b13d8985b3d688d""),
                                    ""NormalizedName"" : ""ADMINISTRATOR""
                                }
                            ],
                            ""SecurityStamp"" : ""ZNB7TIG3GMD6SAWRIYPN2ST5Q734O7DV"",
                            ""SharedInfoId"" : ""62fd293ca12c0fd52db29c8c"",
                            ""Username"" : ""asdfga""
                        }";

                    var expectedDocumentMock = new Mock<UserWeb3>();
                    expectedDocumentMock.Setup(d => d.Id).Returns("62fd293ca12c0fd52db29c8d");
                    expectedDocumentMock.Setup(d => d.CreationDateTime).Returns(new DateTime(2022, 08, 17, 17, 45, 32, 876));
                    expectedDocumentMock.Setup(d => d.Claims).Returns(
                    [
                        new UserClaim("ether_address", "0xDe87768A7B118aAA23Cd3552E4AD34B8F4566Bde"),
                        new UserClaim("ether_prev_addresses", "[\"0xd6cEd4963410D5B99a90510Fe2DcAED517EAa03C\"]")
                    ]);
                    expectedDocumentMock.Setup(d => d.Email).Returns("asdfg@sas.so");
                    expectedDocumentMock.Setup(d => d.EtherAddress).Returns("0xDe87768A7B118aAA23Cd3552E4AD34B8F4566Bde");
                    expectedDocumentMock.Setup(d => d.EtherPreviousAddresses).Returns(["0xd6cEd4963410D5B99a90510Fe2DcAED517EAa03C"]);
                    {
                        var userMock = new Mock<UserWeb2>();
                        userMock.Setup(u => u.Id).Returns("61cd0b616b33d8785b9d34cf");
                        expectedDocumentMock.Setup(d => d.InvitedBy).Returns(userMock.Object);
                    }
                    expectedDocumentMock.Setup(d => d.InvitedByAdmin).Returns(true);
                    expectedDocumentMock.Setup(d => d.LastLoginDateTime).Returns(new DateTime(2022, 09, 17, 14, 22, 06, 601));
                    expectedDocumentMock.Setup(d => d.MaxAllowedClients).Returns(UserBase.DefaultMaxAllowedClients);
                    expectedDocumentMock.Setup(d => d.NormalizedEmail).Returns("ASDFG@SAS.SO");
                    expectedDocumentMock.Setup(d => d.NormalizedUsername).Returns("ASDFGA");
                    expectedDocumentMock.Setup(d => d.PhoneNumber).Returns("123-456-7890");
                    expectedDocumentMock.Setup(d => d.PhoneNumberConfirmed).Returns(true);
                    {
                        var roleMock = new Mock<Role>();
                        roleMock.Setup(r => r.Id).Returns("61cdeb616b13d8985b3d688d");
                        expectedDocumentMock.Setup(d => d.Roles).Returns([roleMock.Object]);
                    }
                    expectedDocumentMock.Setup(d => d.SecurityStamp).Returns("ZNB7TIG3GMD6SAWRIYPN2ST5Q734O7DV");
                    expectedDocumentMock.Setup(d => d.SharedInfoId).Returns("62fd293ca12c0fd52db29c8c");
                    expectedDocumentMock.Setup(d => d.Username).Returns("asdfga");

                    tests.Add(new(sourceDocument, expectedDocumentMock.Object));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        public static IEnumerable<object[]> Web3LoginTokenDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<Web3LoginToken, SsoDbContext>>();

                // "150f4cdf-099a-4195-a145-45f1f9eda60c" - v0.3.22
                {
                    var sourceDocument =
                        @"{
                            ""_id"" : ObjectId(""622b5dbb7101122b1d9f0e7d""),
                            ""_m"" : ""150f4cdf-099a-4195-a145-45f1f9eda60c"",
                            ""CreationDateTime"" : ISODate(""2022-03-11T14:33:31.567+0000""),
                            ""Code"" : ""Vu6pBdFzjm"",
                            ""EtherAddress"" : ""0x75691aD5a48d8f7A9f13a0Eab1B89E19eDFcA4d9""
                        }";

                    var expectedDocumentMock = new Mock<Web3LoginToken>();
                    expectedDocumentMock.Setup(d => d.Id).Returns("622b5dbb7101122b1d9f0e7d");
                    expectedDocumentMock.Setup(d => d.CreationDateTime).Returns(new DateTime(2022, 03, 11, 14, 33, 31, 567));
                    expectedDocumentMock.Setup(d => d.Code).Returns("Vu6pBdFzjm");
                    expectedDocumentMock.Setup(d => d.EtherAddress).Returns("0x75691aD5a48d8f7A9f13a0Eab1B89E19eDFcA4d9");

                    tests.Add(new(sourceDocument, expectedDocumentMock.Object));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        // Tests.
        [Theory, MemberData(nameof(ClientAppDeserializationTests))]
        public void Deserialize_WithClientAppDocument_ReturnsExpectedModel(DeserializationTestElement<ClientApp, SsoDbContext> testElement)
        {
            ArgumentNullException.ThrowIfNull(testElement);

            // Arrange.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<ClientApp>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.AccessTokenType, result.AccessTokenType);
            Assert.Equal(testElement.ExpectedModel.AllowedCorsOrigins, result.AllowedCorsOrigins);
            Assert.Equal(testElement.ExpectedModel.AllowedGrantTypes, result.AllowedGrantTypes);
            Assert.Equal(testElement.ExpectedModel.AllowedScopes, result.AllowedScopes);
            Assert.Equal(testElement.ExpectedModel.AllowOfflineAccess, result.AllowOfflineAccess);
            Assert.Equal(testElement.ExpectedModel.AlwaysIncludeUserClaimsInIdToken, result.AlwaysIncludeUserClaimsInIdToken);
            Assert.Equal(testElement.ExpectedModel.ClientId, result.ClientId);
            Assert.Equal(testElement.ExpectedModel.ClientName, result.ClientName);
            Assert.Equal(testElement.ExpectedModel.ClientType, result.ClientType);
            Assert.Equal(testElement.ExpectedModel.Description, result.Description);
            Assert.Equal(testElement.ExpectedModel.Enabled, result.Enabled);
            Assert.Equal(testElement.ExpectedModel.Owner, result.Owner, EntityModelEqualityComparer.Instance);
            Assert.Equal(testElement.ExpectedModel.PostLogoutRedirectUris, result.PostLogoutRedirectUris);
            Assert.Equal(testElement.ExpectedModel.RedirectUris, result.RedirectUris);
            Assert.Equal(testElement.ExpectedModel.RefreshTokenUsage, result.RefreshTokenUsage);
            Assert.Equal(testElement.ExpectedModel.RequireClientSecret, result.RequireClientSecret);
            Assert.Equal(testElement.ExpectedModel.RequireConsent, result.RequireConsent);
            Assert.Equal(testElement.ExpectedModel.RequirePkce, result.RequirePkce);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.ClientId);
            Assert.NotNull(result.ClientName);
            Assert.NotNull(result.Owner);

            var expectedSecrets = testElement.ExpectedModel.ClientSecrets.ToList();
            var resultSecrets = result.ClientSecrets.ToList();
            Assert.Equal(expectedSecrets.Count, resultSecrets.Count);
            for (var i = 0; i < expectedSecrets.Count; i++)
            {
                Assert.Equal(expectedSecrets[i].Value, resultSecrets[i].Value);
                Assert.Equal(expectedSecrets[i].Description, resultSecrets[i].Description);
                Assert.Equal(expectedSecrets[i].Expiration, resultSecrets[i].Expiration);
            }
        }

        [Theory, MemberData(nameof(AlphaPassRequestDeserializationTests))]
        public void Deserialize_WithAlphaPassRequestDocument_ReturnsExpectedModel(DeserializationTestElement<AlphaPassRequest, SsoDbContext> testElement)
        {
            ArgumentNullException.ThrowIfNull(testElement);

            // Arrange.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<AlphaPassRequest>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.IsEmailConfirmed, result.IsEmailConfirmed);
            Assert.Equal(testElement.ExpectedModel.IsInvitationSent, result.IsInvitationSent);
            Assert.Equal(testElement.ExpectedModel.NormalizedEmail, result.NormalizedEmail);
            Assert.Equal(testElement.ExpectedModel.Secret, result.Secret);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.NormalizedEmail);
            Assert.NotNull(result.Secret);
        }

        [Theory, MemberData(nameof(ApiKeyDeserializationTests))]
        public void Deserialize_WithApiKeyDocument_ReturnsExpectedModel(DeserializationTestElement<ApiKey, SsoDbContext> testElement)
        {
            ArgumentNullException.ThrowIfNull(testElement);

            // Arrange.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<ApiKey>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.EndOfLife, result.EndOfLife);
            Assert.Equal(testElement.ExpectedModel.KeyHash, result.KeyHash);
            Assert.Equal(testElement.ExpectedModel.Label, result.Label);
            Assert.Equal(testElement.ExpectedModel.Owner, result.Owner, EntityModelEqualityComparer.Instance);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.KeyHash);
            Assert.NotNull(result.Label);
            Assert.NotNull(result.Owner);
        }

        [Theory, MemberData(nameof(DailyStatsDeserializationTests))]
        public void Deserialize_WithDailyStatsDocument_ReturnsExpectedModel(DeserializationTestElement<DailyStats, SsoDbContext> testElement)
        {
            ArgumentNullException.ThrowIfNull(testElement);

            // Arrange.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<DailyStats>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.ActiveUsersInLast30Days, result.ActiveUsersInLast30Days);
            Assert.Equal(testElement.ExpectedModel.ActiveUsersInLast60Days, result.ActiveUsersInLast60Days);
            Assert.Equal(testElement.ExpectedModel.ActiveUsersInLast180Days, result.ActiveUsersInLast180Days);
            Assert.NotNull(result.Id);
        }

        [Theory, MemberData(nameof(InvitationDeserializationTests))]
        public void Deserialize_WithInvitationDocument_ReturnsExpectedModel(DeserializationTestElement<Invitation, SsoDbContext> testElement)
        {
            ArgumentNullException.ThrowIfNull(testElement);

            // Arrange.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<Invitation>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.Code, result.Code);
            Assert.Equal(testElement.ExpectedModel.Emitter, result.Emitter, EntityModelEqualityComparer.Instance);
            Assert.Equal(testElement.ExpectedModel.EndLife, result.EndLife);
            Assert.Equal(testElement.ExpectedModel.IsFromAdmin, result.IsFromAdmin);
            Assert.Equal(testElement.ExpectedModel.IsSingleUse, result.IsSingleUse);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Code);
        }

        [Theory, MemberData(nameof(RoleDeserializationTests))]
        public void Deserialize_WithRoleDocument_ReturnsExpectedModel(DeserializationTestElement<Role, SsoDbContext> testElement)
        {
            ArgumentNullException.ThrowIfNull(testElement);

            // Arrange.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<Role>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.Name, result.Name);
            Assert.Equal(testElement.ExpectedModel.NormalizedName, result.NormalizedName);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Name);
            Assert.NotNull(result.NormalizedName);
        }

        [Theory, MemberData(nameof(UserDeserializationTests))]
        public void Deserialize_WithUserDocument_ReturnsExpectedModel(DeserializationTestElement<UserBase, SsoDbContext> testElement)
        {
            ArgumentNullException.ThrowIfNull(testElement);

            // Arrange.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<UserBase>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.Email, result.Email);
            Assert.Equal(testElement.ExpectedModel.EtherAddress, result.EtherAddress);
            Assert.Equal(testElement.ExpectedModel.EtherPreviousAddresses, result.EtherPreviousAddresses);
            Assert.Equal(testElement.ExpectedModel.InvitedBy, result.InvitedBy, EntityModelEqualityComparer.Instance);
            Assert.Equal(testElement.ExpectedModel.InvitedByAdmin, result.InvitedByAdmin);
            Assert.Equal(testElement.ExpectedModel.LastLoginDateTime, result.LastLoginDateTime);
            Assert.Equal(testElement.ExpectedModel.MaxAllowedClients, result.MaxAllowedClients);
            Assert.Equal(testElement.ExpectedModel.NormalizedEmail, result.NormalizedEmail);
            Assert.Equal(testElement.ExpectedModel.NormalizedUsername, result.NormalizedUsername);
            Assert.Equal(testElement.ExpectedModel.PhoneNumber, result.PhoneNumber);
            Assert.Equal(testElement.ExpectedModel.PhoneNumberConfirmed, result.PhoneNumberConfirmed);
            Assert.Equal(testElement.ExpectedModel.Roles, result.Roles, EntityModelEqualityComparer.Instance);
            Assert.Equal(testElement.ExpectedModel.SecurityStamp, result.SecurityStamp);
            Assert.Equal(testElement.ExpectedModel.SharedInfoId, result.SharedInfoId);
            Assert.Equal(testElement.ExpectedModel.Username, result.Username);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.EtherPreviousAddresses);
            Assert.NotNull(result.NormalizedUsername);
            Assert.NotNull(result.Roles);
            Assert.NotNull(result.SecurityStamp);
            Assert.NotNull(result.SharedInfoId);
            Assert.NotNull(result.Username);

            switch (testElement.ExpectedModel)
            {
                case UserWeb2 expectedWeb2:
                    var resultWeb2 = (UserWeb2)result;

                    Assert.Equal(expectedWeb2.AccessFailedCount, resultWeb2.AccessFailedCount);
                    Assert.Equal(expectedWeb2.AuthenticatorKey, resultWeb2.AuthenticatorKey);
                    Assert.Equal(expectedWeb2.EtherManagedPrivateKey, resultWeb2.EtherManagedPrivateKey);
                    Assert.Equal(expectedWeb2.EtherLoginAddress, resultWeb2.EtherLoginAddress);
                    Assert.Equal(expectedWeb2.PasswordHash, resultWeb2.PasswordHash);
                    Assert.Equal(expectedWeb2.TwoFactorEnabled, resultWeb2.TwoFactorEnabled);
                    Assert.Equal(expectedWeb2.TwoFactorRecoveryCodes, resultWeb2.TwoFactorRecoveryCodes);
                    Assert.NotNull(resultWeb2.EtherManagedPrivateKey);
                    break;
                case UserWeb3 expectedWeb3: break;
                default: throw new InvalidOperationException();
            }
        }

        [Theory, MemberData(nameof(Web3LoginTokenDeserializationTests))]
        public void Deserialize_WithWeb3LoginTokenDocument_ReturnsExpectedModel(DeserializationTestElement<Web3LoginToken, SsoDbContext> testElement)
        {
            ArgumentNullException.ThrowIfNull(testElement);

            // Arrange.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<Web3LoginToken>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.Code, result.Code);
            Assert.Equal(testElement.ExpectedModel.EtherAddress, result.EtherAddress);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Code);
        }
    }
}
