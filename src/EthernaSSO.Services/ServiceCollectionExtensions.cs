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
        }
    }
}
