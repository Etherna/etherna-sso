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