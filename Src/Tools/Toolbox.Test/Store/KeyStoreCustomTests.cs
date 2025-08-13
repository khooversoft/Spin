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

public class KeyStoreCustomTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly ConcurrentQueue<string> _loggingMessageSave = new();
    private record JournalEntry(string Name, int Age);

    public KeyStoreCustomTests(ITestOutputHelper output) => _outputHelper = output.NotNull();

    protected virtual void AddStore(IServiceCollection services) => services.AddInMemoryFileStore();

    public async Task<IHost> BuildService()
    {
        TimeSpan cacheDuration = TimeSpan.FromSeconds(1);

        var host = Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            services.AddLogging(config =>
            {
                config.AddLambda(x =>
                {
                    _loggingMessageSave.Enqueue(x);
                    _outputHelper.WriteLine(x);
                });
                config.AddDebug();
                config.AddFilter(x => true);
            });

            AddStore(services);
            services.AddTransient<CustomProvider>();
            services.AddKeyStore<JournalEntry>(FileSystemType.Key, config =>
            {
                config.AddKeyStore();
                config.AddCustomProvider<JournalEntry, CustomProvider>();
            });
        })
        .Build();

        await host.ClearStore<KeyStoreTests>();
        return host;
    }

    [Fact]
    public async Task SingleKey()
    {
        using var host = await BuildService();
        var context = host.Services.CreateContext<KeyStoreCustomTests>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var keyStore = host.Services.GetRequiredService<IKeyStore<JournalEntry>>();
        var journalFileStore = host.Services.GetRequiredService<IFileSystem<JournalEntry>>();

        const string key = nameof(SingleKey);

        var readOption = await keyStore.Get(key, context);
        JournalEntry read = readOption.BeOk().Return();
        JournalEntry journalEntry = new("SingleKeyName", 10);

        (read == journalEntry).BeTrue();

        _loggingMessageSave.Count(x => x.Contains("CreateCustom")).Be(1);
        (await fileStore.Search("**/*", context)).Action(x =>
        {
            x.Count.Be(1);
            x[0].Path.Be(journalFileStore.PathBuilder(key));
        });

        (await keyStore.Get(key, context)).BeOk().Return().Assert(x => x == journalEntry);

        (await fileStore.Search("**/*", context)).Count.Be(1);
        _loggingMessageSave.Count(x => x.Contains("CreateCustom")).Be(1);

        (await keyStore.Delete(key, context)).BeOk();
        (await fileStore.Search("**/*", context)).Count.Be(0);

        (await keyStore.Get(key, context)).BeOk().Return().Assert(x => x == journalEntry);
        (await fileStore.Search("**/*", context)).Count.Be(1);
        _loggingMessageSave.Count(x => x.Contains("CreateCustom")).Be(2);
    }

    [Fact]
    public async Task NoDefaultValueKey()
    {
        using var host = await BuildService();
        var context = host.Services.CreateContext<KeyStoreCustomTests>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var keyStore = host.Services.GetRequiredService<IKeyStore<JournalEntry>>();
        var journalFileStore = host.Services.GetRequiredService<IFileSystem<JournalEntry>>();

        const string key = nameof(NoDefaultValueKey);

        var readOption = await keyStore.Get(key, context);
        readOption.BeNotFound();

        _loggingMessageSave.Count(x => x.Contains("CreateCustom")).Be(0);
    }

    [Fact]
    public async Task TwoKeys()
    {
        using var host = await BuildService();
        var context = host.Services.CreateContext<KeyStoreCustomTests>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var keyStore = host.Services.GetRequiredService<IKeyStore<JournalEntry>>();
        var journalFileStore = host.Services.GetRequiredService<IFileSystem<JournalEntry>>();

        const string key1 = nameof(TwoKeys);
        const string key2 = nameof(TwoKeys) + "2";
        JournalEntry journalEntry = new(key1, 10);
        JournalEntry journalEntry2 = new(key2, 10);

        (await keyStore.Get(key1, context)).BeOk().Return().Assert(x => x == journalEntry);

        _loggingMessageSave.Count(x => x.Contains("CreateCustom")).Be(1);
        (await fileStore.Search("**/*", context)).Action(x =>
        {
            x.Count.Be(1);
            x[0].Path.Be(journalFileStore.PathBuilder(key1));
        });

        (await keyStore.Get(key2, context)).BeOk().Return().Assert(x => x == journalEntry2);

        _loggingMessageSave.Count(x => x.Contains("CreateCustom:TwoKeys2")).Be(1);
        (await fileStore.Search("**/*", context)).Action(x =>
        {
            x.Count.Be(2);
            x.Any(y => y.Path == journalFileStore.PathBuilder(key2)).BeTrue();
        });

        (await keyStore.Delete(key1, context)).BeOk();
        (await keyStore.Delete(key2, context)).BeOk();
    }

    private class CustomProvider : IKeyStore<JournalEntry>
    {
        private readonly ILogger<CustomProvider> _logger;

        public CustomProvider(ILogger<CustomProvider> logger)
        {
            _logger = logger.NotNull();
        }

        public IKeyStore<JournalEntry>? InnerHandler { get; set; }

        public Task<Option> Append(string key, JournalEntry value, ScopeContext context) => new Option(StatusCode.OK).ToTaskResult();
        public Task<Option> Delete(string key, ScopeContext context) => new Option(StatusCode.OK).ToTaskResult();

        public Task<Option<JournalEntry>> Get(string key, ScopeContext context)
        {
            Option<JournalEntry> result = key switch
            {
                "SingleKey" => new JournalEntry("SingleKeyName", 10).Action(_ => context.LogDebug("CreateCustom")),
                "TwoKeys" => new JournalEntry("TwoKeys", 10).Action(_ => context.LogDebug("CreateCustom:TwoKeys")),
                "TwoKeys2" => new JournalEntry("TwoKeys2", 10).Action(_ => context.LogDebug("CreateCustom:TwoKeys2")),
                _ => StatusCode.NotFound.Action(_ => context.LogDebug("NotFound")),
            };

            return result.ToTaskResult();
        }

        public Task<Option<string>> Set(string key, JournalEntry value, ScopeContext context) => new Option<string>(StatusCode.OK).ToTaskResult();

        public Task<Option> AcquireExclusiveLock(string key, ScopeContext context) => new Option(StatusCode.OK).ToTaskResult();
        public Task<Option> AcquireLock(string key, ScopeContext context) => new Option(StatusCode.OK).ToTaskResult();
        public Task<Option> ReleaseLock(string key, ScopeContext context) => new Option(StatusCode.OK).ToTaskResult();
    }
}
