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

using Etherna.MongoDB.Driver;
using Serilog.Exceptions.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.SSOServer.Configs.Logging
{
    /// <summary>
    /// Keeps only the scalar properties of the Mongo driver exceptions in the exception details of a log event:
    /// codes, names, messages, flags and error labels survive, while the object graphs the driver exposes
    /// (<c>Command</c>, <c>Result</c>, <c>Query</c>, <c>WriteError</c>, <c>ConnectionId</c>, ...) are dropped.
    /// Destructured by reflection, those graphs expand into hundreds of nested fields that clash with the mapping
    /// of the Elasticsearch log stream, and an event carrying one never reaches the stream.
    /// The properties inherited from <see cref="Exception"/> (message, inner exception, data) are not filtered.
    /// </summary>
    internal sealed class MongoExceptionPropertyFilter : IExceptionPropertyFilter
    {
        // Methods.
        public bool ShouldPropertyBeFiltered(Exception exception, string propertyName, object? value)
        {
            ArgumentNullException.ThrowIfNull(exception);

            if (exception is not MongoException)
                return false;

            //the value is already destructured (a dictionary or a list): decide on the declared property type
            var property = exception.GetType().GetProperties().FirstOrDefault(p => p.Name == propertyName);
            if (property is null || !typeof(MongoException).IsAssignableFrom(property.DeclaringType))
                return false;

            //strings, value types and string sequences (the error labels) are the scalars to keep
            var propertyType = property.PropertyType;
            return propertyType != typeof(string)
                && !propertyType.IsValueType
                && !typeof(IEnumerable<string>).IsAssignableFrom(propertyType);
        }
    }
}
