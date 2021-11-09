//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Helpers;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserModel : PageModel
    {
        // Model.
        public class InputModel : IValidatableObject
        {
            // Constructors.
            public InputModel() { }
            public InputModel(UserBase user)
            {
                if (user is null)
                    throw new ArgumentNullException(nameof(user));

                Id = user.Id;
                Email = user.Email;
                PhoneNumber = user.PhoneNumber;
                LockoutEnabled = user.LockoutEnabled;
                LockoutEnd = user.LockoutEnd;
                Username = user.Username;

                switch (user)
                {
                    case UserWeb2 userWeb2:
                        EtherLoginAddress = userWeb2.EtherLoginAddress;
                        TwoFactorEnabled = userWeb2.TwoFactorEnabled;
                        break;
                    case UserWeb3 _: break;
                    default: throw new InvalidOperationException();
                }
            }

            // Properties.
            public string? Id { get; set; }

            [EmailAddress]
            public string? Email { get; set; }

            [Display(Name = "Ethereum login address")]
            public string? EtherLoginAddress { get; set; }

            [Display(Name = "Lockout enabled")]
            public bool LockoutEnabled { get; set; } = true;

            [Display(Name = "Lockout end")]
            public DateTimeOffset? LockoutEnd { get; set; }

            [Display(Name = "New user password")]
            public string? NewUserPassword { get; set; }

            [Display(Name = "Phone number")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Two factor enabled")]
            public bool TwoFactorEnabled { get; set; }

            [Required]
            [RegularExpression(UsernameHelper.UsernameRegex)]
            public string Username { get; set; } = default!;

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (Id is null && NewUserPassword is null) //new user require password
                    yield return new ValidationResult("Password is required", new[] { nameof(NewUserPassword) });
            }
        }

        // Fields.
        private readonly ISsoDbContext context;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public UserModel(
            ISsoDbContext context,
            UserManager<UserBase> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        // Properties.
        [Display(Name = "Access failed count")]
        public int AccessFailedCount { get; private set; }

        [Display(Name = "Ethereum address")]
        public string? EtherAddress { get; private set; }

        [Display(Name = "Previous ethereum addresses")]
        public IEnumerable<string> EtherPreviousAddresses { get; private set; } = Array.Empty<string>();

        [Display(Name = "External login providers")]
        public IEnumerable<string> ExternalLoginProviders { get; private set; } = Array.Empty<string>();

        [Display(Name = "Has password")]
        public bool HasPassword { get; private set; }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public bool IsWeb3 { get; private set; }

        [Display(Name = "Last login date/time (UTC)")]
        public DateTime? LastLoginDateTime { get; private set; }

        [Display(Name = "Phone number confirmed")]
        public bool PhoneNumberConfirmed { get; private set; }

        public string? StatusMessage { get; set; }

        // Methods.
        public async Task OnGetAsync(string? id)
        {
            if (id is not null)
            {
                var user = await context.Users.FindOneAsync(id);

                EtherAddress = user.EtherAddress;
                EtherPreviousAddresses = user.EtherPreviousAddresses;
                Input = new InputModel(user);
                IsWeb3 = user is UserWeb3;
                LastLoginDateTime = user.LastLoginDateTime;
                PhoneNumberConfirmed = user.PhoneNumberConfirmed;

                switch (user)
                {
                    case UserWeb2 userWeb2:
                        AccessFailedCount = userWeb2.AccessFailedCount;
                        ExternalLoginProviders = userWeb2.Logins.Select(l => l.ProviderDisplayName);
                        HasPassword = userWeb2.HasPassword;
                        break;
                    case UserWeb3 _: break;
                    default: throw new InvalidOperationException();
                }
            }
            else
            {
                Input = new InputModel();
            }
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            UserBase user;
            if (Input.Id is null) //create
            {
                var userWeb2 = new UserWeb2(Input.Username, null, true, null);

                if (Input.Email is not null)
                    userWeb2.SetEmail(Input.Email);
                userWeb2.SetPhoneNumber(Input.PhoneNumber);
                userWeb2.LockoutEnabled = Input.LockoutEnabled;
                userWeb2.LockoutEnd = Input.LockoutEnd;
                if (!string.IsNullOrWhiteSpace(Input.EtherLoginAddress))
                    userWeb2.SetEtherLoginAddress(Input.EtherLoginAddress);
                userWeb2.TwoFactorEnabled = Input.TwoFactorEnabled;

                user = userWeb2;

                // Create.
                var result = await userManager.CreateAsync(user, Input.NewUserPassword);

                // Report errors.
                if (!result.Succeeded)
                {
                    StatusMessage = "Error: " + string.Join("\n", result.Errors.Select(e => e.Description));
                    return Page();
                }
            }
            else //update
            {
                user = await context.Users.FindOneAsync(Input.Id);

                if (user.Username != Input.Username)
                    user.SetUsername(Input.Username);

                if (string.IsNullOrWhiteSpace(Input.Email))
                    user.RemoveEmail();
                else if (user.Email != Input.Email)
                    user.SetEmail(Input.Email);

                if (user.PhoneNumber != Input.PhoneNumber)
                    user.SetPhoneNumber(Input.PhoneNumber);

                user.LockoutEnabled = Input.LockoutEnabled;

                user.LockoutEnd = Input.LockoutEnd;

                switch (user)
                {
                    case UserWeb2 userWeb2:

                        if (string.IsNullOrWhiteSpace(Input.EtherLoginAddress))
                            userWeb2.RemoveEtherLoginAddress();
                        else if (userWeb2.EtherLoginAddress != Input.EtherLoginAddress)
                            userWeb2.SetEtherLoginAddress(Input.EtherLoginAddress);

                        userWeb2.TwoFactorEnabled = Input.TwoFactorEnabled;

                        break;
                    case UserWeb3 _: break;
                    default: throw new InvalidOperationException();
                }

                await context.SaveChangesAsync();
            }

            return RedirectToPage(new { user.Id });
        }
    }
}
