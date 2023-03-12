using Etherna.MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Domain.Models
{
    public class ApiKey : EntityModelBase<string>
    {
        // Consts.
        public const int KeyLength = 64;
        public const string KeyValidChars = "0123456789" + "abcdefghijklmnopqrstuvwxyz" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // Static fields.
        private static readonly SHA256 sha256Encoder = SHA256.Create();

        // Constructors.
        public ApiKey(
            string plainKey,
            string label,
            DateTime? endOfLife,
            UserBase owner)
        {
            EndOfLife = endOfLife;
            KeyHash = HashKey(plainKey);
            Label = label;
            Owner = owner;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ApiKey() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual DateTime? EndOfLife { get; protected set; }
        public virtual bool IsLive => EndOfLife is null || DateTime.UtcNow <= EndOfLife;
        public virtual byte[] KeyHash { get; protected set; }
        public virtual string Label { get; protected set; }
        public virtual UserBase Owner { get; protected set; }

        // Helpers.
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
