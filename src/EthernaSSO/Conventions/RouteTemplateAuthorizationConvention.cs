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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using System;
using System.Linq;

namespace Etherna.SSOServer.Conventions
{
    public class RouteTemplateAuthorizationConvention(string routeTemplate, string policyName)
        : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            ArgumentNullException.ThrowIfNull(application, nameof(application));
            
            foreach (var controller in application.Controllers)
            {
                var isInRouteTemplate = controller.Selectors.Any(
                    s => s.AttributeRouteModel?.Template?.StartsWith(
                        routeTemplate,
                        StringComparison.OrdinalIgnoreCase) ?? false);
            
                //give priority to authorize attribute
                var hasAuthorizeAttribute = controller.Attributes.OfType<AuthorizeAttribute>().Any(); 
            
                if (isInRouteTemplate && !hasAuthorizeAttribute)
                    controller.Filters.Add(new AuthorizeFilter(policyName));
            }
        }
    }
}