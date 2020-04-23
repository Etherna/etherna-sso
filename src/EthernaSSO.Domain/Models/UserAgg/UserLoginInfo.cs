namespace Etherna.SSOServer.Domain.Models.UserAgg
{
    public class UserLoginInfo : ModelBase
    {
        // Constructors.
        public UserLoginInfo(
            string loginProvider,
            string providerKey)
        {
            LoginProvider = loginProvider;
            ProviderKey = providerKey;
        }
        protected UserLoginInfo() { }

        // Properties.
        public virtual string LoginProvider { get; protected set; }
        public virtual string ProviderKey { get; protected set; }
    }
}
