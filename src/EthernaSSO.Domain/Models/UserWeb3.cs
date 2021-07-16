using Nethereum.Util;
using System;
using System.Linq;

namespace Etherna.SSOServer.Domain.Models
{
    public class UserWeb3 : UserBase
    {
        // Constructors.
        public UserWeb3(string address, string username, string? email, UserBase? invitedBy)
            : base(username, email, invitedBy)
        {
            if (!address.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("The value is not a valid address", nameof(address));

            EtherAddress = address;
        }
        public UserWeb3(UserWeb2 web2User)
        {
            if (web2User is null)
                throw new ArgumentNullException(nameof(web2User));
            if (web2User.EtherLoginAddress is null)
                throw new InvalidOperationException();

            Id = web2User.Id;
            Claims = web2User.Claims;
            CreationDateTime = web2User.CreationDateTime;
            Email = web2User.Email;
            EmailConfirmed = web2User.EmailConfirmed;
            EtherAddress = web2User.EtherLoginAddress;
            EtherPreviousAddresses = web2User.EtherPreviousAddresses.Append(web2User.EtherAddress);
            InvitedBy = web2User.InvitedBy;
            LastLoginDateTime = web2User.LastLoginDateTime;
            LockoutEnabled = web2User.LockoutEnabled;
            LockoutEnd = web2User.LockoutEnd;
            NormalizedEmail = web2User.NormalizedEmail;
            NormalizedUsername = web2User.NormalizedUsername;
            PhoneNumber = web2User.PhoneNumber;
            PhoneNumberConfirmed = web2User.PhoneNumberConfirmed;
            Roles = web2User.Roles;
            SecurityStamp = web2User.SecurityStamp;
            Username = web2User.Username;
        }
        protected UserWeb3() { }
    }
}
