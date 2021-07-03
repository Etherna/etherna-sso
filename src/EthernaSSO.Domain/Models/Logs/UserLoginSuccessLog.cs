namespace Etherna.SSOServer.Domain.Models.Logs
{
    public class UserLoginSuccessLog : LogBase
    {
        // Constructors.
        public UserLoginSuccessLog(User user)
        {
            User = user;
        }
        protected UserLoginSuccessLog() { }

        // Properties.
        public virtual User User { get; protected set; } = default!;
    }
}
