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
using Etherna.SSOServer.Domain.Models.UserAgg;
using Etherna.SSOServer.Services.Domain;
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
            public InputModel(UserBase user, UserSharedInfo sharedInfo)
            {
                ArgumentNullException.ThrowIfNull(user, nameof(user));
                ArgumentNullException.ThrowIfNull(sharedInfo, nameof(sharedInfo));

                Id = user.Id;
                Email = user.Email;
                PhoneNumber = user.PhoneNumber;
                LockoutEnabled = sharedInfo.LockoutEnabled;
                LockoutEnd = sharedInfo.LockoutEnd;
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
        private readonly IUserService userService;

        // Constructor.
        public UserModel(
            ISsoDbContext context,
            IUserService userService)
        {
            this.context = context;
            this.userService = userService;
        }

        // Properties.
        [Display(Name = "Access failed count")]
        public int AccessFailedCount { get; private set; }

        [Display(Name = "Ethereum address")]
        public string? EtherAddress { get; private set; }

        [Display(Name = "Previous ethereum addresses")]
        public IEnumerable<string> EtherPreviousAddresses { get; private set; } = Array.Empty<string>();

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
                var sharedInfo = await userService.GetSharedUserInfoAsync(user);

                EtherAddress = user.EtherAddress;
                EtherPreviousAddresses = user.EtherPreviousAddresses;
                Input = new InputModel(user, sharedInfo);
                IsWeb3 = user is UserWeb3;
                LastLoginDateTime = user.LastLoginDateTime;
                PhoneNumberConfirmed = user.PhoneNumberConfirmed;

                switch (user)
                {
                    case UserWeb2 userWeb2:
                        AccessFailedCount = userWeb2.AccessFailedCount;
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
                // Register.
                var (errors, newUser) = await userService.RegisterWeb2UserByAdminAsync(
                    Input.Username,
                    Input.NewUserPassword!,
                    Input.Email,
                    Input.EtherLoginAddress,
                    Input.LockoutEnabled,
                    Input.LockoutEnd,
                    Input.PhoneNumber,
                    Array.Empty<Role>(),
                    Input.TwoFactorEnabled);

                // Report errors.
                if (newUser is null)
                {
                    StatusMessage = "Error: " + string.Join("\n", errors.Select(e => e.msg));
                    return Page();
                }

                user = newUser;
            }
            else //update
            {
                //user info
                user = await context.Users.FindOneAsync(Input.Id);

                if (user.Username != Input.Username)
                    user.SetUsername(Input.Username);

                if (string.IsNullOrWhiteSpace(Input.Email))
                    user.RemoveEmail();
                else if (user.Email != Input.Email)
                    user.SetEmail(Input.Email);

                if (user.PhoneNumber != Input.PhoneNumber)
                    user.SetPhoneNumber(Input.PhoneNumber);

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

                //shared user info
                await userService.UpdateLockoutStatusAsync(
                    user,
                    Input.LockoutEnabled,
                    Input.LockoutEnd);
            }

            return RedirectToPage(new { user.Id });
        }
    }
}
