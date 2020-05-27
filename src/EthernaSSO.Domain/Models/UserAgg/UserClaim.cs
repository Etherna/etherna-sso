using System;

namespace Etherna.SSOServer.Domain.Models.UserAgg
{
    public class UserClaim : ModelBase
    {
        // Constructors.
        public UserClaim(string type, string value)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type can't be empty", nameof(type));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value can't be empty", nameof(value));

            Type = type;
            Value = value;
        }
        protected UserClaim() { }

        // Properties.
        public string Type { get; protected set; } = default!;
        public string Value { get; protected set; } = default!;

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is UserClaim claim2)) return false;
            return Type == claim2.Type && Value == claim2.Value;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode(StringComparison.InvariantCulture) ^
                Value.GetHashCode(StringComparison.InvariantCulture);
        }
    }
}
