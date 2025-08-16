using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store;

public class KeyStoreNonSchemaTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly ConcurrentQueue<string> _loggingMessage = new();
    private record JournalEntry(int index, string Name, int Age);
    private record TraceEntry(int index, DateTime date, string Name, int Age);
    private record LedgerEntry(int index, DateTime date, string AccountId, double amount);

    public KeyStoreNonSchemaTests(ITestOutputHelper output) => _outputHelper = output.NotNull();

    protected virtual void AddStore(IServiceCollection services) => services.AddInMemoryFileStore();

    public async Task<IHost> BuildService(bool useCache, bool useHash)
    {
        FileSystemType fileSystemType = useHash ? FileSystemType.Hash : FileSystemType.Key;

        var host = Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            services.AddLogging(config =>
            {
                config.AddLambda(x =>
                {
                    _loggingMessage.Enqueue(x);
                    _outputHelper.WriteLine(x);
                });
                config.AddDebug();
                config.AddFilter(x => true);
            });

            AddStore(services);
            services.AddKeyStore<DataETag>(fileSystemType, config =>
            {
                if (useCache) config.AddCacheProvider(TimeSpan.FromMilliseconds(100));
            });
        })
        .Build();

        await host.ClearStore<KeyStoreTests>();
        return host;
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task SingleKey(bool useCache, bool useHash)
    {
        using var host = await BuildService(useCache, useHash);
        var context = host.Services.CreateContext<KeyStoreNonSchemaTests>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var keyStore = host.Services.GetRequiredService<IKeyStore<DataETag>>();
        var journalFileStore = host.Services.GetRequiredService<IFileSystem<DataETag>>();

        const string key = nameof(SingleKey);
        string shouldMatch = journalFileStore.PathBuilder(key);
        var journalEntry = new JournalEntry(1, "Test", 30);

        DataETag dataETag = journalEntry.ToDataETag();
        (await keyStore.Get(key, context)).IsNotFound().BeTrue();
        (await keyStore.Set(key, dataETag, context)).BeOk();
        (await keyStore.Get(key, context)).BeOk().Return().Action(x => (dataETag == x).BeTrue());

        var fileList = await fileStore.Search("**/*", context);
        fileList.Count.Be(1);
        fileList.First().Path.Be(shouldMatch);

        (await keyStore.Delete(key, context)).BeOk();
        (await keyStore.Get(key, context)).IsNotFound().BeTrue();
        (await fileStore.Search("**/*", context)).Count.Be(0);
    }
}

