using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.test.Application;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Datalake;

public class DatalakeStore_LeaseTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public DatalakeStore_LeaseTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private async Task<IHost> BuildService([CallerMemberName] string function = "")
    {
        var option = TestApplication.ReadOption(nameof(DatalakeStore_LeaseTests) + "/" + function);

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddDatalakeFileStore(option);
            })
            .Build();

        // Clear the store before running tests, this includes any locked files
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();
        await keyStore.ClearStore();
        (await keyStore.Search("**.*")).Count().Be(0);
        return host;
    }


    [Fact]
    public async Task AcquireLease_WithDuration_ShouldLockFile()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "lease-test.json";
        var testRecord = new TestRecord("Leased User", 40);

        (await keyStore.Add(path, testRecord)).BeOk();

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromSeconds(30));
        leaseOption.BeOk();
        string leaseId = leaseOption.Return();
        leaseId.NotEmpty();

        (await keyStore.Delete(path)).StatusCode.Be(StatusCode.Locked);

        (await keyStore.ReleaseLease(path, leaseId)).BeOk();
        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task AcquireExclusiveLock_ShouldLockFileIndefinitely()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "exclusive-lock-test.json";
        var testRecord = new TestRecord("Locked User", 45);

        (await keyStore.Add(path, testRecord)).BeOk();

        var leaseOption = await keyStore.AcquireExclusiveLock(path, breakLeaseIfExist: false);
        leaseOption.BeOk();
        string leaseId = leaseOption.Return();
        leaseId.NotEmpty();

        (await keyStore.Delete(path)).StatusCode.Be(StatusCode.Locked);

        (await keyStore.ReleaseLease(path, leaseId)).BeOk();
        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task RenewLease_WithValidLease_ShouldKeepFileLocked()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "renew-lease-valid.json";
        var testRecord = new TestRecord("Renew User", 55);

        (await keyStore.Add(path, testRecord)).BeOk();
        string leaseId = (await keyStore.AcquireLease(path, TimeSpan.FromSeconds(15))).BeOk().Return();

        (await keyStore.RenewLease(path, leaseId)).BeOk();

        (await keyStore.Delete(path)).StatusCode.Be(StatusCode.Locked);

        (await keyStore.ReleaseLease(path, leaseId)).BeOk();
        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task RenewLease_WithInvalidLeaseId_ShouldReturnNotFound()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "renew-lease-invalid.json";
        var testRecord = new TestRecord("Renew Invalid User", 60);

        (await keyStore.Add(path, testRecord)).BeOk();
        string leaseId = (await keyStore.AcquireLease(path, TimeSpan.FromSeconds(15))).BeOk().Return();

        (await keyStore.RenewLease(path, Guid.NewGuid().ToString())).BeNotFound();

        (await keyStore.ReleaseLease(path, leaseId)).BeOk();
        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task RenewLease_OnNonExistentFile_ShouldReturnNotFound()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "renew-lease-missing.json";

        (await keyStore.RenewLease(path, Guid.NewGuid().ToString())).BeNotFound();
    }

    [Fact]
    public async Task BreakLease_ShouldReleaseLockedFile()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "break-lease-test.json";
        var testRecord = new TestRecord("Locked User", 50);

        (await keyStore.Add(path, testRecord)).BeOk();

        var leaseOption = await keyStore.AcquireExclusiveLock(path, breakLeaseIfExist: false);
        leaseOption.BeOk();

        (await keyStore.Delete(path)).StatusCode.Be(StatusCode.Locked);

        (await keyStore.BreakLease(path)).BeOk();

        await Task.Delay(1000); // Wait for lease to break

        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task Set_WithLease_ShouldSucceed()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "set-lease-test.json";
        var record1 = new TestRecord("First", 10);
        var record2 = new TestRecord("Second", 20);

        (await keyStore.Add(path, record1)).BeOk();

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromSeconds(30));
        leaseOption.BeOk();
        string leaseId = leaseOption.Return();

        (await keyStore.Set(path, record2.ToDataETag(), leaseId)).BeOk();
        (await keyStore.Get<TestRecord>(path)).BeOk().Return().Be(record2);

        (await keyStore.ReleaseLease(path, leaseId)).BeOk();
        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task AcquireLease_OnNonExistentFile_ShouldCreateAndLock()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "non-existent-lease.json";

        (await keyStore.Exists(path)).BeNotFound();

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromSeconds(30));
        leaseOption.BeOk();
        string leaseId = leaseOption.Return();
        leaseId.NotEmpty();

        (await keyStore.Exists(path)).BeOk();

        (await keyStore.ReleaseLease(path, leaseId)).BeOk();
        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task AcquireExclusiveLock_WithBreakLease_ShouldBreakExistingLease()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "break-existing-lock.json";
        var testRecord = new TestRecord("Test User", 30);

        (await keyStore.Add(path, testRecord)).BeOk();

        var firstLeaseOption = await keyStore.AcquireExclusiveLock(path, breakLeaseIfExist: false);
        firstLeaseOption.BeOk();
        string firstLeaseId = firstLeaseOption.Return();

        var secondLeaseOption = await keyStore.AcquireExclusiveLock(path, breakLeaseIfExist: true);
        secondLeaseOption.BeOk();
        string secondLeaseId = secondLeaseOption.Return();
        secondLeaseId.NotEmpty();
        secondLeaseId.Assert(x => x != firstLeaseId, "Second lease should be different from first");

        (await keyStore.ReleaseLease(path, secondLeaseId)).BeOk();
        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task Delete_WithValidLease_ShouldSucceed()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "delete-with-lease.json";
        var testRecord = new TestRecord("Delete Test", 25);

        (await keyStore.Add(path, testRecord)).BeOk();
        string leaseId = (await keyStore.AcquireLease(path, TimeSpan.FromSeconds(30))).BeOk().Return();

        (await keyStore.Delete(path, leaseId)).BeOk();
        (await keyStore.Exists(path)).BeNotFound();
    }

    [Fact]
    public async Task Delete_WithInvalidLease_ShouldReturnLocked()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "delete-invalid-lease.json";
        var testRecord = new TestRecord("Invalid Lease Test", 35);

        (await keyStore.Add(path, testRecord)).BeOk();

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromSeconds(30));
        leaseOption.BeOk();
        string leaseId = leaseOption.Return();

        (await keyStore.Delete(path, Guid.NewGuid().ToString())).StatusCode.Be(StatusCode.Locked);

        (await keyStore.ReleaseLease(path, leaseId)).BeOk();
        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task BreakLease_OnNonExistentFile_ShouldSucceed()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "non-existent-break.json";

        (await keyStore.BreakLease(path)).BeOk();
    }

    [Fact]
    public async Task BreakLease_OnUnlockedFile_ShouldSucceed()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "unlocked-break.json";
        var testRecord = new TestRecord("Unlocked User", 40);

        (await keyStore.Add(path, testRecord)).BeOk();

        (await keyStore.BreakLease(path)).BeOk();

        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task ReleaseLease_OnNonExistentFile_ShouldSucceed()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "non-existent-release.json";

        (await keyStore.ReleaseLease(path, "any-lease-id")).BeOk();
    }

    [Fact]
    public async Task Set_WithoutLease_OnLockedFile_ShouldReturnLocked()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "locked-set-test.json";
        var record1 = new TestRecord("First", 10);
        var record2 = new TestRecord("Second", 20);

        (await keyStore.Add(path, record1)).BeOk();

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromSeconds(30));
        leaseOption.BeOk();
        string leaseId = leaseOption.Return();

        (await keyStore.Set(path, record2.ToDataETag())).StatusCode.Be(StatusCode.Locked);

        (await keyStore.ReleaseLease(path, leaseId)).BeOk();
        (await keyStore.Delete(path)).BeOk();
    }
}
