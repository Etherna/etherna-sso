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

using Etherna.ACR;
using Etherna.DomainEvents;
using Etherna.DomainEvents.AspNetCore;
using Etherna.SSOServer.Services.Domain;
using Etherna.SSOServer.Services.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Etherna.SSOServer.Services
{
    public static class ServiceCollectionExtensions
    {
        private const string EventHandlersNamespace = "Etherna.SSOServer.Services.EventHandlers";

        public static void AddDomainServices(this IServiceCollection services)
        {
            // Dependencies.
            services.AddEthernaServicesSharedLibrary();

            // Events.
            //register handlers in Ioc
            var eventHandlerTypes = from t in typeof(ServiceCollectionExtensions).GetTypeInfo().Assembly.GetTypes()
                                    where t.IsClass && t.Namespace == EventHandlersNamespace
                                    where t.GetInterfaces().Contains(typeof(IEventHandler))
                                    select t;

            services.AddDomainEvents(eventHandlerTypes);

            // Register services.
            //domain
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IWeb3AuthnService, Web3AuthnService>();

            // Tasks.
            services.AddTransient<ICompileDailyStatsTask, CompileDailyStatsTask>();
            services.AddTransient<IDeleteOldInvitationsTask, DeleteOldInvitationsTask>();
            services.AddTransient<IProcessAlphaPassRequestsTask, ProcessAlphaPassRequestsTask>();
            services.AddTransient<IWeb3LoginTokensCleanTask, Web3LoginTokensCleanTask>();
        }
    }
}
