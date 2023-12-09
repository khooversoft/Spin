//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using FluentAssertions;
//using Toolbox.Data;
//using Toolbox.Extensions;

//namespace Toolbox.Test.Data.Lang;

//public class GraphCmdTests
//{
//    [Fact]
//    public void AddNode()
//    {
//        var a1 = new GraphCmd().AddNode("key1").Build().Action(x => x.Should().Be("add node key=key1;"));
//        var a2 = new GraphCmd().AddNode("key1", "t2").Build().Action(x => x.Should().Be("add node key=key1,tags=t2;"));
//        var a3 = new GraphCmd().AddNode("key1", "t3=t3v").Build().Action(x => x.Should().Be("add node key=key1,tags='t3=t3v';"));

//        string cmd = new GraphCmds()
//            .Add(a1)
//            .Add(a2)
//            .Add(a3)
//            .Build();

//        var results = new[]
//        {
//            "add node key=key1;",
//            "add node key=key1,tags=t2;",
//            "add node key=key1,tags='t3=t3v';",
//        };

//        cmd.Should().Be(results.Join());
//    }

//    [Fact]
//    public void AddEdge()
//    {
//        var a1 = new GraphCmd().AddEdge("fromKey1", "toKey2").Build().Action(x => x.Should().Be("add edge fromKey=fromKey1,toKey=toKey2;"));

//        var a2 = new GraphCmd().AddEdge("fromKey1", "toKey2", "edgeType3").Build().Action(x =>
//        {
//            x.Should().Be("add edge fromKey=fromKey1,toKey=toKey2,edgeType=edgeType3;");
//        });

//        var a3 = new GraphCmd().AddEdge("fromKey1", "toKey2", "edgeType3", "t3").Build().Action(x =>
//        {
//            x.Should().Be("add edge fromKey=fromKey1,toKey=toKey2,edgeType=edgeType3,tags=t3;");
//        });

//        var a4 = new GraphCmd().AddEdge("fromKey1", "toKey2", "edgeType3", "t3=tag1").Build().Action(x =>
//        {
//            x.Should().Be("add edge fromKey=fromKey1,toKey=toKey2,edgeType=edgeType3,tags='t3=tag1';");
//        });

//        string cmd = new GraphCmds()
//            .Add(a1)
//            .Add(a2)
//            .Add(a3)
//            .Add(a4)
//            .Build();

//        var results = new[]
//        {
//            "add edge fromKey=fromKey1,toKey=toKey2;",
//            "add edge fromKey=fromKey1,toKey=toKey2,edgeType=edgeType3;",
//            "add edge fromKey=fromKey1,toKey=toKey2,edgeType=edgeType3,tags=t3;",
//            "add edge fromKey=fromKey1,toKey=toKey2,edgeType=edgeType3,tags='t3=tag1';",
//        };

//        cmd.Should().Be(results.Join());
//    }

//    [Fact]
//    public void Search()
//    {
//        new GraphCmdSearch().Node("key1").Build().Action(x => x.Should().Be("(key=key1)"));
//        new GraphCmdSearch().Node(tags: "t1").Build().Action(x => x.Should().Be("(tags=t1)"));
//        new GraphCmdSearch().Node(tags: "t1=tv2").Build().Action(x => x.Should().Be("(tags='t1=tv2')"));
//        new GraphCmdSearch().Node("key1", "t1").Build().Action(x => x.Should().Be("(key=key1;tags=t1)"));
//        new GraphCmdSearch().Node("key1", "t1=tv2").Build().Action(x => x.Should().Be("(key=key1;tags='t1=tv2')"));

//        new GraphCmdSearch().Edge("key1").Build().Action(x => x.Should().Be("[fromKey=key1]"));
//        new GraphCmdSearch().Edge(toKey: "key3").Build().Action(x => x.Should().Be("[toKey=key3]"));
//        new GraphCmdSearch().Edge(edgeType: "edt4").Build().Action(x => x.Should().Be("[edgeType=edt4]"));
//        new GraphCmdSearch().Edge(tags: "t4").Build().Action(x => x.Should().Be("[tags=t4]"));
//        new GraphCmdSearch().Edge(tags: "t4=vt4").Build().Action(x => x.Should().Be("[tags='t4=vt4']"));

//        new GraphCmdSearch().Edge("key1", "key2").Build().Action(x => x.Should().Be("[fromKey=key1;toKey=key2]"));
//        new GraphCmdSearch().Edge("key1", "key2", "edType").Build().Action(x => x.Should().Be("[fromKey=key1;toKey=key2;edgeType=edType]"));
//        new GraphCmdSearch().Edge("key1", "key2", "edType", "t1").Build().Action(x => x.Should().Be("[fromKey=key1;toKey=key2;edgeType=edType;tags=t1]"));
//        new GraphCmdSearch().Edge("key1", "key2", "edType", "t1=t1v1").Build().Action(x => x.Should().Be("[fromKey=key1;toKey=key2;edgeType=edType;tags='t1=t1v1']"));

//        new GraphCmdSearch().Node("key1").Edge(toKey: "toKey2").Build().Action(x => x.Should().Be("(key=key1)->[toKey=toKey2]"));

//        new GraphCmdSearch().Edge(toKey: "toKey2").Node("key1").Edge(fromKey: "fromKey2")
//            .Build().Action(x => x.Should().Be("[toKey=toKey2]->(key=key1)->[fromKey=fromKey2]"));
//    }

//    [Fact]
//    public void Delete()
//    {
//        new GraphCmd().Delete(new GraphCmdSearch().Node("key1")).Build().Action(x => x.Should().Be("delete (key=key1);"));
//        new GraphCmd().Delete(new GraphCmdSearch().Node("key1","t1")).Build().Action(x => x.Should().Be("delete (key=key1;tags=t1);"));

//        new GraphCmd().Delete(new GraphCmdSearch().Edge("key1")).Build().Action(x => x.Should().Be("delete [fromKey=key1];"));
//        new GraphCmd().Delete(new GraphCmdSearch().Edge(edgeType: "edgType1")).Build().Action(x => x.Should().Be("delete [edgeType=edgType1];"));
//    }

//    [Fact]
//    public void Update()
//    {
//        new GraphCmd().UpdateNode(new GraphCmdSearch().Node("key1"), "t4").Build().Action(x => x.Should().Be("update (key=key1) set tags=t4;"));
//        new GraphCmd().UpdateNode(new GraphCmdSearch().Edge("key1"), "t4").Build().Action(x => x.Should().Be("update [fromKey=key1] set tags=t4;"));

//        new GraphCmd().UpdateEdge(new GraphCmdSearch().Node("key1"), "edgeTypeT4").Build().Action(x => x.Should().Be("update (key=key1) set edgeType=edgeTypeT4;"));
//        new GraphCmd().UpdateEdge(new GraphCmdSearch().Edge("key1"), "edgeTypeT4").Build().Action(x => x.Should().Be("update [fromKey=key1] set edgeType=edgeTypeT4;"));
//        new GraphCmd().UpdateEdge(new GraphCmdSearch().Edge("key1"), "edgeTypeT4", "t5").Build().Action(x => x.Should().Be("update [fromKey=key1] set edgeType=edgeTypeT4,tags=t5;"));
//    }
//}
