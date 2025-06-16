using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Data.Client;

public class DataClientTypedConstructorTests
{
    private readonly ITestOutputHelper _outputHelper;
    public DataClientTypedConstructorTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task DiConstructorWithInterface()
    {
        using var host = BuildService();
        var myType = host.Services.GetRequiredService<MyType>();
        var context = host.Services.CreateContext<MyType>();

        var readOption = await myType.Cache.Get("anyKey", null, context);   // Read from custom provider
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

                services.Configure<DataClientOption>(x => { });

                services.AddDataClient<EntityModel>(builder =>
                {
                    builder.AddMemoryCache();
                    builder.AddFileStoreCache();
                    builder.AddProvider<CustomProvider>();
                });

                services.AddSingleton<MyType>();
                services.AddSingleton<CustomProvider>();
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
        public MyType(IDataClient<EntityModel> cache)
        {
            Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public IDataClient<EntityModel> Cache { get; }
    }

    public class CustomProvider : DataProviderBase
    {
        public override Task<Option<T>> Get<T>(string key, object? state, ScopeContext context)
        {
            var result = new EntityModel { Name = "CustomerProviderCreated", Age = 25 };
            return result.Cast<T>().ToOption().ToTaskResult();
        }
    }
}
