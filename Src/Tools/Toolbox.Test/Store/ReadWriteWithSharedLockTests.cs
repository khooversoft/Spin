using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store;

public class ReadWriteWithSharedLockTests
{
    private readonly ITestOutputHelper _outputHelper;
    private const string _basePath = nameof(LockManagerExclusiveTests);
    private record Entity(string Name, int Age, bool Failed = false);

    public ReadWriteWithSharedLockTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

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
    public async Task ReadWriteWithSharedLock()
    {
        using var host = await BuildService();
        var lockManager = host.Services.GetRequiredService<LockManager>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var context = host.Services.GetRequiredService<ILogger<LockManagerSharedTests>>().ToScopeContext();
        const string path = "data2/ReadWriteWithSharedLock/test-lock.json";
        ManualResetEventSlim manualEvent = new ManualResetEventSlim();
        ConcurrentQueue<Entity> queue = new ConcurrentQueue<Entity>();

        // Deterministic iteration counts
        const int sharedIterations = 30;
        const int directIterations = 30;

        // Acquire initial shared lock to create the file (zero bytes), then release
        (await lockManager.ProcessLock(path, LockMode.Shared, context)).BeOk();
        (await lockManager.ReleaseLock(path, context)).BeOk();

        Task sharedWriteTask = Task.Run(async () => await ReadWriteShare(
            manualEvent, lockManager, path, queue, sharedIterations, context));

        Task readWriteTask = Task.Run(async () => await ReadWrite(
            manualEvent, fileStore, path, queue, directIterations, context));

        manualEvent.Set();

        await Task.WhenAll(sharedWriteTask, readWriteTask);

        // Ensure no lingering lock
        (await lockManager.IsLocked(path, context)).BeOk().Return().BeFalse();

        // Clean up
        (await fileStore.File(path).Delete(context)).BeOk();

        // Validate counts: each iteration enqueues one entity (plus possible error entities)
        queue.Count.Assert(x => x >= sharedIterations + directIterations, "Queue should contain all iterations");

        var summary = queue
            .Where(x => !x.Failed)
            .Select(x => x.Age < 2000 ? 0 : 1)
            .GroupBy(x => x)
            .ToDictionary(g => g.Key, g => g.Count());

        summary.Count.Be(2);
        summary[0].Be(sharedIterations);
        summary[1].Be(directIterations);

        // Optional: ensure error rate is zero (or very low)
        var failures = queue.Count(x => x.Failed);
        failures.Assert(x => x == 0, $"Failures detected: {failures}");
    }

    private static async Task ReadWriteShare(
        ManualResetEventSlim manualEvent,
        LockManager lockManager,
        string path,
        ConcurrentQueue<Entity> queue,
        int iterations,
        ScopeContext context
        )
    {
        manualEvent.Wait();
        context.LogDebug("Released shared writer to run");

        for (int index = 0; index < iterations; index++)
        {
            var entity = new Entity("Test-ReadWriteWithSharedLock-1", 1000 + index);

            // Acquire/release each iteration with safety
            (await lockManager.ProcessLock(path, LockMode.Shared, context)).BeOk();

            try
            {
                // Shared read/write access while holding shared lock
                var access = lockManager.GetReadWriteAccess(path, context);
                (await access.Set(entity.ToDataETag(), context)).BeOk();

                (await access.Get(context)).Action(x =>
                {
                    x.BeOk();
                    DataETag data = x.Return();
                    data.Data.Length.Assert(v => v > 0, $"Data should not be empty, index={index}");
                    var readEntity = data.ToObject<Entity>();
                    (readEntity == entity).BeTrue();
                });

                queue.Enqueue(entity);
                context.LogDebug("Shared writer iteration={index}", index);
            }
            finally
            {
                // Always release even if assertion fails
                (await lockManager.ReleaseLock(path, context)).BeOk();
            }

            await Task.Delay(TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(50, 150)));
        }
    }

    private static async Task ReadWrite(
        ManualResetEventSlim manualEvent,
        IFileStore fileStore,
        string path,
        ConcurrentQueue<Entity> queue,
        int iterations,
        ScopeContext context
        )
    {
        manualEvent.Wait();
        context.LogDebug("Released direct writer to run");

        for (int index = 0; index < iterations; index++)
        {
            var entity = new Entity("Test-ReadWriteWithSharedLock-2", 2000 + index);

            // Attempt to write until not locked (deterministic max attempts)
            for (int attempt = 0; attempt < 20; attempt++)
            {
                var setResult = await fileStore.File(path).Set(entity.ToDataETag(), context);
                if (setResult.IsOk()) break;
                if (attempt == 19) setResult.BeOk(); // Force failure visibility
                await Task.Delay(10);
            }

            (await fileStore.File(path).Get(context)).Action(x =>
            {
                x.BeOk();
                var data = x.Return();
                if (data.Data.Length == 0)
                {
                    context.LogWarning("Direct writer zero data, index={index}", index);
                    queue.Enqueue(new Entity("Test-ReadWriteWithSharedLock-2 - zero data", 2000 + index, true));
                    return;
                }

                var readEntity = data.ToObject<Entity>();
                if (readEntity != entity)
                {
                    context.LogWarning("Direct writer mismatch, index={index}, readEntity={readEntity}, writtenEntity={entity}", index, readEntity, entity);
                    queue.Enqueue(new Entity("Test-ReadWriteWithSharedLock-2 - not equal", 2000 + index, true));
                    return;
                }

                queue.Enqueue(entity);
            });

            context.LogDebug("Direct writer iteration={index}", index);
            await Task.Delay(TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(50, 150)));
        }
    }
}
