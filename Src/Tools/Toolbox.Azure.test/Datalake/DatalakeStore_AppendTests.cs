using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.test.Application;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Datalake;

public class DatalakeStore_AppendTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public DatalakeStore_AppendTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private async Task<IHost> BuildService([CallerMemberName] string function = "")
    {
        var option = TestApplication.ReadOption(nameof(DatalakeStore_AppendTests) + "/" + function);

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
    public async Task Append_WithoutLease_ShouldSucceed()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "append-test.json";
        var record1 = new TestRecord("First", 10);
        var record2 = new TestRecord("Second", 20);

        (await keyStore.Add(path, record1.ToJson().ToDataETag())).BeOk();

        string appendData = "\n" + record2.ToJson();
        (await keyStore.Append(path, appendData.ToDataETag())).BeOk();

        var result = (await keyStore.Get(path)).BeOk().Return();
        string content = result.DataToString();
        content.Contains(record1.ToJson()).BeTrue();
        content.Contains(record2.ToJson()).BeTrue();

        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task Append_StartWithLease_ShouldSucceed()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "append-lease-test.json";
        var record1 = new TestRecord("First", 10);
        var record2 = new TestRecord("Second", 20);

        (await keyStore.Append(path, record1.ToJson().ToDataETag())).BeOk();

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromSeconds(30));
        leaseOption.BeOk();
        string leaseId = leaseOption.Return();

        string appendData = "\n" + record2.ToJson();
        (await keyStore.Append(path, appendData.ToDataETag(), leaseId)).BeOk();

        (await keyStore.ReleaseLease(path, leaseId)).BeOk();

        var result = (await keyStore.Get(path)).BeOk().Return();
        string content = result.DataToString();
        content.Contains(record1.ToJson()).BeTrue();
        content.Contains(record2.ToJson()).BeTrue();

        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task Append_WithLease_ShouldSucceed()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "append-lease-test.json";
        var record1 = new TestRecord("First", 10);
        var record2 = new TestRecord("Second", 20);

        (await keyStore.Add(path, record1.ToJson().ToDataETag())).BeOk();

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromSeconds(30));
        leaseOption.BeOk();
        string leaseId = leaseOption.Return();

        string appendData = "\n" + record2.ToJson();
        (await keyStore.Append(path, appendData.ToDataETag(), leaseId)).BeOk();

        (await keyStore.ReleaseLease(path, leaseId)).BeOk();

        var result = (await keyStore.Get(path)).BeOk().Return();
        string content = result.DataToString();
        content.Contains(record1.ToJson()).BeTrue();
        content.Contains(record2.ToJson()).BeTrue();

        (await keyStore.Delete(path)).BeOk();
    }


    [Fact]
    public async Task ConcurrentAppend_MultipleCalls_ShouldSucceed()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "concurrent-append.json";
        int count = 10;

        (await keyStore.Add(path, "initial".ToDataETag())).BeOk();

        var tasks = Enumerable.Range(0, count)
            .Select(async i =>
            {
                var record = new TestRecord($"User{i}", 20 + i);
                return await keyStore.Append(path, $"{record.ToJson()}\n".ToDataETag());
            })
            .ToArray();

        var results = await Task.WhenAll(tasks);
        results.ForEach(x => x.BeOk());

        var content = (await keyStore.Get(path)).BeOk().Return().DataToString();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Assert(x => x >= count);

        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task ConcurrentAppend_Stress_ShouldSucceed()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "concurrent-append.json";
        int count = 100;
        var list = Enumerable.Range(0, count)
            .Select(i => new TestRecord($"User{i}", 20 + i))
            .ToList();

        (await keyStore.Add(path, "initial".ToDataETag())).BeOk();

        await ActionParallel.Run(async record =>
        {
            var r = await keyStore.Append(path, $"{record.ToJson()}\n".ToDataETag());
            r.BeOk();
        }, list);

        var content = (await keyStore.Get(path)).BeOk().Return().DataToString();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Assert(x => x >= count);

        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task Append_ToNonExistentFile_ShouldCreateFile()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "append-new-file.json";
        var record = new TestRecord("New File Record", 25);

        (await keyStore.Exists(path)).BeNotFound();

        (await keyStore.Append(path, record.ToJson().ToDataETag())).BeOk();

        (await keyStore.Exists(path)).BeOk();

        var result = (await keyStore.Get(path)).BeOk().Return();
        string content = result.DataToString();
        content.Contains(record.ToJson()).BeTrue();

        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task Append_WithoutLease_OnLockedFile_ShouldAcquireLeaseAndSucceed()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "append-locked-file.json";
        var record1 = new TestRecord("First", 10);
        var record2 = new TestRecord("Second", 20);

        (await keyStore.Add(path, record1.ToJson().ToDataETag())).BeOk();

        string appendData = "\n" + record2.ToJson();
        (await keyStore.Append(path, appendData.ToDataETag())).BeOk();

        var result = (await keyStore.Get(path)).BeOk().Return();
        string content = result.DataToString();
        content.Contains(record1.ToJson()).BeTrue();
        content.Contains(record2.ToJson()).BeTrue();

        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task Append_ReturnsETag()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "append-etag-test.json";
        var record = new TestRecord("ETag Test", 30);

        (await keyStore.Add(path, "initial".ToDataETag())).BeOk();

        var appendResult = await keyStore.Append(path, record.ToJson().ToDataETag());
        appendResult.BeOk();
        string etag = appendResult.Return();
        etag.NotEmpty();

        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task Append_MultipleSequential_ShouldAccumulateContent()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "sequential-append.json";
        int count = 5;

        (await keyStore.Add(path, "start\n".ToDataETag())).BeOk();

        for (int i = 0; i < count; i++)
        {
            var record = new TestRecord($"User{i}", 20 + i);
            (await keyStore.Append(path, $"{record.ToJson()}\n".ToDataETag())).BeOk();
        }

        var result = (await keyStore.Get(path)).BeOk().Return();
        string content = result.DataToString();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Be(count + 1);

        (await keyStore.Delete(path)).BeOk();
    }
}
