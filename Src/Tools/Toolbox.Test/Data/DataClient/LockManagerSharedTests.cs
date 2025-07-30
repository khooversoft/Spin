using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Data.DataClient;

public class LockManagerSharedTests
{
    private readonly ITestOutputHelper _outputHelper;
    private const string _basePath = nameof(LockManagerExclusiveTests);
    private record Entity(string Name, int Age, bool Failed = false);

    public LockManagerSharedTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    protected virtual void AddStore(IServiceCollection services) => services.AddInMemoryFileStore();

    private async Task<IHost> BuildService()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));
                AddStore(services);
                services.AddSingleton<LockManager>();
            })
            .Build();

        // Clear the store before running tests, this includes any locked files
        await host.ClearStore<LockManagerSharedTests>();
        return host;
    }

    [Fact]
    public async Task AcquireAndReleaseLock()
    {
        using var host = await BuildService();
        var lockManager = host.Services.GetRequiredService<LockManager>();
        var context = host.Services.GetRequiredService<ILogger<LockManagerSharedTests>>().ToScopeContext();
        const string path = "data2/AcquireAndReleaseLock/test-lock.json";

        // Acquire lock
        var acquireOption = await lockManager.ProcessLock(path, LockMode.Shared, context);
        acquireOption.BeOk();

        // Verify lock is acquired
        var isLocked = await lockManager.IsLocked(path, context);
        isLocked.BeOk();

        // Release lock
        var releaseOption = await lockManager.ReleaseLock(path, context);
        releaseOption.BeOk();

        // Verify lock is released
        isLocked = await lockManager.IsLocked(path, context);
        isLocked.BeOk();
    }

    [Fact]
    public async Task TryReadWriteWithShared()
    {
        using var host = await BuildService();
        var lockManager = host.Services.GetRequiredService<LockManager>();
        var context = host.Services.GetRequiredService<ILogger<LockManagerSharedTests>>().ToScopeContext();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        const string path = "data2/TryReadWriteWithExclusive/test-lock.json";

        var entity = new Entity("Test", 30);

        // Acquire lock, acquiring a lock on a none-existing file will create the file with 0 bytes
        (await lockManager.ProcessLock(path, LockMode.Shared, context)).BeOk();

        // Try to write
        (await fileStore.File(path).Set(entity.ToDataETag(), context)).IsLocked().BeTrue();

        // Verify file is created with 0 bytes from the acquire operations
        (await fileStore.File(path).Get(context)).BeOk().Assert(x => x.Return().Data.Length == 0, _ => "Not zero size");

        // Verify lock is acquired
        (await lockManager.IsLocked(path, context)).BeOk().Return().BeTrue();

        // Release lock
        (await lockManager.ReleaseLock(path, context)).BeOk();

        // Verify lock is released
        (await lockManager.IsLocked(path, context)).BeOk().Return().BeFalse();

        (await fileStore.File(path).Set(entity.ToDataETag(), context)).BeOk();

        (await fileStore.File(path).Get(context)).Action(x =>
        {
            x.BeOk();
            var readEntity = x.Return().ToObject<Entity>();
            (readEntity == entity).BeTrue();
        });

        var deleteOption = await fileStore.File(path).Delete(context);
        deleteOption.BeOk();
    }

    [Fact]
    public async Task LockedFileAbleToRead()
    {
        using var host = await BuildService();
        var lockManager = host.Services.GetRequiredService<LockManager>();
        var context = host.Services.GetRequiredService<ILogger<LockManagerSharedTests>>().ToScopeContext();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        const string path = "data2/LockExclusiveAbleToRead/test-lock.json";

        var entity = new Entity("Test", 30);

        // Acquire lock, acquiring a lock on a none-existing file will create the file with 0 bytes
        (await lockManager.ProcessLock(path, LockMode.Shared, context)).BeOk();

        // Verify file is created with 0 bytes from the acquire operations
        (await fileStore.File(path).Get(context)).BeOk().Func(x => x.Return()).Assert(x => x.Data.Length == 0, x => $"{x.Data.Length} not zero size");

        // Try to write via access
        var access = lockManager.GetReadWriteAccess(path, context);
        (await access.Set(entity.ToDataETag(), context)).BeOk();

        (await access.Get(context)).Action(x =>
        {
            x.BeOk();
            var readEntity = x.Return().ToObject<Entity>();
            (readEntity == entity).BeTrue();
        });
        (await fileStore.File(path).Get(context)).Action(x =>
        {
            x.BeOk();
            var readEntity = x.Return().ToObject<Entity>();
            (readEntity == entity).BeTrue();
        });

        // Write another update
        access = lockManager.GetReadWriteAccess(path, context);
        entity = new Entity("Test2", 41);
        (await access.Set(entity.ToDataETag(), context)).BeOk();

        (await access.Get(context)).Action(x =>
        {
            x.BeOk();
            var readEntity = x.Return().ToObject<Entity>();
            (readEntity == entity).BeTrue();
        });
        (await fileStore.File(path).Get(context)).Action(x =>
        {
            x.BeOk();
            var readEntity = x.Return().ToObject<Entity>();
            (readEntity == entity).BeTrue();
        });

        // Verify lock is acquired
        (await lockManager.IsLocked(path, context)).BeOk().Return().BeTrue();

        // Release lock
        (await lockManager.ReleaseLock(path, context)).BeOk();

        // Verify lock is released
        (await lockManager.IsLocked(path, context)).BeOk().Return().BeFalse();

        // Write a 3 update, file is unlocked
        entity = new Entity("Test3", 52);
        (await fileStore.File(path).Set(entity.ToDataETag(), context)).BeOk();

        (await fileStore.File(path).Get(context)).Action(x =>
        {
            x.BeOk();
            var readEntity = x.Return().ToObject<Entity>();
            (readEntity == entity).BeTrue();
        });

        (await fileStore.File(path).Delete(context)).BeOk();
    }

    [Fact]
    public async Task ReadWriteWithSharedLock()
    {
        using var host = await BuildService();
        var lockManager = host.Services.GetRequiredService<LockManager>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var context = host.Services.GetRequiredService<ILogger<LockManagerSharedTests>>().ToScopeContext();
        const string path = "data2/ReadWriteWithSharedLock/test-lock.json";
        ManualResetEventSlim manualEvent = new ManualResetEventSlim();
        ConcurrentQueue<Entity> queue = new ConcurrentQueue<Entity>();

        CancellationTokenSource tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Acquire shared lock
        (await lockManager.ProcessLock(path, LockMode.Shared, context)).BeOk();

        Task sharedWriteTask = Task.Run(async () => await ReadWriteShare(manualEvent, lockManager, path, queue, tokenSource.Token, context));
        Task readWriteTask = Task.Run(async () => await ReadWrite(manualEvent, fileStore, path, queue, tokenSource.Token, context));
        manualEvent.Set();

        await Task.WhenAll(sharedWriteTask, readWriteTask);

        // Verify lock is released
        (await lockManager.IsLocked(path, context)).BeOk().Return().BeFalse();

        (await fileStore.File(path).Delete(context)).BeOk();

        queue.Count.Assert(x => x > 0, "Queue should have items");
        var summary = queue.Select(x => x.Age < 2000 ? 0 : 1).GroupBy(x => x).ToArray();
        summary.Length.Be(2);
        summary[0].Count().Assert(x => x > 10, "1000 missed");
        summary[1].Count().Assert(x => x > 10, "2000 missed");
    }

    private static async Task ReadWriteShare(
        ManualResetEventSlim manualEvent,
        LockManager lockManager,
        string path,
        ConcurrentQueue<Entity> queue,
        CancellationToken token,
        ScopeContext context
        )
    {
        // Wait for the semaphore to be released
        manualEvent.Wait();
        context.LogDebug("Released to run");

        int index = 0;
        while (!token.IsCancellationRequested)
        {
            var entity = new Entity("Test-ReadWriteWithSharedLock-1", 1000 + index++);

            (await lockManager.ProcessLock(path, LockMode.Shared, context)).BeOk();

            // Try to write while holding a shared lock
            var access = lockManager.GetReadWriteAccess(path, context);
            (await access.Set(entity.ToDataETag(), context)).BeOk();

            (await access.Get(context)).Action(x =>
            {
                x.BeOk();
                DataETag data = x.Return();
                data.Data.Length.Assert(x => x > 0, $"Data should not be empty, index={index}");
                var readEntity = data.ToObject<Entity>();
                (readEntity == entity).BeTrue();
            });

            queue.Enqueue(entity);

            (await lockManager.ReleaseLock(path, context)).BeOk();

            context.LogDebug("Write share, index={index}", index);
            await Task.Delay(TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(500)));
        }
    }

    private static async Task ReadWrite(
        ManualResetEventSlim manualEvent,
        IFileStore fileStore,
        string path,
        ConcurrentQueue<Entity> queue,
        CancellationToken token,
        ScopeContext context
        )
    {
        manualEvent.Wait();
        context.LogDebug("Released to run");

        int index = 0;
        while (!token.IsCancellationRequested)
        {
            var entity = new Entity("Test-ReadWriteWithSharedLock-2", 2000 + index++);

            await WaitForTool.WaitFor(async () => (await fileStore.File(path).Set(entity.ToDataETag(), context)).IsOk(), TimeSpan.FromSeconds(10));

            (await fileStore.File(path).Get(context)).Action(x =>
            {
                x.BeOk();
                var data = x.Return();
                if (data.Data.Length == 0)
                {
                    context.LogWarning("ReadWriteWithSharedLock-2: Data length is zero, index={index}", index);
                    queue.Enqueue(new Entity("Test-ReadWriteWithSharedLock-2 - zero data", 2000 + index - 1, true));
                    return;
                }

                var readEntity = data.ToObject<Entity>();
                if (readEntity != entity)
                {
                    context.LogWarning("ReadWriteWithSharedLock-2: Read entity does not match written entity, index={index}, readEntity={readEntity}, writtenEntity={entity}", index, readEntity, entity);
                    queue.Enqueue(new Entity("Test-ReadWriteWithSharedLock-2 - not equal", 2000 + index - 1, true));
                }
            });

            queue.Enqueue(entity);

            context.LogDebug("Write, index={index}", index);
            await Task.Delay(TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(500)));
        }
    }
}
