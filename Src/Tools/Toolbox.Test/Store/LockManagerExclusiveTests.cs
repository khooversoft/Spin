using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store;

public class LockManagerExclusiveTests
{
    private readonly ITestOutputHelper _outputHelper;
    private const string _basePath = nameof(LockManagerExclusiveTests);
    private record Entity(string Name, int Age);

    public LockManagerExclusiveTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

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
        await host.ClearStore<LockManagerExclusiveTests>();
        return host;
    }

    [Fact]
    public async Task AcquireAndReleaseLock()
    {
        using var host = await BuildService();
        var lockManager = host.Services.GetRequiredService<LockManager>();
        var context = host.Services.GetRequiredService<ILogger<LockManagerExclusiveTests>>().ToScopeContext();
        const string path = "data/AcquireAndReleaseLock/test-lock.json";

        // Acquire lock
        var acquireOption = await lockManager.ProcessLock(path, LockMode.Exclusive, context);
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
    public async Task TryReadWriteWithExclusive()
    {
        using var host = await BuildService();
        var lockManager = host.Services.GetRequiredService<LockManager>();
        var context = host.Services.GetRequiredService<ILogger<LockManagerExclusiveTests>>().ToScopeContext();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        const string path = "data/TryReadWriteWithExclusive/test-lock.json";

        var entity = new Entity("Test", 30);

        // Acquire lock, acquiring a lock on a none-existing file will create the file with 0 bytes
        (await lockManager.ProcessLock(path, LockMode.Exclusive, context)).BeOk();

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
    public async Task LockExclusiveAbleToRead()
    {
        using var host = await BuildService();
        var lockManager = host.Services.GetRequiredService<LockManager>();
        var context = host.Services.GetRequiredService<ILogger<LockManagerExclusiveTests>>().ToScopeContext();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        const string path = "data/LockExclusiveAbleToRead/test-lock.json";

        var entity = new Entity("Test", 30);

        // Acquire lock, acquiring a lock on a none-existing file will create the file with 0 bytes
        (await lockManager.ProcessLock(path, LockMode.Exclusive, context)).BeOk();

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
}
