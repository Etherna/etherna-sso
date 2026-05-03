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

using Etherna.BeeNet.Exceptions;
using Etherna.MongODM.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Api
{
    public static class ExceptionHandler
    {
        [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        public static async Task<IResult> RunAsync(Func<Task<IResult>> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            
            try
            {
                return await action();
            }
            catch (Exception e)
            {
                // Log exception.
                Log.Warning(e, "API exception");

                switch (e)
                {
                    // Error code 400.
                    case ArgumentException:
                    case FormatException:
                    case MongodmInvalidEntityTypeException:
                        return ErrorResults.GetBadRequestErrorResult();

                    // Error code 401.
                    case UnauthorizedAccessException:
                        return ErrorResults.GetUnauthorizedErrorResult();

                    // Error code 404.
                    case BeeNetApiException { StatusCode: 404 }:
                    case KeyNotFoundException:
                    case MongodmEntityNotFoundException:
                        return ErrorResults.GetNotFoundErrorResult();
                    
                    // Error code 503.
                    case BeeNetApiException:
                    case NotSupportedException:
                        return ErrorResults.GetServiceUnavailableErrorResult();
                        
                    // Error code 500.
                    case InvalidOperationException:
                    default:
                        return ErrorResults.GetInternalServerErrorResult();
                }
            }
        }
    }
}