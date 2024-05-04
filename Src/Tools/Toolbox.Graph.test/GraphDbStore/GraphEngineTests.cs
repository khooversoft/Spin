using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Types;

namespace Toolbox.Graph.test.GraphDbStore;

public class GraphEngineTests
{
    //private IGraphClient CreateEngine()
    //{
    //    var services = new ServiceCollection()
    //        .AddLogging()
    //        .AddInMemoryFileStore()
    //        .AddGraphTrace()
    //        .AddGraphFileStore()
    //        .AddGraphEngine()
    //        .BuildServiceProvider();

    //    var graphClient = services.GetRequiredService<IGraphClient>();
    //    return graphClient;
    //}

    [Fact]
    public async Task AddNode()
    {
        IGraphClient engine = GraphTestStartup.CreateGraphTestHost();

        var addResult = await engine.ExecuteScalar("add node key=node1;", NullScopeContext.Instance);
        addResult.IsOk().Should().BeTrue();

        var selectResult = await engine.ExecuteScalar("select (key=node1);", NullScopeContext.Instance);
        addResult.IsOk().Should().BeTrue();

        var deleteResult = await engine.ExecuteScalar("delete (key=node1);", NullScopeContext.Instance);
        deleteResult.IsOk().Should().BeTrue();
    }
}
