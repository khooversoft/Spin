using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.KeyStore;

public class KeyStore_ReadWriteTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);
    private record LargeRecord(string Name, int Age, string data);

    public KeyStore_ReadWriteTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    protected virtual void AddStore(IServiceCollection services, string basePath) => services.AddInMemoryKeyStore();

    private async Task<IHost> BuildService(bool useHash, bool useCache, [CallerMemberName] string function = "")
    {
        string basePath = nameof(KeyStore_ReadWriteTests) + "/" + function;

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                AddStore(services, basePath);
                services.AddMemoryCache();

                services.AddDataSpace(cnfg =>
                {
                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "file",
                        ProviderName = "fileStore",
                        BasePath = basePath,
                        SpaceFormat = useHash ? SpaceFormat.Hash : SpaceFormat.Key,
                        UseCache = useCache
                    });

                    cnfg.Add<KeyStoreProvider>("fileStore");
                });
            })
            .Build();

        // Clear the store before running tests, this includes any locked files
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();
        await keyStore.DeleteFolder(basePath);
        (await keyStore.Search($"{basePath}/***")).Count().Be(0);
        return host;
    }


    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task SingleFileReadWriteSearchDeleteNoExtensions(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "testrecord.json";
        var testRecord = new TestRecord("John Doe", 30);

        (await keyStore.Exists(path)).BeNotFound();
        (await keyStore.Get(path)).BeNotFound();
        (await keyStore.Delete(path)).BeNotFound();
        (await keyStore.Search("**")).Count().Be(0);

        (await keyStore.Add(path, testRecord.ToDataETag())).BeOk();
        (await keyStore.Add(path, testRecord.ToDataETag())).BeConflict();
        (await keyStore.Search("**")).Count().Be(1);

        (await keyStore.Exists(path)).BeOk();
        (await keyStore.Get(path)).BeOk().Return().Action(x =>
        {
            var o = x.ToObject<TestRecord>();
            o.Be(testRecord);
        });

        (await keyStore.Search("testrecord.json")).Count().Be(1);
        (await keyStore.Search("testrecord.*")).Count().Be(1);
        (await keyStore.Search("*.json")).Count().Be(1);
        (await keyStore.Search("*.*")).Count().Be(1);
        (await keyStore.Search("***")).Count().Be(1);

        (await keyStore.Delete(path)).BeOk();

        (await keyStore.Search("testrecord.json")).Count().Be(0);
        (await keyStore.Search("testrecord.*")).Count().Be(0);
        (await keyStore.Search("*.json")).Count().Be(0);
        (await keyStore.Search("*.*")).Count().Be(0);
        (await keyStore.Search("***")).Count().Be(0);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task SingleFileReadWriteSearchDeleteExtensions(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "testrecord.json";
        var testRecord = new TestRecord("John Doe", 30);

        (await keyStore.Exists(path)).BeNotFound();
        (await keyStore.Get<TestRecord>(path)).BeNotFound();
        (await keyStore.Delete(path)).BeNotFound();

        (await keyStore.Add(path, testRecord)).BeOk();
        (await keyStore.Add(path, testRecord)).BeConflict();

        (await keyStore.Exists(path)).BeOk();
        (await keyStore.Get<TestRecord>(path)).BeOk().Return().Be(testRecord);

        (await keyStore.Search("testrecord.json")).Count().Be(1);
        (await keyStore.Search("testrecord.*")).Count().Be(1);
        (await keyStore.Search("*.json")).Count().Be(1);
        (await keyStore.Search("*.*")).Count().Be(1);
        (await keyStore.Search("**.*")).Count().Be(1);

        (await keyStore.Delete(path)).BeOk();

        (await keyStore.Search("testrecord.json")).Count().Be(0);
        (await keyStore.Search("testrecord.*")).Count().Be(0);
        (await keyStore.Search("*.json")).Count().Be(0);
        (await keyStore.Search("*.*")).Count().Be(0);
        (await keyStore.Search("**.*")).Count().Be(0);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task ClearFolder(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "testrecord.json";
        int count = 15;

        var list = new Sequence<string>();

        for (int i = 0; i < count; i++)
        {
            string fullPath = i switch
            {
                < 5 => $"{i}-{path}",
                < 10 => $"folder1/{i}-{path}",
                _ => $"folder2/{i}-{path}"
            };
            list += fullPath;

            var testRecord = new TestRecord($"John Doe {i}", 30 + i);
            await keyStore.Add(fullPath, testRecord);
        }

        foreach (var p in list)
        {
            (await keyStore.Exists(p)).BeOk();
        }

        (await keyStore.Search("testrecord.json")).Count().Be(0);
        (await keyStore.Search("testrecord.*")).Count().Be(0);
        (await keyStore.Search("*.json")).Count().Be(5);
        (await keyStore.Search("*.*")).Count().Be(5);
        (await keyStore.Search("**")).Count().Be(15);

        (await keyStore.DeleteFolder("**")).BeOk();
        (await keyStore.Search("**")).Count().Be(0);

        (await keyStore.Search("testrecord.json")).Count().Be(0);
        (await keyStore.Search("testrecord.*")).Count().Be(0);
        (await keyStore.Search("*.json")).Count().Be(0);
        (await keyStore.Search("*.*")).Count().Be(0);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task SetFile_UpdateExistingFile_ShouldSucceed(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "testrecord.json";
        var testRecord1 = new TestRecord("John Doe", 30);
        var testRecord2 = new TestRecord("Jane Smith", 35);

        (await keyStore.Add(path, testRecord1)).BeOk();
        (await keyStore.Get<TestRecord>(path)).BeOk().Return().Be(testRecord1);

        (await keyStore.Set(path, testRecord2)).BeOk();
        (await keyStore.Get<TestRecord>(path)).BeOk().Return().Be(testRecord2);

        (await keyStore.Delete(path)).BeOk();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task GetDetails_ExistingFile_ReturnsPathDetails(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "details-test.json";
        var testRecord = new TestRecord("Test User", 25);

        (await keyStore.Add(path, testRecord)).BeOk();
        (await keyStore.Exists(path)).BeOk();

        var detailsOption = await keyStore.GetDetails(path);
        detailsOption.BeOk();
        var details = detailsOption.Return();

        details.Path.NotEmpty();
        details.IsFolder.BeFalse();
        details.ContentLength.Assert(x => x > 0);
        details.ETag.NotEmpty();

        (await keyStore.Delete(path)).BeOk();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task GetDetails_NonExistingFile_ReturnsNotFound(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "nonexistent.json";
        var detailsOption = await keyStore.GetDetails(path);
        detailsOption.BeNotFound();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task DeleteFolder_ShouldRemoveAllFilesInFolder(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string folderPath = "test-folder";
        var testRecord = new TestRecord("Test", 30);

        await keyStore.Add($"{folderPath}/file1.json", testRecord);
        await keyStore.Add($"{folderPath}/file2.json", testRecord);
        await keyStore.Add($"{folderPath}/subfolder/file3.json", testRecord);

        (await keyStore.Search($"{folderPath}/**")).Count().Be(3);

        (await keyStore.DeleteFolder(folderPath)).BeOk();

        (await keyStore.Search($"{folderPath}/**")).Count().Be(0);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Search_WithComplexPatterns_ShouldReturnMatchingFiles(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var testRecord = new TestRecord("Test", 30);

        (await keyStore.Add("root1.json", testRecord)).BeOk();
        (await keyStore.Add("root2.txt", testRecord)).BeOk();
        (await keyStore.Add("folder1/file1.json", testRecord)).BeOk();
        (await keyStore.Add("folder1/file2.json", testRecord)).BeOk();
        (await keyStore.Add("folder2/subfolder/file3.json", testRecord)).BeOk();
        (await keyStore.Add("hashfiles/a0/b3/hashfile4.json", testRecord)).BeOk();
        (await keyStore.Add("hashfiles/a0/b3/hashfile5.json", testRecord)).BeOk();
        (await keyStore.Add("hashfiles/a5/b2/hashfile6.json", testRecord)).BeOk();

        (await keyStore.Search("**")).Count().Be(8);
        (await keyStore.Search("root*.json")).Count().Be(1);
        (await keyStore.Search("folder1/*.json")).Count().Be(2);
        (await keyStore.Search("folder2/**/*.json")).Count().Be(1);
        (await keyStore.Search("**/file*.json")).Count().Be(3);
        (await keyStore.Search("hashfiles/*/*/hashfile*.json")).Count().Be(3);

        await keyStore.DeleteFolder("**");
        (await keyStore.Search("**")).Count().Be(0);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task ReleaseLease_NonExistentLease_ShouldSucceed(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "release-test.json";
        var testRecord = new TestRecord("Test", 30);

        (await keyStore.Add(path, testRecord)).BeOk();

        (await keyStore.ReleaseLease(path, Guid.NewGuid().ToString())).BeNotFound();

        (await keyStore.Delete(path)).BeOk();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Set_OnNonExistentFile_ShouldCreateFile(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "set-new-file.json";
        var testRecord = new TestRecord("New File", 25);

        (await keyStore.Exists(path)).BeNotFound();

        (await keyStore.Set(path, testRecord)).BeOk();

        (await keyStore.Exists(path)).BeOk();
        (await keyStore.Get<TestRecord>(path)).BeOk().Return().Be(testRecord);

        (await keyStore.Delete(path)).BeOk();
    }


    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Search_EmptyStore_ShouldReturnEmptyList(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var results = await keyStore.Search("**.*");
        results.Count.Be(0);

        results = await keyStore.Search("nonexistent/*.json");
        results.Count.Be(0);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Search_NonExistentFolder_ShouldReturnEmptyList(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var testRecord = new TestRecord("Test", 30);
        await keyStore.Add("existing/file.json", testRecord);

        var results = await keyStore.Search("nonexistent/**/*");
        results.Count.Be(0);

        await keyStore.DeleteFolder("**");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task DeleteFolder_NonExistentFolder_ShouldBeNotFound(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        (await keyStore.DeleteFolder("non-existent-folder")).BeNotFound();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task AddLargeData(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var testRecords = CreateTestEntries(0, 5000);
        (testRecords.ToJson().Length > 10_000_000).BeTrue(); // Ensure we are testing with a large payload

        const string key = "large-data/records.json";
        (await keyStore.Add(key, testRecords)).BeOk();

        var data = (await keyStore.Get(key)).BeOk().Return();
        var retrievedRecords = data.ToObject<List<LargeRecord>>();

        (await keyStore.Delete(key)).BeOk();
    }

    private static IReadOnlyList<LargeRecord> CreateTestEntries(int start, int count) => Enumerable.Range(0, count)
        .Select(x => (i: x, index: start + x))
        .Select(i => new LargeRecord($"Person{i.index}", i.index, GenerateRandomData()))
        .ToArray();

    private static string GenerateRandomData()
    {
        int size = RandomNumberGenerator.GetInt32(5000, 10000);
        int newSize = (size & 1) == 0 ? size : size - 1; // ensure even
        string data = RandomTool.GenerateRandomSequence(newSize);
        int dataLength = $"{data.Length}".Length + 1 + data.Length;
        return $"{dataLength}:{data}";
    }

}
