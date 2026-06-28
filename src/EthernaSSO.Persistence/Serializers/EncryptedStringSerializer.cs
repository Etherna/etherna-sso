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

using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Etherna.SSOServer.Persistence.Serializers
{
    /// <summary>
    /// Serializes a string encrypting it at rest with AES-256-GCM, storing it base64-encoded.
    /// The stored payload layout is: nonce (12 bytes) | tag (16 bytes) | ciphertext.
    /// </summary>
    public sealed class EncryptedStringSerializer : SerializerBase<string>
    {
        // Consts.
        private const int KeySize = 32; //AES-256
        private const int NonceSize = 12; //AesGcm standard nonce size
        private const int TagSize = 16; //AesGcm max tag size

        // Fields.
        private readonly byte[] key;
        private readonly StringSerializer stringSerializer = new();

        // Constructor.
        public EncryptedStringSerializer(string base64Key)
        {
            ArgumentException.ThrowIfNullOrEmpty(base64Key);

            key = Convert.FromBase64String(base64Key);
            if (key.Length != KeySize)
                throw new ArgumentException(
                    $"Encryption key must be {KeySize} bytes long (base64-encoded)", nameof(base64Key));
        }

        // Methods.
        public string Decrypt(string base64Payload)
        {
            ArgumentException.ThrowIfNullOrEmpty(base64Payload);

            var payload = Convert.FromBase64String(base64Payload);
            var nonce = payload.AsSpan(0, NonceSize);
            var tag = payload.AsSpan(NonceSize, TagSize);
            var ciphertext = payload.AsSpan(NonceSize + TagSize);
            var plaintext = new byte[ciphertext.Length];

            using var aesGcm = new AesGcm(key, TagSize);
            aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }

        public override string Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) =>
            Decrypt(stringSerializer.Deserialize(context, args));

        public string Encrypt(string plaintext)
        {
            ArgumentNullException.ThrowIfNull(plaintext);

            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var nonce = RandomNumberGenerator.GetBytes(NonceSize);
            var tag = new byte[TagSize];
            var ciphertext = new byte[plaintextBytes.Length];

            using var aesGcm = new AesGcm(key, TagSize);
            aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            var payload = new byte[NonceSize + TagSize + ciphertext.Length];
            nonce.CopyTo(payload, 0);
            tag.CopyTo(payload, NonceSize);
            ciphertext.CopyTo(payload, NonceSize + TagSize);

            return Convert.ToBase64String(payload);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string value) =>
            stringSerializer.Serialize(context, args, Encrypt(value));
    }
}
