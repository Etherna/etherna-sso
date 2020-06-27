using Nethereum.Util;
using System;
using System.Text;

namespace Etherna.SSOServer.Domain.Models
{
    public class Web3LoginToken : EntityModelBase<string>
    {
        // Consts.
        public const int CodeLength = 10;
        public readonly static string CodeValidChars = "0123456789" + "abcdefghijklmnopqrstuvwxyz" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + "-_";

        // Static fields.
        private static readonly Random random = new Random();

        // Constructors.
        public Web3LoginToken(string etherAddress)
        {
            if (!etherAddress.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("The value is not a valid address", nameof(etherAddress));

            EtherAddress = etherAddress.ConvertToEthereumChecksumAddress();
            Code = GenerateNewCode();
        }
        protected Web3LoginToken() { }

        // Properties.
        public virtual string Code { get; protected set; } = default!;
        public virtual string EtherAddress { get; protected set; } = default!;

        // Helpers.
        private string GenerateNewCode()
        {
            var length = CodeLength;
            var dictionary = CodeValidChars;

            var codeBuilder = new StringBuilder();
            for (int i = 0; i < length; i++)
                codeBuilder.Append(dictionary[random.Next(dictionary.Length)]);

            return codeBuilder.ToString();
        }
    }
}
