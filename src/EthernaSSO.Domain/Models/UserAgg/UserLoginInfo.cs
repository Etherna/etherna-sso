namespace Etherna.SSOServer.Domain.Models.UserAgg
{
    public class UserLoginInfo : ModelBase
    {
        // Constructors.
        public UserLoginInfo(
            string loginProvider,
            string providerKey,
            string providerDisplayName)
        {
            LoginProvider = loginProvider;
            ProviderKey = providerKey;
            ProviderDisplayName = providerDisplayName;
        }
        protected UserLoginInfo() { }

        // Properties.
        public virtual string LoginProvider { get; protected set; } = default!;
        public virtual string ProviderDisplayName { get; protected set; } = default!;
        public virtual string ProviderKey { get; protected set; } = default!;
    }
}
