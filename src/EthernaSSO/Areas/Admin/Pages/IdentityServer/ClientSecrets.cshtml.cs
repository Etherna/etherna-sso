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
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class ClientSecretsModel(
        ISsoDbContext context,
        ILogger<ClientSecretsModel> logger,
        UserManager<UserBase> userManager)
        : StatusMessagePageModel
    {
        // Models.
        public class SecretDto
        {
            public SecretDto(ClientSecret secret)
            {
                ArgumentNullException.ThrowIfNull(secret);

                Description = secret.Description;
                Created = secret.Created;
                Expiration = secret.Expiration;
                IsExpired = secret.IsExpired;
                HashedValue = secret.Value;
            }

            public string? Description { get; }
            public DateTime Created { get; }
            public DateTime? Expiration { get; }
            public bool IsExpired { get; }
            public string HashedValue { get; }
        }

        public class InputModel
        {
            [Display(Name = "Description")]
            public string? Description { get; set; }

            [DataType(DataType.DateTime)]
            [Display(Name = "Expiration (UTC)")]
            public DateTime? Expiration { get; set; }
        }

        // Properties.
        public string Id { get; private set; } = null!;

        [Display(Name = "Client Id")]
        public string ClientId { get; private set; } = null!;

        [Display(Name = "Client Name")]
        public string ClientName { get; private set; } = null!;

        public IEnumerable<SecretDto> Secrets { get; private set; } = [];

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        // Methods.
        public async Task OnGetAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

            await InitializeAsync(id);
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

            if (!ModelState.IsValid)
            {
                await InitializeAsync(id);
                return Page();
            }

            var clientApp = await context.ClientApps.FindOneAsync(id);

            var plainSecret = ClientApp.GeneratePlainSecret();
            var hashedSecret = ClientApp.HashSecret(plainSecret);
            var secret = new ClientSecret(hashedSecret, Input.Description, Input.Expiration);
            clientApp.AddSecret(secret);
            await context.SaveChangesAsync();
            logger.ClientAppSecretAddedByAdmin(userManager.GetUserId(User)!, clientApp.ClientId);

            StatusMessage = new StatusMessage(
                $"Your new client secret: {plainSecret}\nCopy it now - it won't be shown again.",
                StatusMessageType.Warning);

            await InitializeAsync(id);
            return Page();
        }

        // Helpers.
        private async Task InitializeAsync(string id)
        {
            Id = id;
            var clientApp = await context.ClientApps.FindOneAsync(id);
            ClientId = clientApp.ClientId;
            ClientName = clientApp.ClientName;
            Secrets = clientApp.ClientSecrets.Select(s => new SecretDto(s));
        }
    }
}
