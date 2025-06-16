using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Data.Client;

public class DataClientTwoTypedTests
{
    private readonly ITestOutputHelper _outputHelper;
    public DataClientTwoTypedTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task DiConstructorTwoTypes()
    {
        using var host = BuildService();
        var myType1 = host.Services.GetRequiredService<MyType1>();
        var myType2 = host.Services.GetRequiredService<MyType2>();
        var context = host.Services.CreateContext<MyType1>();

        (await myType1.Cache.Get("anyKey1", null, context)).Action(result =>
        {
            result.BeOk();

            new EntityModel1 { Name = "CustomerProviderCreated-1", Age = 25 }.Action(x =>
            {
                (result.Return() == x).BeTrue();
            });
        });

        (await myType2.Cache.Get("anyKey2", null, context)).Action(result =>
        {
            result.BeOk();

            new EntityModel2 { City = "Kirkland", Date = new DateTime(2025, 1, 10) }.Action(x =>
            {
                (result.Return() == x).BeTrue();
            });
        });
    }

    private IHost BuildService()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));
                services.AddInMemoryFileStore();

                services.Configure<DataClientOption>(x => { });

                services.AddDataClient<EntityModel1>(builder =>
                {
                    builder.AddMemoryCache();
                    builder.AddFileStoreCache();
                    builder.AddProvider<CustomProvider1>();
                });

                services.AddDataClient<EntityModel2>(builder =>
                {
                    builder.AddMemoryCache();
                    builder.AddFileStoreCache();
                    builder.AddProvider<CustomProvider2>();
                });

                services.AddSingleton<MyType1>();
                services.AddSingleton<MyType2>();
                services.AddSingleton<CustomProvider1>();
                services.AddSingleton<CustomProvider2>();
            })
            .Build();

        return host;
    }

    public record EntityModel1
    {
        public string Name { get; init; } = null!;
        public int Age { get; init; }
    }

    public record EntityModel2
    {
        public string City { get; init; } = null!;
        public DateTime Date { get; init; }
    }

    public class MyType1
    {
        public MyType1(IDataClient<EntityModel1> cache)
        {
            Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public IDataClient<EntityModel1> Cache { get; }
    }

    public class MyType2
    {
        public MyType2(IDataClient<EntityModel2> cache)
        {
            Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public IDataClient<EntityModel2> Cache { get; }
    }

    public class CustomProvider1 : DataProviderBase
    {
        public override Task<Option<T>> Get<T>(string key, object? state, ScopeContext context)
        {
            var result = new EntityModel1 { Name = "CustomerProviderCreated-1", Age = 25 };
            return result.Cast<T>().ToOption().ToTaskResult();
        }
    }

    public class CustomProvider2 : DataProviderBase
    {
        public override Task<Option<T>> Get<T>(string key, object? state, ScopeContext context)
        {
            var result = new EntityModel2 { City = "Kirkland", Date = new DateTime(2025, 1, 10) };
            return result.Cast<T>().ToOption().ToTaskResult();
        }
    }
}
