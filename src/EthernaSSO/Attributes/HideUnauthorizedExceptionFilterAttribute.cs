// Copyright 2021-present Etherna Sa
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

using Etherna.MongODM.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System;
using System.Collections.Generic;

namespace Etherna.SSOServer.Attributes
{
    public sealed class HideUnauthorizedExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            
            // Log exception.
            Log.Warning(context.Exception, "API exception");

            // Handle exception.
            switch (context.Exception)
            {
                case ArgumentException _:
                case FormatException _:
                case InvalidOperationException _:
                case MongodmInvalidEntityTypeException _:
                    context.Result = new BadRequestObjectResult(context.Exception.Message);
                    break;
                case KeyNotFoundException _:
                case MongodmEntityNotFoundException _:
                case UnauthorizedAccessException _:
                    context.Result = new NotFoundResult();
                    break;
            }
        }
    }
}
