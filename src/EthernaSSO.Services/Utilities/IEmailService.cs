using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Utilities
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
