﻿using Toolbox.LangTools;

namespace Toolbox.Graph.test.Lang.Graph;

public class GraphAddTests
{
    private readonly ILangRoot _root = GraphLangGrammar.Root;

    [Fact]
    public void SingleNode()
    {
        var test = new QueryTest
        {
            RawData = "add node key=key1, tags=t1;",
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
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void MissingOperatorTerm()
    {
        var test = new QueryTest
        {
            RawData = "node key=key1,tags=t1;",
            Results = new List<IQueryResult>(),
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void MissingTerm()
    {
        var test = new QueryTest
        {
            RawData = "add node key=key1,tags=t1",
            Results = new List<IQueryResult>(),
        };

        LangTestTools.Verify(_root, test);
    }


    [Fact]
    public void MissingNodeTypeTerm()
    {
        var test = new QueryTest
        {
            RawData = "add key=key1,tags=t1;",
            Results = new List<IQueryResult>(),
        };

        LangTestTools.Verify(_root, test);
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
            RawData = "add node t1;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("add"),
                new QueryResult<LsSymbol>("node","node"),
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
            RawData = "add node t1, t2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("add"),
                new QueryResult<LsSymbol>("node","node"),
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
            RawData = "add node t1 = v;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("add"),
                new QueryResult<LsSymbol>("node","node"),
                new QueryResult<LsValue>("t1","lvalue"),
                new QueryResult<LsToken>("=","equal"),
                new QueryResult<LsValue>("v","rvalue"),
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
            RawData = "add node t1 = v,t2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("add"),
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
    public void TwoTagsWithValues()
    {
        var test = new QueryTest
        {
            RawData = "add node t1 = v,t2=v2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("add"),
                new QueryResult<LsSymbol>("node","node"),
                new QueryResult<LsValue>("t1","lvalue"),
                new QueryResult<LsToken>("=","equal"),
                new QueryResult<LsValue>("v","rvalue"),
                new QueryResult<LsToken>(",","delimiter"),
                new QueryResult<LsValue>("t2","lvalue"),
                new QueryResult<LsToken>("=","equal"),
                new QueryResult<LsValue>("v2","rvalue"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void AddNodeWithLink()
    {
        var test = new QueryTest
        {
            RawData = "add node key=node1, link=a/b/c;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("add"),
                new QueryResult<LsSymbol>("node","node"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("node1", "rvalue"),
                new QueryResult<LsToken>(",","delimiter"),
                new QueryResult<LsValue>("link","lvalue"),
                new QueryResult<LsToken>("=","equal"),
                new QueryResult<LsValue>("a/b/c","rvalue"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void AddNodeWithTwoLink()
    {
        var test = new QueryTest
        {
            RawData = "add node key=node1, link=a/b/c, link=file:nodes/file.json;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("add"),
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
}
