//   Copyright 2021-present Etherna Sagl
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

using Etherna.MongODM.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Etherna.SSOServer.Persistence.Helpers
{
    internal sealed class EntityModelEqualityComparer : IEqualityComparer<IEntityModel<string>?>
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
