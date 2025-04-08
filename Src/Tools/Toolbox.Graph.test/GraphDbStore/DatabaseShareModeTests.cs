using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.GraphDbStore;

public class DatabaseShareModeTests
{
    private readonly ITestOutputHelper _outputHelper;

    public DatabaseShareModeTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task OneWriteOtherRead()
    {
        await using var firstEngineClient = await GraphTestStartup.CreateGraphService(logOutput: x => _outputHelper.WriteLine(x), sharedMode: true);
        var context = firstEngineClient.CreateScopeContext<DatabaseShareModeTests>();
        var fileStore = firstEngineClient.Services.GetRequiredService<IFileStore>();
        var graphFileStore = firstEngineClient.Services.GetRequiredService<IGraphStore>();

        await using var SecondEngineClient = await GraphTestStartup.CreateGraphService(
            logOutput: x => _outputHelper.WriteLine(x),
            config: x => x.AddSingleton<IFileStore>(fileStore).AddSingleton<IGraphStore>(graphFileStore),
            sharedMode: true,
            useInMemoryStore: false
            );

        var e1 = await firstEngineClient.Execute("add node key=node1 set t1=v1, t2=v ;", context);
        e1.IsOk().Should().BeTrue();

        var q1 = await firstEngineClient.Execute("select (key=node1);", context);
        q1.Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Nodes.Count.Should().Be(1);
                var node = y.Nodes[0].Key.Should().Be("node1");
            });
        });

        var s1 = await SecondEngineClient.Execute("select (key=node1);", context);
        s1.Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Nodes.Count.Should().Be(1);
                var node = y.Nodes[0].Key.Should().Be("node1");
            });
        });
    }
}
