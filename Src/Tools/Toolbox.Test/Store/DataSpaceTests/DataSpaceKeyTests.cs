using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.DataSpaceTests;

public class DataSpaceKeyTests
{
    private ITestOutputHelper _outputHelper;

    public DataSpaceKeyTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private async Task<IHost> BuildService()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddInMemoryKeyStore();

                services.AddDataSpace(cnfg =>
                {
                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "file",
                        ProviderName = "fileStore",
                        BasePath = "dataFiles",
                        SpaceFormat = SpaceFormat.Key,
                    });
                    cnfg.Add<KeyStoreProvider>("fileStore");
                });
            })
            .Build();

        // Clear the store before running tests, this includes any locked files
        //await host.ClearStore<FileStoreTransactionTests>();
        return host;
    }

    [Fact]
    public async Task SimpleWriteAndRead()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string path = "test/data.txt";

        var content = "Hello, World!".ToBytes();
        var setResult = await keyStore.Set(path, new DataETag(content), context);
        setResult.BeOk();

        var readOption = await keyStore.Get(path, context);
        readOption.BeOk();
        var readData = readOption.Return().Data;
        content.SequenceEqual(readData).BeTrue();

        var s1 = await keyStore.Search("**.*", context);
        s1.Count.Be(1);
        s1[0].Path.Be(path);

        s1 = await keyStore.Search("test/*.txt", context);
        s1.Count.Be(1);
        s1[0].Path.Be(path);

        var deleteOption = await keyStore.Delete(path, context);
        deleteOption.BeOk();

        var s2 = await keyStore.Search("**.*", context);
        s2.Count.Be(0);
    }

    [Fact]
    public async Task AddOperation_ShouldCreateNewKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string path = "test/add-data.txt";
        var content = "Add operation test".ToBytes();

        var addResult = await keyStore.Add(path, new DataETag(content), context);
        addResult.BeOk();

        var readOption = await keyStore.Get(path, context);
        readOption.BeOk();
        content.SequenceEqual(readOption.Return().Data).BeTrue();

        await keyStore.Delete(path, context);
    }

    [Fact]
    public async Task AddOperation_ShouldFailWhenKeyExists()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string path = "test/duplicate-data.txt";
        var content = "Duplicate test".ToBytes();

        var addResult = await keyStore.Add(path, new DataETag(content), context);
        addResult.BeOk();

        var duplicateResult = await keyStore.Add(path, new DataETag(content), context);
        duplicateResult.IsError().BeTrue();

        await keyStore.Delete(path, context);
    }

    [Fact]
    public async Task AppendOperation_ShouldAppendData()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string path = "test/append-data.txt";
        var content1 = "First line".ToBytes();
        var content2 = "Second line".ToBytes();

        await keyStore.Set(path, new DataETag(content1), context);

        var appendResult = await keyStore.Append(path, new DataETag(content2), context);
        appendResult.BeOk();

        var readOption = await keyStore.Get(path, context);
        readOption.BeOk();
        var combined = content1.Concat(content2).ToArray();
        combined.SequenceEqual(readOption.Return().Data).BeTrue();

        await keyStore.Delete(path, context);
    }

    [Fact]
    public async Task SetOperation_ShouldOverwriteExistingKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string path = "test/overwrite-data.txt";
        var content1 = "Original content".ToBytes();
        var content2 = "Updated content".ToBytes();

        await keyStore.Set(path, new DataETag(content1), context);
        var setResult = await keyStore.Set(path, new DataETag(content2), context);
        setResult.BeOk();

        var readOption = await keyStore.Get(path, context);
        readOption.BeOk();
        content2.SequenceEqual(readOption.Return().Data).BeTrue();

        await keyStore.Delete(path, context);
    }

    [Fact]
    public async Task ExistsOperation_ShouldReturnTrueForExistingKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string path = "test/exists-data.txt";
        var content = "Exists test".ToBytes();

        await keyStore.Set(path, new DataETag(content), context);

        var existsResult = await keyStore.Exists(path, context);
        existsResult.BeOk();

        await keyStore.Delete(path, context);
    }

    [Fact]
    public async Task ExistsOperation_ShouldFailForNonExistingKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string path = "test/non-existing.txt";

        var existsResult = await keyStore.Exists(path, context);
        existsResult.IsError().BeTrue();
    }

    [Fact]
    public async Task GetDetailsOperation_ShouldReturnPathDetails()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string path = "test/details-data.txt";
        var content = "Details test".ToBytes();

        await keyStore.Set(path, new DataETag(content), context);

        var detailsOption = await keyStore.GetDetails(path, context);
        detailsOption.BeOk();
        var details = detailsOption.Return();
        details.Path.Be(path);
        details.IsFolder.BeFalse();

        await keyStore.Delete(path, context);
    }

    [Fact]
    public async Task DeleteFolderOperation_ShouldRemoveFolder()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string folder = "test/folder";
        string path1 = $"{folder}/file1.txt";
        string path2 = $"{folder}/file2.txt";

        await keyStore.Set(path1, new DataETag("File 1".ToBytes()), context);
        await keyStore.Set(path2, new DataETag("File 2".ToBytes()), context);

        var deleteFolderResult = await keyStore.DeleteFolder(folder, context);
        deleteFolderResult.BeOk();

        var searchResult = await keyStore.Search($"{folder}/*.*", context);
        searchResult.Count.Be(0);
    }

    [Fact]
    public async Task SearchOperation_ShouldFindMultipleFiles()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        await keyStore.Set("test/search/file1.txt", new DataETag("Data 1".ToBytes()), context);
        await keyStore.Set("test/search/file2.txt", new DataETag("Data 2".ToBytes()), context);
        await keyStore.Set("test/search/file3.json", new DataETag("Data 3".ToBytes()), context);

        var searchResult = await keyStore.Search("test/search/*.txt", context);
        searchResult.Count.Be(2);

        var allFiles = await keyStore.Search("test/search/*.*", context);
        allFiles.Count.Be(3);

        await keyStore.DeleteFolder("test/search", context);
    }

    [Fact]
    public async Task AcquireLeaseOperation_ShouldLockKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string path = "test/lease-data.txt";
        var content = "Lease test".ToBytes();

        await keyStore.Set(path, new DataETag(content), context);

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromMinutes(1), context);
        leaseOption.BeOk();
        var leaseId = leaseOption.Return();
        leaseId.NotEmpty();

        var releaseResult = await keyStore.Release(leaseId, context);
        releaseResult.BeOk();

        await keyStore.Delete(path, context);
    }

    [Fact]
    public async Task AcquireExclusiveLockOperation_ShouldLockKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string path = "test/exclusive-lock.txt";
        var content = "Exclusive lock test".ToBytes();

        await keyStore.Set(path, new DataETag(content), context);

        var lockOption = await keyStore.AcquireExclusiveLock(path, false, context);
        lockOption.BeOk();
        var leaseId = lockOption.Return();
        leaseId.NotEmpty();

        var releaseResult = await keyStore.Release(leaseId, context);
        releaseResult.BeOk();

        await keyStore.Delete(path, context);
    }

    [Fact]
    public async Task BreakLeaseOperation_ShouldReleaseLockedKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string path = "test/break-lease.txt";
        var content = "Break lease test".ToBytes();

        await keyStore.Set(path, new DataETag(content), context);

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromMinutes(1), context);
        leaseOption.BeOk();

        var breakResult = await keyStore.BreakLease(path, context);
        breakResult.BeOk();

        await keyStore.Delete(path, context);
    }

    [Fact]
    public async Task SetWithLeaseId_ShouldUpdateLockedKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string path = "test/lease-update.txt";
        var content1 = "Original".ToBytes();
        var content2 = "Updated".ToBytes();

        await keyStore.Set(path, new DataETag(content1), context);

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromMinutes(1), context);
        leaseOption.BeOk();
        var leaseId = leaseOption.Return();

        var setResult = await keyStore.Set(path, new DataETag(content2), context, leaseId: leaseId);
        setResult.BeOk();

        var readOption = await keyStore.Get(path, context);
        readOption.BeOk();
        content2.SequenceEqual(readOption.Return().Data).BeTrue();

        await keyStore.Release(leaseId, context);
        await keyStore.Delete(path, context);
    }

    [Fact]
    public async Task DeleteWithLeaseId_ShouldRemoveLockedKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string path = "test/lease-delete.txt";
        var content = "Delete with lease".ToBytes();

        await keyStore.Set(path, new DataETag(content), context);

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromMinutes(1), context);
        leaseOption.BeOk();
        var leaseId = leaseOption.Return();

        var deleteResult = await keyStore.Delete(path, context, leaseId: leaseId);
        deleteResult.BeOk();

        var existsResult = await keyStore.Exists(path, context);
        existsResult.IsError().BeTrue();
    }

    [Fact]
    public async Task AppendWithLeaseId_ShouldAppendToLockedKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string path = "test/lease-append.txt";
        var content1 = "First".ToBytes();
        var content2 = " Second".ToBytes();

        await keyStore.Set(path, new DataETag(content1), context);

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromMinutes(1), context);
        leaseOption.BeOk();
        var leaseId = leaseOption.Return();

        var appendResult = await keyStore.Append(path, new DataETag(content2), context, leaseId: leaseId);
        appendResult.BeOk();

        var readOption = await keyStore.Get(path, context);
        readOption.BeOk();
        var combined = content1.Concat(content2).ToArray();
        combined.SequenceEqual(readOption.Return().Data).BeTrue();

        await keyStore.Release(leaseId, context);
        await keyStore.Delete(path, context);
    }

    [Fact]
    public async Task GetOperation_ShouldFailForNonExistingKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string path = "test/non-existing-get.txt";

        var readOption = await keyStore.Get(path, context);
        readOption.IsError().BeTrue();
    }

    [Fact]
    public async Task DeleteOperation_ShouldFailForNonExistingKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");
        var context = host.Services.CreateContext<DataSpaceKeyTests>();

        string path = "test/non-existing-delete.txt";

        var deleteResult = await keyStore.Delete(path, context);
        deleteResult.IsError().BeTrue();
    }
}
