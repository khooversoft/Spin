using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.LangTools;
using Xunit.Abstractions;

namespace Toolbox.Test.Lang.Graph;

public class GraphBatchTests
{
    private readonly ITestOutputHelper _output;
    private readonly ILangRoot _root;

    public GraphBatchTests(ITestOutputHelper output)
    {
        _output = output;
        _root = GraphLangGrammer.Root;
    }

    [Fact]
    public void SingleNode()
    {
        var test = new QueryTest
        {
            RawData = """
            add node key=key1,tags=t1;
            update (key=key1) set key=key2,tags=t2;
            delete [key='string value'] a1;
            select [key='string value'] a1;
            """,
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

                new QueryResult<LsSymbol>("update"),
                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsGroup>(")","pgroup"),
                new QueryResult<LsSymbol>("set"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key2", "rvalue"),
                new QueryResult<LsToken>(",", "value-delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t2", "rvalue"),
                new QueryResult<LsToken>(";", "term"),

                new QueryResult<LsSymbol>("delete"),
                new QueryResult<LsGroup>("[","bgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>("]","bgroup"),
                new QueryResult<LsValue>("a1","alias"),
                new QueryResult<LsToken>(";", "term"),

                new QueryResult<LsSymbol>("select"),
                new QueryResult<LsGroup>("[","bgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>("]","bgroup"),
                new QueryResult<LsValue>("a1","alias"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }
}
