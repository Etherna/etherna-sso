namespace Etherna.SSOServer.Services.Settings
{
    public class MVCSettings
    {
        public string ConfirmEmailAction { get; set; } = default!;
        public string ConfirmEmailController { get; set; } = default!;
        public string ResetPasswordAction { get; set; } = default!;
        public string ResetPasswordController { get; set; } = default!;
    }
}
