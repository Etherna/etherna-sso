using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
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
                EmailConfirmed = user.EmailConfirmed;
                PhoneNumber = user.PhoneNumber;
                PhoneNumberConfirmed = user.PhoneNumberConfirmed;
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

            [Display(Name = "Email confirmed")]
            public bool EmailConfirmed { get; set; }

            [Display(Name = "Ethereum login address")]
            public string? EtherLoginAddress { get; set; }

            [Display(Name = "Lockout enabled")]
            public bool LockoutEnabled { get; set; }

            [Display(Name = "Lockout end")]
            public DateTimeOffset? LockoutEnd { get; set; }

            [Display(Name = "Phone number")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Phone number confirmed")]
            public bool PhoneNumberConfirmed { get; set; }

            [Display(Name = "Two factor enabled")]
            public bool TwoFactorEnabled { get; private set; }

            public string Username { get; set; } = default!;
        }

        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public UserModel(ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public string? Id { get; private set; }

        [Display(Name = "Access failed count")]
        public int AccessFailedCount { get; private set; }

        [Display(Name = "Ethereum address")]
        public string? EtherAddress { get; private set; }

        [Display(Name = "Previous ethereum addresses")]
        public IEnumerable<string> EtherPreviousAddresses { get; private set; } = Array.Empty<string>();

        public bool IsWeb3 { get; private set; }

        [Display(Name = "Last login date/time (UTC)")]
        public DateTime? LastLoginDateTime { get; private set; }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        // Methods.
        public async Task OnGetAsync(string? id)
        {
            Id = id;

            if (id is not null)
            {
                var user = await context.Users.FindOneAsync(id);

                EtherAddress = user.EtherAddress;
                EtherPreviousAddresses = user.EtherPreviousAddresses;
                Input = new InputModel(user);
                IsWeb3 = user is UserWeb3;
                LastLoginDateTime = user.LastLoginDateTime;

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

        public IActionResult OnPostSave(string? id)
        {
            Id = id;

            if (!ModelState.IsValid)
                return Page();

            UserBase user = default!;
            if (id is null) //create
            {
                //TODO
            }
            else //update
            {
                //TODO
            }

            return RedirectToPage(new { user.Id });
        }
    }
}
