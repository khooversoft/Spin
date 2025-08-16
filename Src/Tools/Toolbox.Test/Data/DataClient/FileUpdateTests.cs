//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Toolbox.Data;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Test.Data.Client;

//public class FileUpdateTests
//{
//    private readonly ITestOutputHelper _outputHelper;
//    private const string _basePath = "data/single-file-updates";
//    private const int _count = 100;

//    public FileUpdateTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;


//    [Fact]
//    public async Task Direct()
//    {
//        using var host = await BuildService(false, false);
//        var context = host.Services.CreateContext<FileUpdateTests>();

//        using (var scope = context.LogDuration((t, d) => context.LogInformation("Duration={duration}", d)))
//        {
//            await SingleThreadFileUpdate(host, _count);
//        }
//    }

//    [Fact]
//    public async Task WithCache()
//    {
//        using var host = await BuildService(true, false);
//        var context = host.Services.CreateContext<FileUpdateTests>();

//        using (var scope = context.LogDuration((t, d) => context.LogInformation("Duration={duration}", d)))
//        {
//            await SingleThreadFileUpdate(host, _count);
//        }
//    }


//    [Fact]
//    public async Task WithQueue()
//    {
//        using var host = await BuildService(false, true);
//        var context = host.Services.CreateContext<FileUpdateTests>();

//        using (var scope = context.LogDuration((t, d) => context.LogInformation("Duration={duration}", d)))
//        {
//            await SingleThreadFileUpdate(host, _count);
//        }
//    }

//    [Fact]
//    public async Task WithCacheQueue()
//    {
//        using var host = await BuildService(true, true);
//        var context = host.Services.CreateContext<FileUpdateTests>();

//        using (var scope = context.LogDuration((t, d) => context.LogInformation("Duration={duration}", d)))
//        {
//            await SingleThreadFileUpdate(host, _count);
//        }
//    }

//    private async Task SingleThreadFileUpdate(IHost host, int count)
//    {
//        const string key = nameof(SingleThreadFileUpdate);
//        var context = host.Services.CreateContext<FileUpdateTests>();

//        host.NotNull();
//        IDataClient<EntityMaster> dataClient = host.Services.GetDataClient<EntityMaster>();

//        var entityMaster = new EntityMaster();

//        context.LogWarning("SingleThreadFileUpdate: Writing first");
//        await write(entityMaster);
//        foreach (var index in Enumerable.Range(0, count))
//        {
//            entityMaster.Entities.Add(new EntityModel
//            {
//                Name = $"Name-{index}",
//                Index = index
//            });

//            context.LogWarning("SingleThreadFileUpdate: Writing others");
//            await write(entityMaster);
//        }

//        async Task write(EntityMaster subject)
//        {
//            context.LogWarning("SingleThreadFileUpdate: Set, count={count}", subject.Entities.Count);
//            var result = await dataClient.Set(key, subject, context).ConfigureAwait(false);
//            result.BeOk();

//            context.LogWarning("SingleThreadFileUpdate: Get");
//            var readOption = await dataClient.Get(key, context).ConfigureAwait(false);
//            readOption.BeOk();
//            context.LogWarning("SingleThreadFileUpdate: Get, count={count}", readOption.Return().Entities.Count);
//            (readOption.Return() == subject).BeTrue($"Failed to match sourceCount={subject.Entities.Count}, readCount={readOption.Return().Entities.Count}");
//        }
//    }

//    protected virtual void AddStore(IServiceCollection services) => services.AddInMemoryFileStore();

//    private async Task<IHost> BuildService(bool addCache, bool addQueue)
//    {

//        var host = Host.CreateDefaultBuilder()
//            .ConfigureServices((context, services) =>
//            {
//                services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));
//                AddStore(services);
//                services.AddDataPipeline<EntityMaster>(builder =>
//                {
//                    builder.MemoryCacheDuration = TimeSpan.FromSeconds(100);
//                    builder.BasePath = _basePath;

//                    if (addCache) builder.AddCacheMemory();
//                    if (addQueue) builder.AddQueueStore();
//                    builder.AddFileStore();
//                });
//            })
//            .Build();

//        await host.ClearStore<FileUpdateTests>();
//        return host;
//    }


//    public sealed record class EntityMaster
//    {
//        public List<EntityModel> Entities { get; init; } = new List<EntityModel>();

//        public bool Equals(EntityMaster? other)
//        {
//            if (other is null) return false;
//            if (ReferenceEquals(this, other)) return true;
//            return Entities.SequenceEqual(other.Entities);
//        }

//        public override int GetHashCode() => HashCode.Combine(Entities);
//    }

//    public record EntityModel
//    {
//        public string Key { get; init; } = Guid.NewGuid().ToString();
//        public DateTime Date { get; init; } = DateTime.UtcNow;
//        public string Name { get; init; } = null!;
//        public int Index { get; init; }
//    }
//}
