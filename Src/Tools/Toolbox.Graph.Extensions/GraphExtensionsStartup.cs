//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.DependencyInjection.Extensions;

//namespace Toolbox.Graph.Extensions;

//public static class GraphExtensionsStartup
//{
//    public static IServiceCollection AddGraphExtensions(this IServiceCollection service)
//    {
//        service.AddSingleton<IdentityClient>();
//        service.AddSingleton<SecurityGroupClient>();
//        service.AddSingleton<ChannelClient>();
//        service.TryAddSingleton<IUserStore<PrincipalIdentity>, IdentityUserStore>();
//        return service;
//    }
//}
