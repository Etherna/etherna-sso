namespace Etherna.SSOServer.RCL.Views.Emails
{
    public class InvitationLetterModel
    {
        public const string Title = "Etherna invitation";

        public InvitationLetterModel(string invitationUrl)
        {
            InvitationUrl = invitationUrl;
        }

        public string InvitationUrl { get; }
    }
}
