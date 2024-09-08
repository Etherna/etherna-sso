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

using System;
using System.Security.Cryptography;
using System.Text;

namespace Etherna.SSOServer.Domain.Models
{
    public class ApiKey : EntityModelBase<string>
    {
        // Consts.
        public const int KeyLength = 64;
        public const string KeyValidChars = "0123456789" + "abcdefghijklmnopqrstuvwxyz" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const int LabelMaxLength = 25;
        public const int MaxKeysPerUser = 10;

        // Static fields.
        private static readonly SHA256 sha256Encoder = SHA256.Create();

        // Constructors.
        public ApiKey(
            string plainKey,
            string label,
            DateTime? endOfLife,
            UserBase owner)
        {
            if (string.IsNullOrEmpty(plainKey))
                throw new ArgumentException($"'{nameof(plainKey)}' cannot be null or empty.", nameof(plainKey));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException($"'{nameof(label)}' cannot be null or whitespace.", nameof(label));
            if (label.Length > LabelMaxLength)
                throw new ArgumentException($"'{nameof(label)}' cannot be longer than {LabelMaxLength}.", nameof(label));

            EndOfLife = endOfLife;
            KeyHash = HashKey(plainKey);
            Label = label;
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ApiKey() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual DateTime? EndOfLife { get; protected set; }
        public virtual bool IsAlive => EndOfLife is null || DateTime.UtcNow <= EndOfLife;
        public virtual byte[] KeyHash { get; protected set; }
        public virtual string Label { get; protected set; }
        public virtual UserBase Owner { get; protected set; }

        // Helpers.
        public static string GetPrettyPrintedPlainKey(string plainKey, UserBase owner)
        {
            ArgumentNullException.ThrowIfNull(plainKey, nameof(plainKey));
            ArgumentNullException.ThrowIfNull(owner, nameof(owner));

            return $"{owner.Id}.{plainKey}";
        }

        public static string GetRandomPlainKey()
        {
            var codeBuilder = new StringBuilder();
            for (int i = 0; i < KeyLength; i++)
                codeBuilder.Append(KeyValidChars[RandomNumberGenerator.GetInt32(KeyValidChars.Length)]);

            return codeBuilder.ToString();
        }

        public static byte[] HashKey(string plainKey) =>
            sha256Encoder.ComputeHash(Encoding.UTF8.GetBytes(plainKey));
    }
}
