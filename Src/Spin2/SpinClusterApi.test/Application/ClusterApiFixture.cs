﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterApi.test.Application;

public class ClusterApiFixture
{
    public ClusterApiFixture()
    {
        Option = new ConfigurationBuilder()
            .AddJsonFile("test-appsettings.json")
            .Build()
            .Bind<TestOption>().Assert(x => x.Validate().IsOk(), "Invalid");

        ServiceProvider = new ServiceCollection()
        .AddSingleton(Option)
        .Action(x =>
        {
            x.AddHttpClient("raw", client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<SubscriptionClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<TenantClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<UserClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<PrincipalKeyClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<PrincipalPrivateKeyClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<SignatureClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
        })
        .BuildServiceProvider();
    }

    public TestOption Option { get; }

    public IServiceProvider ServiceProvider { get; }

    public HttpClient GetClient() => ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("raw");
}
