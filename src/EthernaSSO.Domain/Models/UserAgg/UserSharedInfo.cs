using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Etherna.SSOServer.Domain.Models.UserAgg
{
    /* This model will not be encapsulated with UserBase until https://etherna.atlassian.net/browse/MODM-101 is solved.
     * After it, a full referenced inclusion can be implemented. */

    /// <summary>
    /// A class that expose <see cref="UserBase"/> information, but that needs to be saved on a different DbContext,
    /// shared with other Etherna services.
    /// </summary>
    public class UserSharedInfo : EntityModelBase<string>
    {
        // Fields.
        private List<string> _etherPreviousAddresses = new();

        // Constructors.
        public UserSharedInfo(string etherAddress)
        {
            EtherAddress = etherAddress;
        }
        protected UserSharedInfo() { }

        // Properties.
        [PersonalData]
        public virtual string EtherAddress { get; protected set; } = default!;
        [PersonalData]
        public virtual IEnumerable<string> EtherPreviousAddresses
        {
            get => _etherPreviousAddresses;
            protected set => _etherPreviousAddresses = new List<string>(value ?? Array.Empty<string>());
        }
        public virtual bool LockoutEnabled { get; set; }
        public virtual DateTimeOffset? LockoutEnd { get; set; }
    }
}
