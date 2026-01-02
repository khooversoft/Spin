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

public class DatalakeStore_ReadWriteTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public DatalakeStore_ReadWriteTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private async Task<IHost> BuildService([CallerMemberName] string function = "")
    {
        var option = TestApplication.ReadOption(nameof(DatalakeStore_ReadWriteTests) + "/" + function);

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
    public async Task SingleFileReadWriteSearchDeleteNoExtensions()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "testrecord.json";
        var testRecord = new TestRecord("John Doe", 30);

        (await keyStore.Exists(path)).BeNotFound();
        (await keyStore.Get(path)).BeNotFound();
        (await keyStore.Delete(path)).BeNotFound();

        (await keyStore.Add(path, testRecord.ToDataETag())).BeOk();
        (await keyStore.Add(path, testRecord.ToDataETag())).BeConflict();

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
        (await keyStore.Search("**.*")).Count().Be(1);

        (await keyStore.Delete(path)).BeOk();

        (await keyStore.Search("testrecord.json")).Count().Be(0);
        (await keyStore.Search("testrecord.*")).Count().Be(0);
        (await keyStore.Search("*.json")).Count().Be(0);
        (await keyStore.Search("*.*")).Count().Be(0);
        (await keyStore.Search("**.*")).Count().Be(0);
    }

    [Fact]
    public async Task SingleFileReadWriteSearchDeleteExtensions()
    {
        var host = await BuildService();
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

    [Fact]
    public async Task ClearFolder()
    {
        var host = await BuildService();
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
        (await keyStore.Search("**.*")).Count().Be(15);

        (await keyStore.ClearStore()).BeOk();

        (await keyStore.Search("testrecord.json")).Count().Be(0);
        (await keyStore.Search("testrecord.*")).Count().Be(0);
        (await keyStore.Search("*.json")).Count().Be(0);
        (await keyStore.Search("*.*")).Count().Be(0);
        (await keyStore.Search("**.*")).Count().Be(0);
    }

    [Fact]
    public async Task SetFile_UpdateExistingFile_ShouldSucceed()
    {
        var host = await BuildService();
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

    [Fact]
    public async Task GetDetails_ExistingFile_ReturnsPathDetails()
    {
        var host = await BuildService();
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

    [Fact]
    public async Task GetDetails_NonExistingFile_ReturnsNotFound()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "nonexistent.json";
        var detailsOption = await keyStore.GetDetails(path);
        detailsOption.BeNotFound();
    }

    [Fact]
    public async Task DeleteFolder_ShouldRemoveAllFilesInFolder()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string folderPath = "test-folder";
        var testRecord = new TestRecord("Test", 30);

        await keyStore.Add($"{folderPath}/file1.json", testRecord);
        await keyStore.Add($"{folderPath}/file2.json", testRecord);
        await keyStore.Add($"{folderPath}/subfolder/file3.json", testRecord);

        (await keyStore.Search($"{folderPath}/**/*")).Count().Be(3);

        (await keyStore.DeleteFolder(folderPath)).BeOk();

        (await keyStore.Search($"{folderPath}/**/*")).Count().Be(0);
    }

    [Fact]
    public async Task Search_WithComplexPatterns_ShouldReturnMatchingFiles()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var testRecord = new TestRecord("Test", 30);

        await keyStore.Add("root1.json", testRecord);
        await keyStore.Add("root2.txt", testRecord);
        await keyStore.Add("folder1/file1.json", testRecord);
        await keyStore.Add("folder1/file2.json", testRecord);
        await keyStore.Add("folder2/subfolder/file3.json", testRecord);

        (await keyStore.Search("root*.json")).Count().Be(1);
        (await keyStore.Search("folder1/*.json")).Count().Be(2);
        (await keyStore.Search("folder2/**/*.json")).Count().Be(1);
        (await keyStore.Search("**/file*.json")).Count().Be(3);

        await keyStore.ClearStore();
    }

    [Fact]
    public async Task ReleaseLease_NonExistentLease_ShouldSucceed()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "release-test.json";
        var testRecord = new TestRecord("Test", 30);

        (await keyStore.Add(path, testRecord)).BeOk();

        (await keyStore.ReleaseLease(path, "invalid-lease-id")).BeOk();

        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task Set_OnNonExistentFile_ShouldCreateFile()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "set-new-file.json";
        var testRecord = new TestRecord("New File", 25);

        (await keyStore.Exists(path)).BeNotFound();

        (await keyStore.Set(path, testRecord)).BeOk();

        (await keyStore.Exists(path)).BeOk();
        (await keyStore.Get<TestRecord>(path)).BeOk().Return().Be(testRecord);

        (await keyStore.Delete(path)).BeOk();
    }


    [Fact]
    public async Task Search_EmptyStore_ShouldReturnEmptyList()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var results = await keyStore.Search("**.*");
        results.Count.Be(0);

        results = await keyStore.Search("nonexistent/*.json");
        results.Count.Be(0);
    }

    [Fact]
    public async Task Search_NonExistentFolder_ShouldReturnEmptyList()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var testRecord = new TestRecord("Test", 30);
        await keyStore.Add("existing/file.json", testRecord);

        var results = await keyStore.Search("nonexistent/**/*");
        results.Count.Be(0);

        await keyStore.ClearStore();
    }

    [Fact]
    public async Task DeleteFolder_NonExistentFolder_ShouldSucceed()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        (await keyStore.DeleteFolder("non-existent-folder")).BeNotFound();
    }
}
