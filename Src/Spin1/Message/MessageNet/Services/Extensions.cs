using MessageNet.Application;
using MessageNet.sdk.Endpoint;
using MessageNet.sdk.Host;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace MessageNet.Services
{
    public static class Extensions
    {
        public static void AddMessageNet(this IApplicationBuilder app)
        {
            IMessageHost messageHost = app.ApplicationServices.GetRequiredService<IMessageHost>();
            Option option = app.ApplicationServices.GetRequiredService<Option>();
            messageHost.Register(option.Nodes.ToArray());
        }

        public static void AddMessageNet(this IServiceCollection services)
        {
            services.AddSingleton<IMessageHost, MessageHost>();
            services.AddSingleton<MessageEndpointCollection>();

            services.AddHttpClient<ICallbackFactory, CallbackFactory>();
        }
    }
}