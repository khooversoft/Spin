using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.LangTools;
using Xunit.Abstractions;

namespace Toolbox.Test.Lang.Graph;

public class GraphAddTests
{
    private readonly ITestOutputHelper _output;
    private readonly ILangRoot _root;

    public GraphAddTests(ITestOutputHelper output)
    {
        _output = output;
        _root = GraphLangGrammer.Root;
    }

    [Fact]
    public void SingleNode()
    {
        var test = new QueryTest
        {
            RawData = "add node key=key1,tags=t1;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("add"),
                new QueryResult<LsSymbol>("node"),

                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(",", "value-delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t1", "rvalue"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void MissingOperatorTerm()
    {
        var test = new QueryTest
        {
            RawData = "node key=key1,tags=t1;",
            Results = new List<IQueryResult>(),
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void MissingTerm()
    {
        var test = new QueryTest
        {
            RawData = "add node key=key1,tags=t1",
            Results = new List<IQueryResult>(),
        };

        LangTestTools.Verify(_output, _root, test);
    }


    [Fact]
    public void MissingNodeTypeTerm()
    {
        var test = new QueryTest
        {
            RawData = "add key=key1,tags=t1;",
            Results = new List<IQueryResult>(),
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void SingleEdge()
    {
        var test = new QueryTest
        {
            RawData = "add edge fromKey=key1,toKey=key2,edgeType=et,tags=t2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("add"),
                new QueryResult<LsSymbol>("edge"),

                new QueryResult<LsValue>("fromKey","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(",", "value-delimiter"),

                new QueryResult<LsValue>("toKey","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key2", "rvalue"),
                new QueryResult<LsToken>(",", "value-delimiter"),

                new QueryResult<LsValue>("edgeType","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("et", "rvalue"),
                new QueryResult<LsToken>(",", "value-delimiter"),

                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t2", "rvalue"),

                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }
}
