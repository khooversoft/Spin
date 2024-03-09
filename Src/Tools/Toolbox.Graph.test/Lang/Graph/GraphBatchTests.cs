using Toolbox.Data;
using Toolbox.LangTools;

namespace Toolbox.Graph.test.Lang.Graph;

public class GraphBatchTests
{
    private readonly ILangRoot _root = GraphLangGrammar.Root;

    [Fact]
    public void AllCommandsBatch()
    {
        var test = new QueryTest
        {
            RawData = """
            add node key=key1,tags=t1;
            add edge fromKey=key1,toKey=key2,edgeType=et,tags=t2;
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
                new QueryResult<LsToken>(",", "delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t1", "rvalue"),
                new QueryResult<LsToken>(";", "term"),

                new QueryResult<LsSymbol>("add"),
                new QueryResult<LsSymbol>("edge"),
                new QueryResult<LsValue>("fromKey","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),
                new QueryResult<LsValue>("toKey","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key2", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),
                new QueryResult<LsValue>("edgeType","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("et", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t2", "rvalue"),
                new QueryResult<LsToken>(";", "term"),

                new QueryResult<LsSymbol>("update"),
                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsGroup>(")","node-group"),
                new QueryResult<LsSymbol>("set"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key2", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t2", "rvalue"),
                new QueryResult<LsToken>(";", "term"),

                new QueryResult<LsSymbol>("delete"),
                new QueryResult<LsGroup>("[","edge-group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>("]","edge-group"),
                new QueryResult<LsValue>("a1","alias"),
                new QueryResult<LsToken>(";", "term"),

                new QueryResult<LsSymbol>("select"),
                new QueryResult<LsGroup>("[","edge-group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>("]","edge-group"),
                new QueryResult<LsValue>("a1","alias"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }
}
