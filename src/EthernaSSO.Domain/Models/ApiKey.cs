﻿// Copyright 2021-present Etherna Sa
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
