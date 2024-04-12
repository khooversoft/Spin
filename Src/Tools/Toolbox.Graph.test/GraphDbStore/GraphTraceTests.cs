//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using FluentAssertions;
//using Toolbox.Extensions;
//using Toolbox.Store;
//using Toolbox.Types;

//namespace Toolbox.Graph.test.GraphDbStore;

//public class GraphTraceTests
//{
//    [Fact]
//    public async Task SingleAddNode()
//    {
//        IFileStore store = new InMemoryFileStore();
//        var trace = new InMemoryChangeTrace();
//        GraphDb db = new GraphDb(store, trace);

//        var result = await db.Graph.ExecuteScalar("add node key=node1;", NullScopeContext.Instance);
//        result.IsOk().Should().BeTrue();
//        result.Return().Items.Count.Should().Be(0);

//        var lookup = await db.Graph.ExecuteScalar("select (*);", NullScopeContext.Instance);
//        lookup.IsOk().Should().BeTrue();
//        lookup.Return().Items.Count.Should().Be(1);

//        var traces = trace.GetTraces();
//        traces.Count.Should().Be(1);
//    }

//    [Fact]
//    public void UpdateNode()
//    {
//    }

//    [Fact]
//    public void ChangeNode()
//    {
//    }

//    [Fact]
//    public void SingleEdge()
//    {
//    }

//    [Fact]
//    public void UpdateEdge()
//    {
//    }

//    [Fact]
//    public void ChangeEdge()
//    {
//    }
//}
