using Toolbox.LangTools;

namespace Toolbox.Graph.test.Lang.Syntax;

public class UpsertEdgeTests
{
    private readonly ILangRoot _root = GraphLangGrammar.Root;

    [Fact]
    public void SingleEdge()
    {
        var test = new QueryTest
        {
            RawData = "upsert edge fromKey=key1,toKey=key2,edgeType=et,tags=t2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("upsert"),
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
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void SingleUniqueEdge()
    {
        var test = new QueryTest
        {
            RawData = "upsert unique edge fromKey=key1,toKey=key2,edgeType=et,tags=t2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("upsert"),
                new QueryResult<LsSymbol>("unique"),
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
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void SingleTagValue()
    {
        var test = new QueryTest
        {
            RawData = "upsert edge fromKey=key1,toKey=key2, t1;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("upsert"),
                new QueryResult<LsSymbol>("edge"),

                new QueryResult<LsValue>("fromKey","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),

                new QueryResult<LsValue>("toKey","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key2", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),

                new QueryResult<LsValue>("t1","svalue"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void TwoSingleTagValue()
    {
        var test = new QueryTest
        {
            RawData = "upsert edge fromKey=key1,toKey=key2, t1, t2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("upsert"),
                new QueryResult<LsSymbol>("edge"),

                new QueryResult<LsValue>("fromKey","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),

                new QueryResult<LsValue>("toKey","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key2", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),

                new QueryResult<LsValue>("t1","svalue"),
                new QueryResult<LsToken>(",","delimiter"),
                new QueryResult<LsValue>("t2","svalue"),

                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void SingleTagWithValue()
    {
        var test = new QueryTest
        {
            RawData = "upsert edge fromKey=key1,toKey=key2, t1 = v;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("upsert"),
                new QueryResult<LsSymbol>("edge"),

                new QueryResult<LsValue>("fromKey","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),

                new QueryResult<LsValue>("toKey","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key2", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),

                new QueryResult<LsValue>("t1","lvalue"),
                new QueryResult<LsToken>("=","equal"),
                new QueryResult<LsValue>("v","rvalue"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void SingleUniqueTagWithValue()
    {
        var test = new QueryTest
        {
            RawData = "upsert unique edge fromKey=key1,toKey=key2, t1 = v;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("upsert"),
                new QueryResult<LsSymbol>("unique"),
                new QueryResult<LsSymbol>("edge"),

                new QueryResult<LsValue>("fromKey","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),

                new QueryResult<LsValue>("toKey","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key2", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),

                new QueryResult<LsValue>("t1","lvalue"),
                new QueryResult<LsToken>("=","equal"),
                new QueryResult<LsValue>("v","rvalue"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }
}
