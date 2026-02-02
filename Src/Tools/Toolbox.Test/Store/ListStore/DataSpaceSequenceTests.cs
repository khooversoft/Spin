using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.ListStore;

public class DataSpaceSequenceTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public DataSpaceSequenceTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;
    protected virtual void AddStore(IServiceCollection services, string basePath) => services.AddInMemoryKeyStore();


    private async Task<IHost> BuildService([CallerMemberName] string function = "")
    {
        string basePath = nameof(DataSpaceListTests) + "/" + function;

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                AddStore(services, basePath);

                services.AddDataSpace(cnfg =>
                {
                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "sequence",
                        ProviderName = "sequenceStore",
                        BasePath = "sequenceBase",
                        SpaceFormat = SpaceFormat.Sequence,
                    });
                    cnfg.Add<SequenceSpaceProvider>("sequenceStore");
                });

                services.AddSequenceStore<TestRecord>("sequence");

            })
            .Build();

        // Clear the store before running tests, this includes any locked files
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();
        await keyStore.DeleteFolder(basePath);
        (await keyStore.Search($"{basePath}/***")).Count().Be(0); ;

        return host;
    }

    private static IEnumerable<TestRecord> CreateTestEntries(int count) =>
        Enumerable.Range(1, count).Select(i => new TestRecord($"Person{i}", 20 + i));

    [Fact]
    public async Task SingleItemInList()
    {
        //JsonSerializerContextRegistered.ScanAndRegister();
        using var host = await BuildService();
        ISequenceStore<TestRecord> sequenceStore = host.Services.GetRequiredService<ISequenceStore<TestRecord>>();

        var ls = sequenceStore as SequenceSpace<TestRecord> ?? throw new ArgumentException();
        var fileStore = ls.SequenceKeySystem;

        const string key = nameof(SingleItemInList);

        string pathPrefix = fileStore.GetPathPrefix();
        string fullPath = fileStore.PathBuilder(key);
        string shouldMatch = fullPath.Replace($"{pathPrefix}/", string.Empty);

        var testRecord = new TestRecord("Test", 30);
        (await sequenceStore.Add(key, testRecord)).BeOk();

        (await sequenceStore.Get(key)).BeOk().Return().Action(x =>
        {
            x.Count.Be(1);
            x.SequenceEqual([testRecord]).BeTrue();
        });

        (await sequenceStore.Delete(key)).BeOk();
        (await sequenceStore.Get(key)).Return().Count.Be(0);
    }
}
