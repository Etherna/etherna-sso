using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Helpers;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserModel : PageModel
    {
        // Model.
        public class InputModel
        {
            // Constructors.
            public InputModel() { }
            public InputModel(UserBase user)
            {
                if (user is null)
                    throw new ArgumentNullException(nameof(user));

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
            [EmailAddress]
            public string? Email { get; set; }

            [Display(Name = "Ethereum login address")]
            public string? EtherLoginAddress { get; set; }

            [Display(Name = "Lockout enabled")]
            public bool LockoutEnabled { get; set; } = true;

            [Display(Name = "Lockout end")]
            public DateTimeOffset? LockoutEnd { get; set; }

            [Display(Name = "Phone number")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Two factor enabled")]
            public bool TwoFactorEnabled { get; set; }

            [Required]
            [RegularExpression(UsernameHelper.UsernameRegex)]
            public string Username { get; set; } = default!;
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
        public string? Id { get; private set; }

        [Display(Name = "Access failed count")]
        public int AccessFailedCount { get; private set; }

        [Display(Name = "Email confirmed")]
        public bool EmailConfirmed { get; private set; }

        [Display(Name = "Ethereum address")]
        public string? EtherAddress { get; private set; }

        [Display(Name = "Previous ethereum addresses")]
        public IEnumerable<string> EtherPreviousAddresses { get; private set; } = Array.Empty<string>();

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public bool IsWeb3 { get; private set; }

        [Display(Name = "Last login date/time (UTC)")]
        public DateTime? LastLoginDateTime { get; private set; }

        [Display(Name = "Phone number confirmed")]
        public bool PhoneNumberConfirmed { get; private set; }

        // Methods.
        public async Task OnGetAsync(string? id)
        {
            Id = id;

            if (id is not null)
            {
                var user = await context.Users.FindOneAsync(id);

                EmailConfirmed = user.EmailConfirmed;
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

        public async Task<IActionResult> OnPostSaveAsync(string? id)
        {
            Id = id;

            if (!ModelState.IsValid)
                return Page();

            UserBase user;
            if (id is null) //create
            {
                var userWeb2 = new UserWeb2(Input.Username, Input.Email, null, null);

                userWeb2.SetPhoneNumber(Input.PhoneNumber);
                userWeb2.LockoutEnabled = Input.LockoutEnabled;
                userWeb2.LockoutEnd = Input.LockoutEnd;
                if (!string.IsNullOrWhiteSpace(Input.EtherLoginAddress))
                    userWeb2.SetEtherLoginAddress(Input.EtherLoginAddress);
                userWeb2.TwoFactorEnabled = Input.TwoFactorEnabled;

                user = userWeb2;
                await userManager.CreateAsync(user);
            }
            else //update
            {
                user = await context.Users.FindOneAsync(id);

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