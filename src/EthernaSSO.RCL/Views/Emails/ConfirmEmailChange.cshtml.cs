namespace Etherna.SSOServer.RCL.Views.Emails
{
    public class ConfirmEmailChangeModel
    {
        public const string Title = "Confirm your email";

        public ConfirmEmailChangeModel(string callbackUrl)
        {
            CallbackUrl = callbackUrl;
        }

        public string CallbackUrl { get; }
    }
}
