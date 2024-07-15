using Toolbox.LangTools;

namespace Toolbox.Graph.test.Lang.Syntax;

public class UpsertNodeTests
{
    private readonly ILangRoot _root = GraphLangGrammar.Root;

    [Fact]
    public void SingleNode()
    {
        var test = new QueryTest
        {
            RawData = "upsert node key=key1, tags=t1;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("upsert"),
                new QueryResult<LsSymbol>("node"),

                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t1", "rvalue"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void TwoTagsWithOneValue()
    {
        var test = new QueryTest
        {
            RawData = "upsert node t1 = v,t2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("upsert"),
                new QueryResult<LsSymbol>("node","node"),
                new QueryResult<LsValue>("t1","lvalue"),
                new QueryResult<LsToken>("=","equal"),
                new QueryResult<LsValue>("v","rvalue"),
                new QueryResult<LsToken>(",","delimiter"),
                new QueryResult<LsValue>("t2","svalue"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void UpsertNodeWithTwoLink()
    {
        var test = new QueryTest
        {
            RawData = "upsert node key=node1, link=a/b/c, link=file:nodes/file.json;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("upsert"),
                new QueryResult<LsSymbol>("node","node"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("node1", "rvalue"),
                new QueryResult<LsToken>(",","delimiter"),
                new QueryResult<LsValue>("link","lvalue"),
                new QueryResult<LsToken>("=","equal"),
                new QueryResult<LsValue>("a/b/c","rvalue"),
                new QueryResult<LsToken>(",","delimiter"),
                new QueryResult<LsValue>("link","lvalue"),
                new QueryResult<LsToken>("=","equal"),
                new QueryResult<LsValue>("file:nodes/file.json","rvalue"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void SingleData()
    {
        var test = new QueryTest
        {
            RawData = "upsert node key=node1, entity { abc };",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("upsert"),
                new QueryResult<LsSymbol>("node","node"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("node1", "rvalue"),
                new QueryResult<LsToken>(",","delimiter"),
                new QueryResult<LsValue>("entity", "dataName"),
                new QueryResult<LsGroup>("{","dataGroup"),
                new QueryResult<LsValue>("abc","svalue"),
                new QueryResult<LsGroup>("}","dataGroup"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }
}
