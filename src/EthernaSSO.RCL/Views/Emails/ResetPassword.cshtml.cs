namespace Etherna.SSOServer.RCL.Views.Emails
{
    public class ResetPasswordModel
    {
        public const string Title = "Etherna: Reset password";

        public ResetPasswordModel(string url)
        {
            Url = url;
        }

        public string Url { get; }
    }
}
