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

using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Helpers;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.ClientAppAgg;
using Etherna.SSOServer.Models;
using Etherna.SSOServer.Pages;
using Etherna.SSOServer.Services.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage.Developer
{
    public class ClientEditModel(
        ILogger<ClientEditModel> logger,
        ISsoDbContext ssoDbContext,
        UserManager<UserBase> userManager)
        : StatusMessagePageModel
    {
        // Models.
        public class InputModel
        {
            public InputModel() { }
            public InputModel(ClientApp clientApp)
            {
                ArgumentNullException.ThrowIfNull(clientApp);

                ClientName = clientApp.ClientName;
                Description = clientApp.Description;
                Enabled = clientApp.Enabled;
                AllowedScopes = clientApp.AllowedScopes.ToList();
                RedirectUris = string.Join("\n", clientApp.RedirectUris);
                PostLogoutRedirectUris = string.Join("\n", clientApp.PostLogoutRedirectUris);
                AllowedCorsOrigins = string.Join("\n", clientApp.AllowedCorsOrigins);
            }

            [Required]
            [MaxLength(ClientApp.ClientNameMaxLength)]
            [Display(Name = "Client name")]
            public string ClientName { get; set; } = default!;

            [MaxLength(ClientApp.DescriptionMaxLength)]
            [Display(Name = "Description")]
            public string? Description { get; set; }

            [Display(Name = "Enabled")]
            public bool Enabled { get; set; } = true;

            [Display(Name = "Allowed scopes")]
            [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Setter needed for model binding")]
            public List<string> AllowedScopes { get; set; } = new();

            [Display(Name = "Redirect URIs (one per line)")]
            public string? RedirectUris { get; set; }

            [Display(Name = "Post-logout redirect URIs (one per line)")]
            public string? PostLogoutRedirectUris { get; set; }

            [Display(Name = "Allowed CORS origins (one per line)")]
            public string? AllowedCorsOrigins { get; set; }
        }

        // Properties.
        public string ClientAppId { get; private set; } = null!;
        public string ClientId { get; private set; } = null!;
        public ClientAppType ClientType { get; private set; }
        public bool RequireClientSecret { get; private set; }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        // Methods.
        public async Task<IActionResult> OnGetAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

            var clientApp = await ssoDbContext.ClientApps.FindOneAsync(id);

            var userId = userManager.GetUserId(User);
            if (clientApp.ClientType == ClientAppType.Custom || clientApp.Owner.Id != userId)
                return Forbid();

            ClientAppId = id;
            ClientId = clientApp.ClientId;
            ClientType = clientApp.ClientType;
            RequireClientSecret = clientApp.RequireClientSecret;
            Input = new InputModel(clientApp);

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

            var clientApp = await ssoDbContext.ClientApps.FindOneAsync(id);

            var userId = userManager.GetUserId(User);
            if (clientApp.ClientType == ClientAppType.Custom || clientApp.Owner.Id != userId)
                return Forbid();

            ClientAppId = id;
            ClientId = clientApp.ClientId;
            ClientType = clientApp.ClientType;
            RequireClientSecret = clientApp.RequireClientSecret;

            if (!ModelState.IsValid)
                return Page();

            // Validate scopes are within allowed set.
            var invalidScopes = Input.AllowedScopes
                .Where(s => !ClientApp.DeveloperAllowedScopes.Contains(s))
                .ToList();
            if (invalidScopes.Count > 0)
            {
                ModelState.AddModelError(string.Empty,
                    $"Invalid scopes: {string.Join(", ", invalidScopes)}");
                return Page();
            }

            var redirectUris = ParseMultilineField(Input.RedirectUris);
            var postLogoutRedirectUris = ParseMultilineField(Input.PostLogoutRedirectUris);
            var allowedCorsOrigins = ParseMultilineField(Input.AllowedCorsOrigins);

            var allowCustomSchemes = ClientType == ClientAppType.NativeApp;
            if (redirectUris.Any(u => !UriHelper.IsValidRedirectUri(u, allowCustomSchemes)))
            {
                var hint = allowCustomSchemes
                    ? "Use https://, http://localhost, or a custom scheme (e.g. myapp://)."
                    : "Use https:// or http://localhost, with a valid domain name.";
                ModelState.AddModelError("Input.RedirectUris",
                    $"One or more redirect URIs are invalid. {hint}");
                return Page();
            }
            if (postLogoutRedirectUris.Any(u => !UriHelper.IsValidRedirectUri(u, allowCustomSchemes)))
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

            clientApp.SetInfo(Input.ClientName, Input.Description);
            clientApp.Enabled = Input.Enabled;
            clientApp.SetAllowedScopes(Input.AllowedScopes);
            clientApp.RedirectUris = redirectUris;
            clientApp.PostLogoutRedirectUris = postLogoutRedirectUris;
            clientApp.AllowedCorsOrigins = allowedCorsOrigins;

            await ssoDbContext.SaveChangesAsync();
            logger.ClientAppUpdated(userId!, clientApp.ClientId);

            StatusMessage = new StatusMessage("Client saved successfully.");
            return RedirectToPage(new { id });
        }

        // Helpers.
        private static string[] ParseMultilineField(string? value) =>
            (value ?? "")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(s => s.Length > 0)
                .ToArray();
    }
}
