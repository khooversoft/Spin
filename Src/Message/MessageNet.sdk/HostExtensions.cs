using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Directory.sdk.Model;
using MessageNet.sdk.Host;
using MessageNet.sdk.Protocol;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.Queue;
using Toolbox.Broker;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace MessageNet.sdk
{
    public static class HostExtensions
    {
        public static IServiceCollection AddMessageHost(this IServiceCollection serviceCollection)
        {
            serviceCollection.VerifyNotNull(nameof(serviceCollection));

            serviceCollection.AddSingleton<IMessageHost, MessageHost>();
            return serviceCollection;
        }

        public static void StartMessageHost(this IApplicationBuilder builder, string serviceId)
        {
            IMessageHost host = builder.ApplicationServices.GetRequiredService<IMessageHost>();
            host.Start(serviceId);
        }

        public static void StartMessageHost(this IServiceProvider serviceProvider, string serviceId)
        {
            serviceProvider
                .GetRequiredService<IMessageHost>()
                .Start(serviceId);
        }

        public static IServiceCollection AddMessageControllers(this IServiceCollection serviceCollection)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                //Find assemblies with your ServiceType
                var types = assembly.GetTypes()
                    .Where(x => x.GetCustomAttribute<MessageControllerAttribute>() != null);

                foreach (var type in types)
                {
                    serviceCollection.AddSingleton(type);
                }
            }

            return serviceCollection;
        }

        public static IApplicationBuilder MapMessageControllers(this IApplicationBuilder builder)
        {
            builder.VerifyNotNull(nameof(builder));

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            IMessageHost messageHost = builder.ApplicationServices.GetRequiredService<IMessageHost>();

            foreach (var assembly in assemblies)
            {
                //Find assemblies with your ServiceType
                IEnumerable<(Type type, MessageControllerAttribute? attribute)> types = assembly.GetTypes()
                    .Select(x => (type: x, attribute: x.GetCustomAttribute<MessageControllerAttribute>()))
                    .Where(x => x.attribute != null);

                foreach (var type in types)
                {
                    object instance = builder.ApplicationServices.GetRequiredService(type.type);
                    RegisterRoutes(messageHost, instance, type.attribute!, type.type);
                }
            }

            return builder;
        }

        private static void RegisterRoutes(IMessageHost messageHost, object instance, MessageControllerAttribute classAttribute, Type type)
        {
            const string controllerText = "Controller";

            IEnumerable<(MethodInfo MethodInfo, MessageEndpointAttribute? Attribute)> methods = type.GetMethods()
                .Select(x => (MethodInfo: x, Attribute: x.GetCustomAttribute<MessageEndpointAttribute>()))
                .Where(x => x.Attribute != null);

            foreach (var method in methods)
            {
                // Pattern = "[{method}]{path[/path...]}"
                string path = new[]
                {
                    $"[{method.Attribute?.Method ?? method.MethodInfo.Name}]",
                    classAttribute.BasePath ?? type.Name.Func(x => x.EndsWith(controllerText) ? x[0..^(controllerText.Length)] : x)
                }.Join();

                var route = new Route<Message>(
                    path,
                    message =>
                    {
                        object? result = method.MethodInfo.Invoke(instance, new object[] { message! });
                        return result as Task ?? Task.CompletedTask;
                    });

                messageHost.Router.Add(route);
            }
        }
    }
}