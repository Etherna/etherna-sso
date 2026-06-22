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
using Etherna.SSOServer.Models;
using Etherna.SSOServer.Services.Domain;
using Etherna.SwarmSdk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserModel(
        ISsoDbContext context,
        IUserService userService)
        : PageModel
    {
        // Model.
        public class InputModel : IValidatableObject
        {
            // Constructors.
            public InputModel() { }
            public InputModel(UserBase user, UserSharedInfo sharedInfo)
            {
                ArgumentNullException.ThrowIfNull(user);
                ArgumentNullException.ThrowIfNull(sharedInfo);

                Id = user.Id;
                Email = user.Email;
                MaxAllowedClients = user.MaxAllowedClients;
                PhoneNumber = user.PhoneNumber;
                LockoutEnabled = sharedInfo.LockoutEnabled;
                LockoutEnd = sharedInfo.LockoutEnd?.UtcDateTime;
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
            public EthAddress? EtherLoginAddress { get; set; }

            [Display(Name = "Lockout enabled")]
            public bool LockoutEnabled { get; set; } = true;

            [Display(Name = "Lockout end (UTC)")]
            public DateTime? LockoutEnd { get; set; }

            [Display(Name = "New user password")]
            public string? NewUserPassword { get; set; }

            [Display(Name = "Phone number")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Two factor enabled")]
            public bool TwoFactorEnabled { get; set; }

            [Required]
            [RegularExpression(UsernameHelper.UsernameRegex)]
            public string Username { get; set; } = default!;

            [Display(Name = "Max allowed clients")]
            [Range(0, 100)]
            public int MaxAllowedClients { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (Id is null && NewUserPassword is null) //new user require password
                    yield return new ValidationResult("Password is required", new[] { nameof(NewUserPassword) });
            }
        }

        // Properties.
        [Display(Name = "Access failed count")]
        public int AccessFailedCount { get; private set; }

        [Display(Name = "Ethereum address")]
        public EthAddress? EtherAddress { get; private set; }

        [Display(Name = "Previous ethereum addresses")]
        public IEnumerable<EthAddress> EtherPreviousAddresses { get; private set; } = [];

        [Display(Name = "Has password")]
        public bool HasPassword { get; private set; }

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public bool IsWeb3 { get; private set; }

        [Display(Name = "User registration (UTC)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? CreationDateTime { get; private set; }

        [Display(Name = "Last login date/time (UTC)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? LastLoginDateTime { get; private set; }

        [Display(Name = "Phone number confirmed")]
        public bool PhoneNumberConfirmed { get; private set; }

        public StatusMessage? StatusMessage { get; set; }

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
                CreationDateTime = user.CreationDateTime;
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
                    Input.LockoutEnd.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(Input.LockoutEnd.Value, DateTimeKind.Utc)) : null,
                    Input.PhoneNumber,
                    []);

                // Report errors.
                if (newUser is null)
                {
                    StatusMessage = new StatusMessage("Error: " + string.Join("\n", errors.Select(e => e.msg)), StatusMessageType.Error);
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

                if (user.MaxAllowedClients != Input.MaxAllowedClients)
                    user.SetMaxAllowedClients(Input.MaxAllowedClients);

                switch (user)
                {
                    case UserWeb2 userWeb2:

                        if (!Input.EtherLoginAddress.HasValue)
                            userWeb2.RemoveEtherLoginAddress();
                        else if (userWeb2.EtherLoginAddress != Input.EtherLoginAddress)
                            userWeb2.EtherLoginAddress = Input.EtherLoginAddress;

                        //admin can only disable 2FA (clears all factors); enabling requires user-side setup
                        if (!Input.TwoFactorEnabled && userWeb2.TwoFactorEnabled)
                        {
                            userWeb2.DisableAuthenticatorApp();
                            foreach (var credential in userWeb2.Fido2Credentials.ToList())
                                userWeb2.RemoveFido2Credential(credential.CredentialId);
                        }

                        break;
                    case UserWeb3 _: break;
                    default: throw new InvalidOperationException();
                }

                await context.SaveChangesAsync();

                //shared user info
                await userService.UpdateLockoutStatusAsync(
                    user,
                    Input.LockoutEnabled,
                    Input.LockoutEnd.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(Input.LockoutEnd.Value, DateTimeKind.Utc)) : null);
            }

            return RedirectToPage(new { user.Id });
        }
    }
}
