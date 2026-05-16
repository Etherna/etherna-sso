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
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Models;
using Etherna.SSOServer.Services.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class ClientModel(
        ILogger<ClientModel> logger,
        ISsoDbContext ssoDbContext,
        UserManager<UserBase> userManager)
        : PageModel
    {
        // Model.
        public class InputModel
        {
            // Constructors.
            public InputModel() { }
            public InputModel(ClientApp clientApp)
            {
                ArgumentNullException.ThrowIfNull(clientApp);

                ClientId = clientApp.ClientId;
                ClientName = clientApp.ClientName;
                Description = clientApp.Description;
                Enabled = clientApp.Enabled;
                AllowedGrantTypes = string.Join(", ", clientApp.AllowedGrantTypes);
                RequireClientSecret = clientApp.RequireClientSecret;
                RequirePkce = clientApp.RequirePkce;
                AllowOfflineAccess = clientApp.AllowOfflineAccess;
                AlwaysIncludeUserClaimsInIdToken = clientApp.AlwaysIncludeUserClaimsInIdToken;
                RequireConsent = clientApp.RequireConsent;
                AccessTokenType = clientApp.AccessTokenType;
                RefreshTokenUsage = clientApp.RefreshTokenUsage;
                AllowedScopes = string.Join("\n", clientApp.AllowedScopes);
                RedirectUris = string.Join("\n", clientApp.RedirectUris);
                PostLogoutRedirectUris = string.Join("\n", clientApp.PostLogoutRedirectUris);
                AllowedCorsOrigins = string.Join("\n", clientApp.AllowedCorsOrigins);
            }

            // Properties.
            [Required]
            [Display(Name = "Client ID")]
            public string ClientId { get; set; } = null!;

            [Required]
            [MaxLength(ClientApp.ClientNameMaxLength)]
            [Display(Name = "Client name")]
            public string ClientName { get; set; } = null!;

            [MaxLength(ClientApp.DescriptionMaxLength)]
            [Display(Name = "Description")]
            public string? Description { get; set; }

            [Display(Name = "Enabled")]
            public bool Enabled { get; set; } = true;

            [Required]
            [Display(Name = "Allowed grant types")]
            public string AllowedGrantTypes { get; set; } = GrantType.AuthorizationCode;

            [Display(Name = "Require client secret")]
            public bool RequireClientSecret { get; set; }

            [Display(Name = "Require PKCE")]
            public bool RequirePkce { get; set; }

            [Display(Name = "Allow offline access")]
            public bool AllowOfflineAccess { get; set; }

            [Display(Name = "Always include user claims in ID token")]
            public bool AlwaysIncludeUserClaimsInIdToken { get; set; }

            [Display(Name = "Require consent")]
            public bool RequireConsent { get; set; }

            [Display(Name = "Access token type")]
            public AccessTokenType AccessTokenType { get; set; }

            [Display(Name = "Refresh token usage")]
            public TokenUsage RefreshTokenUsage { get; set; }

            [Display(Name = "Allowed scopes (one per line)")]
            public string? AllowedScopes { get; set; }

            [Display(Name = "Redirect URIs (one per line)")]
            public string? RedirectUris { get; set; }

            [Display(Name = "Post-logout redirect URIs (one per line)")]
            public string? PostLogoutRedirectUris { get; set; }

            [Display(Name = "Allowed CORS origins (one per line)")]
            public string? AllowedCorsOrigins { get; set; }
        }

        // Properties.
        public string? ClientAppId { get; private set; }

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public bool IsNewClient => ClientAppId is null;

        public StatusMessage? StatusMessage { get; set; }

        // Methods.
        public async Task OnGetAsync(string? id)
        {
            ClientAppId = id;

            if (id is not null)
            {
                var clientApp = await ssoDbContext.ClientApps.FindOneAsync(id);
                Input = new InputModel(clientApp);
            }
            else
            {
                Input = new InputModel();
            }
        }

        public async Task<IActionResult> OnPostSaveAsync(string? id)
        {
            ClientAppId = id;

            if (!ModelState.IsValid)
                return Page();

            var allowedGrantTypes = ParseMultilineField(Input.AllowedGrantTypes)
                .SelectMany(g => g.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Distinct()
                .ToArray();
            var allowedScopes = ParseMultilineField(Input.AllowedScopes);
            var redirectUris = ParseMultilineField(Input.RedirectUris);
            var postLogoutRedirectUris = ParseMultilineField(Input.PostLogoutRedirectUris);
            var allowedCorsOrigins = ParseMultilineField(Input.AllowedCorsOrigins);

            ClientApp clientApp;
            if (id is null) // create
            {
                var currentUser = await userManager.GetUserAsync(User) ??
                    throw new InvalidOperationException("Unable to load current user.");

                clientApp = new ClientApp(
                    Input.ClientId,
                    Input.ClientName,
                    Input.Description,
                    currentUser,
                    allowedGrantTypes,
                    Input.RequireClientSecret,
                    Input.RequirePkce,
                    Input.AllowOfflineAccess,
                    Input.AlwaysIncludeUserClaimsInIdToken,
                    Input.RequireConsent,
                    Input.AccessTokenType,
                    Input.RefreshTokenUsage,
                    allowedScopes,
                    redirectUris,
                    postLogoutRedirectUris,
                    allowedCorsOrigins)
                {
                    Enabled = Input.Enabled
                };

                await ssoDbContext.ClientApps.CreateAsync(clientApp);
                logger.ClientAppCreatedByAdmin(currentUser.Id, clientApp.ClientId);
            }
            else // update
            {
                clientApp = await ssoDbContext.ClientApps.FindOneAsync(id);

                clientApp.SetInfo(Input.ClientName, Input.Description);
                clientApp.Enabled = Input.Enabled;
                clientApp.AllowedGrantTypes = allowedGrantTypes;
                clientApp.RequireClientSecret = Input.RequireClientSecret;
                clientApp.RequirePkce = Input.RequirePkce;
                clientApp.AllowOfflineAccess = Input.AllowOfflineAccess;
                clientApp.AlwaysIncludeUserClaimsInIdToken = Input.AlwaysIncludeUserClaimsInIdToken;
                clientApp.RequireConsent = Input.RequireConsent;
                clientApp.AccessTokenType = Input.AccessTokenType;
                clientApp.RefreshTokenUsage = Input.RefreshTokenUsage;
                clientApp.SetAllowedScopes(allowedScopes);
                clientApp.RedirectUris = redirectUris;
                clientApp.PostLogoutRedirectUris = postLogoutRedirectUris;
                clientApp.AllowedCorsOrigins = allowedCorsOrigins;

                await ssoDbContext.SaveChangesAsync();
                logger.ClientAppUpdatedByAdmin(userManager.GetUserId(User)!, clientApp.ClientId);
            }

            StatusMessage = new StatusMessage("Client saved successfully.");
            return RedirectToPage(new { id = clientApp.Id });
        }

        // Helpers.
        private static string[] ParseMultilineField(string? value) =>
            (value ?? "")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(s => s.Length > 0)
                .ToArray();
    }
}
