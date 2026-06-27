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

using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.UserAgg;
using Etherna.SSOServer.Services.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class LegalModel(
        ILegalService legalService,
        UserManager<UserBase> userManager)
        : PageModel
    {
        // Models.
        public sealed record AcceptedDocument(
            string Name,
            string Version,
            DateTime AcceptanceDateTime,
            string? CurrentUrl,
            bool IsCurrentVersion);

        // Properties.
        public IReadOnlyList<AcceptedDocument> AcceptedDocuments { get; private set; } = [];

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            var requiredDocuments = legalService.RequiredDocuments;
            AcceptedDocuments = user.AcceptedLegalDocuments
                .OrderByDescending(a => a.AcceptanceDateTime)
                .Select(a =>
                {
                    var current = requiredDocuments.FirstOrDefault(d => d.Type == a.DocumentType);
                    var name = a.DocumentType switch
                    {
                        LegalDocumentType.PrivacyPolicy => "Privacy Policy",
                        LegalDocumentType.TermsOfService => "Terms of Service",
                        _ => a.DocumentType.ToString()
                    };
                    return new AcceptedDocument(
                        name,
                        a.Version,
                        a.AcceptanceDateTime,
                        current?.Url,
                        current is not null && current.Version == a.Version);
                })
                .ToList();

            return Page();
        }
    }
}
