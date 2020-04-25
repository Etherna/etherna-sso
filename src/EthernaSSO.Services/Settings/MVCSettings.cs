using System;
using System.Collections.Generic;
using System.Text;

namespace Etherna.SSOServer.Services.Settings
{
    public class MVCSettings
    {
        public string ResetPasswordAction { get; set; } = default!;
        public string ResetPasswordController { get; set; } = default!;
    }
}
