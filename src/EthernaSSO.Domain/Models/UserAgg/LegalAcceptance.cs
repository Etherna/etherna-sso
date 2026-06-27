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

namespace Etherna.SSOServer.Domain.Models.UserAgg
{
    /// <summary>
    /// The proof that a user accepted a specific version of a legal document at a given moment.
    /// Kept as an immutable, append-only record to satisfy the accountability principle
    /// (GDPR art. 7(1) / Swiss nFADP): we must be able to demonstrate what was accepted and when.
    /// </summary>
    public class LegalAcceptance : ModelBase
    {
        // Constructors.
        public LegalAcceptance(
            LegalDocumentType documentType,
            string version,
            DateTime acceptanceDateTime)
        {
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException("Version can't be empty", nameof(version));

            AcceptanceDateTime = acceptanceDateTime;
            DocumentType = documentType;
            Version = version;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        protected LegalAcceptance() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        // Properties.
        public virtual DateTime AcceptanceDateTime { get; protected set; }
        public virtual LegalDocumentType DocumentType { get; protected set; }
        public virtual string Version { get; protected set; }

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not LegalAcceptance other) return false;
            return AcceptanceDateTime == other.AcceptanceDateTime &&
                DocumentType == other.DocumentType &&
                Version == other.Version;
        }

        public override int GetHashCode() =>
            HashCode.Combine(AcceptanceDateTime, DocumentType, Version);
    }
}
