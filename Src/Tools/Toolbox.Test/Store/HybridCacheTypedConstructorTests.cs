using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store;

public class HybridCacheTypedConstructorTests
{
    private readonly ITestOutputHelper _outputHelper;
    public HybridCacheTypedConstructorTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task DiConstructorTestsWithInterface()
    {
        using var host = BuildService();
        var myType = host.Services.GetRequiredService<MyType>();
        var context = host.Services.CreateContext<MyType>();

        var readOption = await myType.Cache.Get("anyKey", context);   // Read from custom provider
        readOption.BeOk();

        var compare = new EntityModel { Name = "CustomerProviderCreated", Age = 25 };
        (readOption.Return() == compare).BeTrue();
    }

    private IHost BuildService()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));
                services.AddInMemoryFileStore();

                services.Configure<HybridCacheOption>(x => { });

                services.AddHybridCache<EntityModel>(builder =>
                {
                    builder.AddMemoryCache();
                    builder.AddFileStoreCache();
                    builder.AddProvider(new CustomProvider());
                });

                services.AddSingleton<MyType>();
            })
            .Build();

        return host;
    }

    public record EntityModel
    {
        public string Name { get; init; } = null!;
        public int Age { get; init; }
    }

    public class MyType
    {
        public MyType(IHybridCache<EntityModel> cache)
        {
            Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public IHybridCache<EntityModel> Cache { get; }
    }

    public class CustomProvider : IHybridCacheProvider
    {
        public string Name => throw new NotImplementedException();
        public HybridCacheCounters Counters => new();

        public Task<Option> Delete(string key, ScopeContext context) => Task.FromResult<Option>(StatusCode.OK);
        public Task<Option<string>> Exists(string key, ScopeContext context) => new Option<string>(StatusCode.OK).ToTaskResult();

        public Task<Option<T>> Get<T>(string key, ScopeContext context)
        {
            var result = new EntityModel { Name = "CustomerProviderCreated", Age = 25 };
            return result.Cast<T>().ToOption<T>().ToTaskResult();
        }

        public Task<Option> Set<T>(string key, T value, ScopeContext context) => Task.FromResult<Option>(StatusCode.OK);
    }

}
