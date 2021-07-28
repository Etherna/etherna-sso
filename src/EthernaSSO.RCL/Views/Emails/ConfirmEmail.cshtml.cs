namespace Etherna.SSOServer.RCL.Views.Emails
{
    public class ConfirmEmailModel
    {
        public const string Title = "Confirm your email";

        public ConfirmEmailModel(string callbackUrl)
        {
            CallbackUrl = callbackUrl;
        }

        public string CallbackUrl { get; }
    }
}
