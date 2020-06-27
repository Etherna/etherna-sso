using Etherna.DomainEvents;
using Etherna.SSOServer.Services.Domain;
using Etherna.SSOServer.Services.Utilities;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private const string EventHandlersNamespace = "Etherna.SSOServer.Services.EventHandlers";

        public static void AddDomainServices(this IServiceCollection services)
        {
            // Events.
            //register handlers in Ioc
            var eventHandlerTypes = from t in typeof(ServiceCollectionExtensions).GetTypeInfo().Assembly.GetTypes()
                                    where t.IsClass && t.Namespace == EventHandlersNamespace
                                    where t.GetInterfaces().Contains(typeof(IEventHandler))
                                    select t;
            foreach (var handlerType in eventHandlerTypes)
                services.AddScoped(handlerType);

            services.AddSingleton<IEventDispatcher>(sp =>
            {
                var dispatcher = new EventDispatcher(sp);

                //subscrive handlers to dispatcher
                foreach (var handlerType in eventHandlerTypes)
                    dispatcher.AddHandler(handlerType);

                return dispatcher;
            });

            // Register services.
            //domain
            services.AddScoped<IWeb3AuthnService, Web3AuthnService>();

            //utilities
            services.AddScoped<IEmailSender, EmailSender>();
        }
    }
}
