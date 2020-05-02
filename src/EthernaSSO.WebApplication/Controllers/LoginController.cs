using Etherna.SSOServer.WebApplication.Services;
using Microsoft.AspNetCore.Mvc;

namespace Etherna.SSOServer.WebApplication.Controllers
{
    public class LoginController : Controller
    {
        // Fields.
        private readonly ILoginControllerService service;

        // Constructors.
        public LoginController(ILoginControllerService service)
        {
            this.service = service;
        }

        // Methods.

    }
}
