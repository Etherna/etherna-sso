// Copyright 2021-present Etherna SA
// This file is part of Etherna Sso.
// 
// Etherna Sso is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Sso is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Sso.
// If not, see <https://www.gnu.org/licenses/>.

using Nethereum.Util;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Etherna.SSOServer.Domain.Models
{
    public class Web3LoginToken : EntityModelBase<string>
    {
        // Consts.
        public const int CodeLength = 10;
        public readonly static string CodeValidChars = "0123456789" + "abcdefghijklmnopqrstuvwxyz" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + "-_";

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
        [SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "A different code each login is sufficient. Secure randomness is not required")]
        private string GenerateNewCode()
        {
            var length = CodeLength;
            var dictionary = CodeValidChars;

            var codeBuilder = new StringBuilder();
            for (int i = 0; i < length; i++)
                codeBuilder.Append(dictionary[Random.Shared.Next(dictionary.Length)]);

            return codeBuilder.ToString();
        }
    }
}
