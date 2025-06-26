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
    private const string _pipelineName = nameof(DataClientTypedConstructorTests) + ".pipeline";
    private readonly ITestOutputHelper _outputHelper;
    public DataClientTypedConstructorTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task DiConstructorWithInterface()
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

                services.AddDataPipeline<EntityModel>(builder =>
                {
                    builder.MemoryCacheDuration = TimeSpan.FromMinutes(1);
                    builder.FileCacheDuration = TimeSpan.FromMinutes(1);
                    builder.BasePath = nameof(DataClientTypedConstructorTests);

                    builder.AddMemory();
                    builder.AddFileStore();
                    builder.AddProvider<CustomProvider>();
                }, _pipelineName);

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

    public class CustomProvider : IDataProvider
    {
        public IDataProvider? InnerHandler { get; set; }

        public Task<Option<DataPipelineContext>> Execute(DataPipelineContext dataContext, ScopeContext context)
        {
            switch (dataContext.Command)
            {
                case DataPipelineCommand.Get:
                    var result = new EntityModel { Name = "CustomerProviderCreated", Age = 25 };
                    dataContext = dataContext with { GetData = [result.ToDataETag()] };
                    return dataContext.ToOption().ToTaskResult();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
