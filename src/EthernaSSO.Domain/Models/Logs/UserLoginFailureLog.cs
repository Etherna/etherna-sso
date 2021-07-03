namespace Etherna.SSOServer.Domain.Models.Logs
{
    public class UserLoginFailureLog : LogBase
    {
        // Constructors.
        public UserLoginFailureLog(string error, string identifier)
        {
            ErrorMessage = error;
            Identifier = identifier;
        }
        protected UserLoginFailureLog() { }

        // Properties.
        public virtual string ErrorMessage { get; protected set; } = default!;
        public virtual string Identifier { get; protected set; } = default!;
    }
}
