// Copyright 2021-present Etherna Sa
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
