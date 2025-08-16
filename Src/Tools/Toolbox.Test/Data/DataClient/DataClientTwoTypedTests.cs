//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Test.Data.Client;

//public class DataClientTwoTypedTests
//{
//    private readonly ITestOutputHelper _outputHelper;

//    public DataClientTwoTypedTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

//    [Fact]
//    public async Task DiConstructorTwoTypes()
//    {
//        using var host = BuildService();
//        var myType1 = host.Services.GetRequiredService<MyType1>();
//        var myType2 = host.Services.GetRequiredService<MyType2>();
//        var context = host.Services.CreateContext<MyType1>();

//        (await myType1.Handler.Get("anyKey1", context)).Action(result =>
//        {
//            result.BeOk();
//            (result.Return() == new EntityModel1 { Name = "CustomerProviderCreated-1", Age = 25 }).BeTrue();
//        });

//        (await myType2.Handler.Get("anyKey2", context)).Action(result =>
//        {
//            result.BeOk();
//            (result.Return() == new EntityModel2 { City = "Kirkland", Date = new DateTime(2025, 1, 10) }).BeTrue();
//        });
//    }

//    private IHost BuildService()
//    {
//        var host = Host.CreateDefaultBuilder()
//            .ConfigureServices((context, services) =>
//            {
//                services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));
//                services.AddInMemoryFileStore();

//                services.AddDataPipeline<EntityModel1>(builder =>
//                {
//                    builder.MemoryCacheDuration = TimeSpan.FromMinutes(1);
//                    builder.FileCacheDuration = TimeSpan.FromMinutes(1);
//                    builder.BasePath = nameof(DataClientTwoTypedTests);

//                    builder.AddCacheMemory();
//                    builder.AddProvider<CustomProvider1>();
//                });

//                services.AddDataPipeline<EntityModel2>(builder =>
//                {
//                    builder.MemoryCacheDuration = TimeSpan.FromMinutes(1);
//                    builder.FileCacheDuration = TimeSpan.FromMinutes(1);
//                    builder.BasePath = nameof(DataClientTwoTypedTests);

//                    builder.AddCacheMemory();
//                    builder.AddFileStore();
//                    builder.AddProvider<CustomProvider2>();
//                });

//                services.AddSingleton<MyType1>();
//                services.AddSingleton<MyType2>();
//                services.AddSingleton<CustomProvider1>();
//                services.AddSingleton<CustomProvider2>();
//            })
//            .Build();

//        return host;
//    }

//    public record EntityModel1
//    {
//        public string Name { get; init; } = null!;
//        public int Age { get; init; }
//    }

//    public record EntityModel2
//    {
//        public string City { get; init; } = null!;
//        public DateTime Date { get; init; }
//    }

//    public class MyType1
//    {
//        public MyType1(IDataClient<EntityModel1> handler)
//        {
//            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
//        }

//        public IDataClient<EntityModel1> Handler { get; }
//    }

//    public class MyType2
//    {
//        public MyType2(IDataClient<EntityModel2> handler)
//        {
//            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
//        }

//        public IDataClient<EntityModel2> Handler { get; }
//    }

//    public class CustomProvider1 : IDataProvider
//    {
//        public IDataProvider? InnerHandler { get; set; }

//        public Task<Option<DataPipelineContext>> Execute(DataPipelineContext dataContext, ScopeContext context)
//        {
//            switch (dataContext.Command)
//            {
//                case DataPipelineCommand.Get:
//                    var result = new EntityModel1 { Name = "CustomerProviderCreated-1", Age = 25 };
//                    dataContext = dataContext with { GetData = [result.ToDataETag()] };
//                    return dataContext.ToOption().ToTaskResult();

//                default:
//                    throw new NotImplementedException();
//            }
//        }
//    }

//    public class CustomProvider2 : IDataProvider
//    {
//        public IDataProvider? InnerHandler { get; set; }

//        public Task<Option<DataPipelineContext>> Execute(DataPipelineContext dataContext, ScopeContext context)
//        {
//            switch (dataContext.Command)
//            {
//                case DataPipelineCommand.Get:
//                    var result = new EntityModel2 { City = "Kirkland", Date = new DateTime(2025, 1, 10) };
//                    dataContext = dataContext with { GetData = [result.ToDataETag()] };
//                    return dataContext.ToOption().ToTaskResult();

//                default:
//                    throw new NotImplementedException();
//            }
//        }
//    }
//}
