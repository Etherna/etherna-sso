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

using Etherna.Authentication;
using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Configs.Metrics;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Helpers;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.ClientAppAgg;
using Etherna.SSOServer.Models;
using Etherna.SSOServer.Services.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage.Developer
{
    public class ClientCreateModel(
        ILogger<ClientCreateModel> logger,
        ISsoDbContext ssoDbContext,
        UserManager<UserBase> userManager)
        : PageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            [Display(Name = "Application type")]
            public ClientAppType ClientType { get; set; }

            [Required]
            [StringLength(ClientApp.ClientNameMaxLength, MinimumLength = 1)]
            [Display(Name = "Application name")]
            public string ClientName { get; set; } = null!;

            [StringLength(ClientApp.DescriptionMaxLength)]
            [Display(Name = "Description")]
            public string? Description { get; set; }

            [Display(Name = "Redirect URIs")]
            [DataType(DataType.MultilineText)]
            public string? RedirectUris { get; set; }

            [Display(Name = "Post-logout redirect URIs")]
            [DataType(DataType.MultilineText)]
            public string? PostLogoutRedirectUris { get; set; }

            [Display(Name = "Allowed CORS origins")]
            [DataType(DataType.MultilineText)]
            public string? AllowedCorsOrigins { get; set; }

            [Display(Name = "Scopes")]
            [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Setter needed for model binding")]
            public List<string> AllowedScopes { get; set; } = new();
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public StatusMessage? StatusMessage { get; set; }
        public string? GeneratedClientId { get; set; }
        public string? GeneratedSecret { get; set; }

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User) ??
                throw new InvalidOperationException();

            var isAdmin = await userManager.IsInRoleAsync(user, Role.AdministratorName);
            if (!isAdmin)
            {
                if (!await userManager.IsEmailConfirmedAsync(user))
                {
                    StatusMessage = new StatusMessage(
                        "A verified email address is required to create client applications. Please verify your email first.",
                        StatusMessageType.Error);
                }
                else if (user.MaxAllowedClients <= 0)
                {
                    StatusMessage = new StatusMessage(
                        "You are not allowed to create client applications. Please contact an administrator.",
                        StatusMessageType.Error);
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await userManager.GetUserAsync(User) ??
                throw new InvalidOperationException();

            var isAdmin = await userManager.IsInRoleAsync(user, Role.AdministratorName);

            // Check email confirmation (admins bypass).
            if (!isAdmin && !await userManager.IsEmailConfirmedAsync(user))
            {
                StatusMessage = new StatusMessage(
                    "A verified email address is required to create client applications. Please verify your email first.",
                    StatusMessageType.Error);
                return Page();
            }

            // Check max allowed clients (admins bypass the limit).
            if (!isAdmin)
            {
                var existingClients = await ssoDbContext.ClientApps.QueryElementsAsync(elements =>
                    elements.Where(c => c.Owner.Id == user.Id && c.ClientType != ClientAppType.Custom)
                            .ToListAsync());

                if (existingClients.Count >= user.MaxAllowedClients)
                {
                    StatusMessage = new StatusMessage(
                        $"You have reached the maximum number of client applications ({user.MaxAllowedClients}).",
                        StatusMessageType.Error);
                    return Page();
                }
            }

            // Validate required fields based on client type.
            var redirectUris = ParseMultilineField(Input.RedirectUris);
            var postLogoutRedirectUris = ParseMultilineField(Input.PostLogoutRedirectUris);
            var allowedCorsOrigins = ParseMultilineField(Input.AllowedCorsOrigins);

            switch (Input.ClientType)
            {
                case ClientAppType.WebApp:
                    if (redirectUris.Count == 0)
                    {
                        ModelState.AddModelError("Input.RedirectUris",
                            "At least one redirect URI is required for web applications.");
                        return Page();
                    }
                    if (redirectUris.Any(u => !UriHelper.IsValidRedirectUri(u)))
                    {
                        ModelState.AddModelError("Input.RedirectUris",
                            "One or more redirect URIs are invalid. Use https:// or http://localhost, with a valid domain name.");
                        return Page();
                    }
                    if (postLogoutRedirectUris.Any(u => !UriHelper.IsValidRedirectUri(u)))
                    {
                        ModelState.AddModelError("Input.PostLogoutRedirectUris",
                            "One or more post-logout redirect URIs are invalid.");
                        return Page();
                    }
                    if (allowedCorsOrigins.Any(o => !UriHelper.IsValidCorsOrigin(o)))
                    {
                        ModelState.AddModelError("Input.AllowedCorsOrigins",
                            "One or more CORS origins are invalid. Use https://domain.com or http://localhost[:port] with no path.");
                        return Page();
                    }
                    break;

                case ClientAppType.NativeApp:
                    if (redirectUris.Count == 0)
                    {
                        ModelState.AddModelError("Input.RedirectUris",
                            "At least one redirect URI is required for native applications.");
                        return Page();
                    }
                    if (redirectUris.Any(u => !UriHelper.IsValidRedirectUri(u, allowCustomSchemes: true)))
                    {
                        ModelState.AddModelError("Input.RedirectUris",
                            "One or more redirect URIs are invalid. Use https://, http://localhost, or a custom scheme (e.g. myapp://).");
                        return Page();
                    }
                    if (postLogoutRedirectUris.Any(u => !UriHelper.IsValidRedirectUri(u, allowCustomSchemes: true)))
                    {
                        ModelState.AddModelError("Input.PostLogoutRedirectUris",
                            "One or more post-logout redirect URIs are invalid.");
                        return Page();
                    }
                    if (allowedCorsOrigins.Count == 0)
                    {
                        ModelState.AddModelError("Input.AllowedCorsOrigins",
                            "At least one CORS origin is required for native applications.");
                        return Page();
                    }
                    if (allowedCorsOrigins.Any(o => !UriHelper.IsValidCorsOrigin(o)))
                    {
                        ModelState.AddModelError("Input.AllowedCorsOrigins",
                            "One or more CORS origins are invalid. Use https://domain.com or http://localhost[:port] with no path.");
                        return Page();
                    }
                    break;

                case ClientAppType.ClientCredential:
                    // No URIs needed.
                    break;

                default:
                    StatusMessage = new StatusMessage("Invalid application type.", StatusMessageType.Error);
                    return Page();
            }

            // Ensure openid scope is always included.
            if (!Input.AllowedScopes.Contains(EthernaScopes.OpenId))
                Input.AllowedScopes.Insert(0, EthernaScopes.OpenId);

            // Validate scopes.
            foreach (var scope in Input.AllowedScopes)
            {
                if (!ClientApp.DeveloperAllowedScopes.Contains(scope))
                {
                    StatusMessage = new StatusMessage(
                        $"Scope '{scope}' is not allowed for developer clients.",
                        StatusMessageType.Error);
                    return Page();
                }
            }

            // Create client app.
            var clientApp = new ClientApp(
                Input.ClientName,
                Input.Description,
                Input.ClientType,
                user,
                Input.AllowedScopes,
                redirectUris.Count > 0 ? redirectUris : null,
                postLogoutRedirectUris.Count > 0 ? postLogoutRedirectUris : null,
                allowedCorsOrigins.Count > 0 ? allowedCorsOrigins : null);

            // Generate and add secret for WebApp and ClientCredential.
            string? plainSecret = null;
            if (Input.ClientType is ClientAppType.WebApp or ClientAppType.ClientCredential)
            {
                plainSecret = ClientApp.GeneratePlainSecret();
                var hashedSecret = ClientApp.HashSecret(plainSecret);
                clientApp.AddSecret(new ClientSecret(hashedSecret, "Auto-generated secret", null));
            }

            await ssoDbContext.ClientApps.CreateAsync(clientApp);
            logger.ClientAppCreated(user.Id, clientApp.ClientId);
            SsoMetrics.RecordClientAppCreated(Input.ClientType.ToString());

            // Set result properties.
            GeneratedClientId = clientApp.ClientId;
            GeneratedSecret = plainSecret;

            StatusMessage = plainSecret is not null
                ? new StatusMessage(
                    $"Client application created successfully! Your Client ID is {clientApp.ClientId}. " +
                    "A client secret has been generated. Please save it now -- it will not be shown again.",
                    StatusMessageType.Success)
                : new StatusMessage(
                    $"Client application created successfully! Your Client ID is {clientApp.ClientId}.",
                    StatusMessageType.Success);

            return Page();
        }

        // Helpers.
        private static List<string> ParseMultilineField(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return [];

            return value
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .ToList();
        }
    }
}
