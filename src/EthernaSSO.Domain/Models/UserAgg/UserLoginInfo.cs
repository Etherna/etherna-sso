//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

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

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            if (obj is not UserLoginInfo) return false;
            return GetType() == obj.GetType() &&
                LoginProvider == ((UserLoginInfo)obj).LoginProvider &&
                ProviderDisplayName == ((UserLoginInfo)obj).ProviderDisplayName &&
                ProviderKey == ((UserLoginInfo)obj).ProviderKey;
        }

        public override int GetHashCode() =>
            LoginProvider.GetHashCode() ^
            ProviderDisplayName.GetHashCode() ^
            ProviderKey.GetHashCode();
    }
}
