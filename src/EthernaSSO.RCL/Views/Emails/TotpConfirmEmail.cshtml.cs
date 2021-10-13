namespace Etherna.SSOServer.RCL.Views.Emails
{
    public class ConfirmEmailModel
    {
        public const string Title = "Confirm your email";

        public ConfirmEmailModel(string code)
        {
            Code = code;
        }

        public string Code { get; }
    }
}
