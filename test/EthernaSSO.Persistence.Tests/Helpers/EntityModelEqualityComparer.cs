using Etherna.MongODM.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Etherna.SSOServer.Persistence.Helpers
{
    internal class EntityModelEqualityComparer : IEqualityComparer<IEntityModel<string>?>
    {
        public static EntityModelEqualityComparer Instance { get; } = new EntityModelEqualityComparer();

        public bool Equals(IEntityModel<string>? x, IEntityModel<string>? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] IEntityModel<string>? obj)
        {
            if (obj?.Id is null)
                return -1;
            return obj.Id.GetHashCode(StringComparison.Ordinal);
        }
    }
}
