using Nethereum.Util;
using System;

namespace Etherna.SSOServer.Domain.Models.UserAgg
{
    public class EtherAccount : ModelBase
    {
        // Enums.
        public enum EncryptionState
        {
            Plain,
            UserEncrypted,
            ServerEncrypted
        }

        // Constructors.
        public EtherAccount(
            string address,
            string? privateKey = default,
            EncryptionState privateKeyEncryption = EncryptionState.Plain)
        {
            SetAddress(address);
            if (privateKey != null)
            {
                SetPrivateKey(privateKey);
                PrivateKeyEncryption = privateKeyEncryption;
            }
        }
        protected EtherAccount()
        { }

        // Properties.
        public virtual string Address { get; protected set; } = default!;
        public virtual EncryptionState PrivateKeyEncryption { get; protected set; } = EncryptionState.Plain;
        public virtual bool IsManaged =>
            !(PrivateKey is null || PrivateKeyEncryption == EncryptionState.UserEncrypted);
        public virtual string? PrivateKey { get; protected set; }

        // Helpers.
        private void SetAddress(string address)
        {
            if (!address.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("The value is not a valid address", nameof(address));

            Address = address.ConvertToEthereumChecksumAddress();
        }

        private void SetPrivateKey(string privateKey)
        {
            if (privateKey is null)
                throw new ArgumentNullException(nameof(privateKey));

            //encrypted key validation
            //TBD

            PrivateKey = privateKey;
        }
    }
}
