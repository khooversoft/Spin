using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.Memory;

public class MemoryKeyStoreTests
{
    private readonly ITestOutputHelper _output;
    public MemoryKeyStoreTests(ITestOutputHelper output) => _output = output.NotNull();

    private IHost BuildHost()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddLogging(c => c.AddLambda(_output.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddSingleton<MemoryStore>();
                services.AddSingleton<MemoryKeyStore>();
            })
            .Build();

        return host;
    }

    [Fact]
    public async Task MemoryKeyStore_RoundTrip()
    {
        using var host = BuildHost();
        var memoryKeyStore = host.Services.GetRequiredService<MemoryKeyStore>();

        string path = "test/key1.txt";
        DataETag data = "Hello, World!".ToBytes().ToDataETag();

        var add = await memoryKeyStore.Add(path, data);
        add.BeOk();

        var get = await memoryKeyStore.Get(path);
        get.BeOk();
        get.Return().Data.SequenceEqual(data.Data).BeTrue();

        var search = await memoryKeyStore.Search("test/**.*");
        search.Count.Be(1);
        search[0].Path.Be(path);

        var delete = await memoryKeyStore.Delete(path);
        delete.BeOk();

        search = await memoryKeyStore.Search("test/**.*");
        search.Count.Be(0);
    }

    [Fact]
    public async Task MemoryKeyStore_Set_UpdatesExistingData()
    {
        using var host = BuildHost();
        var memoryKeyStore = host.Services.GetRequiredService<MemoryKeyStore>();

        string path = "test/key1.txt";
        DataETag data1 = "Hello".ToBytes().ToDataETag();
        DataETag data2 = "World".ToBytes().ToDataETag();

        (await memoryKeyStore.Add(path, data1)).BeOk();
        (await memoryKeyStore.Set(path, data2)).BeOk();

        var get = await memoryKeyStore.Get(path);
        get.BeOk();
        get.Return().Data.SequenceEqual(data2.Data).BeTrue();
    }

    [Fact]
    public async Task MemoryKeyStore_Append_AddsToExistingData()
    {
        using var host = BuildHost();
        var memoryKeyStore = host.Services.GetRequiredService<MemoryKeyStore>();

        string path = "test/append.txt";
        DataETag data1 = "Hello ".ToBytes().ToDataETag();
        DataETag data2 = "World".ToBytes().ToDataETag();

        (await memoryKeyStore.Add(path, data1)).BeOk();
        (await memoryKeyStore.Append(path, data2)).BeOk();

        var get = await memoryKeyStore.Get(path);
        get.BeOk();
        var expected = "Hello World".ToBytes();
        get.Return().Data.SequenceEqual(expected).BeTrue();
    }

    [Fact]
    public async Task MemoryKeyStore_Exists_ReturnsCorrectStatus()
    {
        using var host = BuildHost();
        var memoryKeyStore = host.Services.GetRequiredService<MemoryKeyStore>();

        string path = "test/exists.txt";

        (await memoryKeyStore.Exists(path)).StatusCode.Be(StatusCode.NotFound);

        await memoryKeyStore.Add(path, "data".ToBytes().ToDataETag());
        (await memoryKeyStore.Exists(path)).BeOk();

        await memoryKeyStore.Delete(path);
        (await memoryKeyStore.Exists(path)).StatusCode.Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task MemoryKeyStore_GetDetails_ReturnsPathDetail()
    {
        using var host = BuildHost();
        var memoryKeyStore = host.Services.GetRequiredService<MemoryKeyStore>();

        string path = "test/details.txt";
        DataETag data = "test data".ToBytes().ToDataETag();

        await memoryKeyStore.Add(path, data);

        var details = await memoryKeyStore.GetDetails(path);
        details.BeOk();
        details.Return().Path.Be(path);
        details.Return().ETag.NotEmpty();
    }

    [Fact]
    public async Task MemoryKeyStore_AcquireExclusiveLock_WithBreakLease()
    {
        using var host = BuildHost();
        var memoryKeyStore = host.Services.GetRequiredService<MemoryKeyStore>();

        string path = "test/lock.txt";
        await memoryKeyStore.Add(path, "data".ToBytes().ToDataETag());

        var leaseId = await memoryKeyStore.AcquireExclusiveLock(path, true);
        leaseId.BeOk();
        leaseId.Return().NotEmpty();
    }

    [Fact]
    public async Task MemoryKeyStore_AcquireLease_PreventsConcurrentModification()
    {
        using var host = BuildHost();
        var memoryKeyStore = host.Services.GetRequiredService<MemoryKeyStore>();

        string path = "test/lease.txt";
        await memoryKeyStore.Add(path, "data".ToBytes().ToDataETag());

        var leaseId = await memoryKeyStore.AcquireLease(path, TimeSpan.FromSeconds(10));
        leaseId.BeOk();

        // Second acquire should fail
        var leaseId2 = await memoryKeyStore.AcquireLease(path, TimeSpan.FromSeconds(10));
        leaseId2.StatusCode.Be(StatusCode.Locked);
    }

    [Fact]
    public async Task MemoryKeyStore_BreakLease_ReleasesLock()
    {
        using var host = BuildHost();
        var memoryKeyStore = host.Services.GetRequiredService<MemoryKeyStore>();

        string path = "test/breaklease.txt";
        await memoryKeyStore.Add(path, "data".ToBytes().ToDataETag());

        var leaseId = await memoryKeyStore.AcquireLease(path, TimeSpan.FromSeconds(60));
        leaseId.BeOk();

        await memoryKeyStore.BreakLease(path);

        // Should be able to acquire again after break
        var leaseId2 = await memoryKeyStore.AcquireLease(path, TimeSpan.FromSeconds(10));
        leaseId2.BeOk();
    }

    [Fact]
    public async Task MemoryKeyStore_Add_Conflict_WhenDuplicatePath()
    {
        using var host = BuildHost();
        var memoryKeyStore = host.Services.GetRequiredService<MemoryKeyStore>();

        string path = "test/duplicate.txt";
        DataETag data = "data".ToBytes().ToDataETag();

        (await memoryKeyStore.Add(path, data)).BeOk();
        (await memoryKeyStore.Add(path, data)).StatusCode.Be(StatusCode.Conflict);
    }
}
