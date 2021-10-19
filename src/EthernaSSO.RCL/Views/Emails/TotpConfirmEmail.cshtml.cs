namespace Etherna.SSOServer.RCL.Views.Emails
{
    public class TotpConfirmEmailModel
    {
        public const string Title = "Etherna: Confirm your email";

        public TotpConfirmEmailModel(string code)
        {
            Code = code;
        }

        public string Code { get; }
    }
}
