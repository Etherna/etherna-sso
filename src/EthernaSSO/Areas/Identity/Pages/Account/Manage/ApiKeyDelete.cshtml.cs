using Etherna.SSOServer.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class ApiKeyDeleteModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext ssoDbContext;

        // Constructor.
        public ApiKeyDeleteModel(
            ISsoDbContext ssoDbContext)
        {
            this.ssoDbContext = ssoDbContext;
        }

        // Properties.
        public string Id { get; private set; } = default!;

        [Display(Name = "End of life")]
        public DateTime? EndOfLife { get; private set; }

        public string Label { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            var apiKey = await ssoDbContext.ApiKeys.FindOneAsync(id);

            Id = id;
            EndOfLife = apiKey.EndOfLife;
            Label = apiKey.Label;
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await ssoDbContext.ApiKeys.DeleteAsync(id);
            return RedirectToPage("ApiKeys");
        }
    }
}
