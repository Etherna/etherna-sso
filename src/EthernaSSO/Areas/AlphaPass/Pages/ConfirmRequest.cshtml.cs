using Etherna.ACR.Helpers;
using Etherna.SSOServer.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.AlphaPass.Pages
{
    public class ConfirmRequestModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext dbContext;

        // Constructor.
        public ConfirmRequestModel(
            ISsoDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // Properties.
        [TempData]
        public string? StatusMessage { get; set; }

        // Get.

        public async Task OnGetAsync(string email, string secret)
        {
            var request = await dbContext.AlphaPassRequests.TryFindOneAsync(r => r.NormalizedEmail == EmailHelper.NormalizeEmail(email));

            // Verify provided data.
            if (request is null ||
                request.Secret != secret)
            {
                StatusMessage = "Error: provided data is not correct";
                return;
            }

            // Confirm email.
            request.ConfirmEmail(secret);
            await dbContext.SaveChangesAsync();

            StatusMessage = "Thank you, your email has been verified! You will receive an Etherna Alpha Pass when available.";
        }
    }
}
