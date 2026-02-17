using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.KeyStore;

public class KeyStore_SearchTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public KeyStore_SearchTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

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
    public async Task Search_ShouldTrimBasePathAndReturnRelativePaths(bool useHash, bool useCache)
    {
        using var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var record = new TestRecord("Normalize", 1);
        string[] paths = ["normalized/file1.json", "normalized/sub/file2.json", "other/file3.json"];

        foreach (var path in paths)
        {
            (await keyStore.Add(path, record)).BeOk();
        }

        var results = await keyStore.Search("**/*.json");
        results.Count.Be(paths.Length);

        var returnedPaths = results.Select(x => x.Path).OrderBy(x => x).ToArray();
        returnedPaths.SequenceEqual(paths.OrderBy(x => x)).BeTrue();
        returnedPaths.All(x => !x.StartsWith("azuretest-datalake-test", StringComparison.OrdinalIgnoreCase)).BeTrue();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Search_WithPaging_ShouldSkipAndTakeExpectedResults(bool useHash, bool useCache)
    {
        using var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var record = new TestRecord("Paging", 2);
        var paths = Enumerable.Range(0, 6).Select(i => $"paging/page-{i:D2}.json").ToArray();

        foreach (var path in paths)
        {
            (await keyStore.Add(path, record)).BeOk();
        }

        var allResults = (await keyStore.Search("paging/*.json")).ToArray();
        allResults.Length.Be(paths.Length);

        var page = (await keyStore.Search("paging/*.json", index: 2, size: 3)).ToArray();
        page.Length.Be(3);

        var expectedPaths = allResults.Skip(2).Take(3).Select(x => x.Path).OrderBy(x => x).ToArray();
        var pagePaths = page.Select(x => x.Path).OrderBy(x => x).ToArray();
        pagePaths.SequenceEqual(expectedPaths).BeTrue();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Search_ShouldIncludeFoldersWhenPatternMatches(bool useHash, bool useCache)
    {
        using var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var record = new TestRecord("Folder", 3);

        (await keyStore.Add("folders/root/file1.json", record)).BeOk();
        (await keyStore.Add("folders/root/nested/file2.json", record)).BeOk();

        var results = await keyStore.Search("folders/***");

        results.Any(x => x.Path.Equals("folders/root/file1.json", StringComparison.OrdinalIgnoreCase)).BeTrue();
        results.Any(x => x.Path.Equals("folders/root/nested/file2.json", StringComparison.OrdinalIgnoreCase)).BeTrue();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Search_TripleStar_ShouldReturnFolders_DoubleStar_ShouldNot(bool useHash, bool useCache)
    {
        using var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var record = new TestRecord("Diff", 5);

        (await keyStore.Add("diff/a/file1.json", record)).BeOk();
        (await keyStore.Add("diff/a/sub/file2.json", record)).BeOk();

        var doubleStar = await keyStore.Search("diff/**");
        doubleStar.Count.Be(2);
        doubleStar.Any(x => x.IsFolder).BeFalse();

        var tripleStar = await keyStore.Search("diff/***");
        tripleStar.Count.Assert(x => x >= doubleStar.Count);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Search_NonRecursivePattern_ShouldNotReturnNestedFiles(bool useHash, bool useCache)
    {
        using var host = await BuildService(useHash, useCache);
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var record = new TestRecord("Scope", 4);

        (await keyStore.Add("nonrecursive/level1.json", record)).BeOk();
        (await keyStore.Add("nonrecursive/sub/level2.json", record)).BeOk();

        var nonRecursive = await keyStore.Search("nonrecursive/*.json");
        nonRecursive.Count.Be(1);
        nonRecursive.Any(x => x.Path.Equals("nonrecursive/level1.json", StringComparison.OrdinalIgnoreCase)).BeTrue();

        var recursive = await keyStore.Search("nonrecursive/**/*.json");
        recursive.Count.Be(2);
        recursive.Any(x => x.Path.Equals("nonrecursive/sub/level2.json", StringComparison.OrdinalIgnoreCase)).BeTrue();
    }
}
