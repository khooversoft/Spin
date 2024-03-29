﻿using Toolbox.Data;
using Toolbox.LangTools;

namespace Toolbox.Test.Lang.Graph;

public class GraphUpdateTests
{
    private readonly ILangRoot _root;

    public GraphUpdateTests()
    {
        _root = GraphLangGrammar.Root;
    }

    [Fact]
    public void SingleNode()
    {
        var test = new QueryTest
        {
            RawData = "update (key=key1) set key=key2,tags=t2;",
            Results = new List<IQueryResult>()
            {
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
                new QueryResult<LsToken>(",", "value-delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t2", "rvalue"),

                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void EdgeToSingleNode()
    {
        var test = new QueryTest
        {
            RawData = "update [FromKey=fKey] -> (key=key1) set key=key2,tags=t2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("update"),

                new QueryResult<LsGroup>("[","edge-group"),
                new QueryResult<LsValue>("FromKey","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("fKey", "rvalue"),
                new QueryResult<LsGroup>("]","edge-group"),

                new QueryResult<LsToken>("->", "select-next"),

                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsGroup>(")","node-group"),

                new QueryResult<LsSymbol>("set"),

                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key2", "rvalue"),
                new QueryResult<LsToken>(",", "value-delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t2", "rvalue"),

                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void SingleEdge()
    {
        var test = new QueryTest
        {
            RawData = "update [edgeType=abc*;schedulework:active] set fromKey=key1,toKey=key2,edgeType=et,tags=t2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("update"),

                new QueryResult<LsGroup>("[","edge-group"),
                new QueryResult<LsValue>("edgeType","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("abc*", "rvalue"),

                new QueryResult<LsToken>(";", "delimiter"),
                new QueryResult<LsValue>("schedulework:active","svalue"),

                new QueryResult<LsGroup>("]","edge-group"),

                new QueryResult<LsSymbol>("set"),

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

        LangTestTools.Verify(_root, test);
    }
}
