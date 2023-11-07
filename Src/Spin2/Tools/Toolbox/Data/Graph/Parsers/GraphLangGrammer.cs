using Toolbox.Extensions;
using Toolbox.LangTools;

namespace Toolbox.Data;

/// <summary>
/// 
/// select (key=key1;tags=t1) n1 -> [schedulework:active] -> (schedule) n2;
/// select (key=key1;tags=t1) n1 -> [edgeType=abc*;schedulework:active] -> (schedule) n2;
/// 
/// add node key=key1,tags=t1;
/// add edge fromKey=key1,toKey=key2,edgeType=et,tags=t2;
/// 
/// delete (key=key1;tags=t1);
/// delete [edgeType=abc*;schedulework:active];
/// delete (key=key1;tags=t1) a1 -> [schedulework:active] a2;
/// 
/// update (key=key1;tags=t1) set key=key1,tags=t1;
/// update [edgeType=abc*;schedulework:active] set fromKey=key1,toKey=key2,edgeType=et,tags=t2;
/// 
/// </summary>
public static class GraphLangGrammer
{
    public static ILangRoot ValueAssignment { get; } = new LsRoot(nameof(ValueAssignment))
        + new LsValue("lvalue")
        + ("=", "equal")
        + new LsValue("rvalue");

    public static ILangRoot SearchFilter
    {
        get
        {
            var valueOnly = new LsRoot("valueOnly") + new LsValue("svalue");

            var parameters = new LsRepeat(nameof(SearchFilter))
                + (new LsSwitch($"{nameof(SearchFilter)}-or") + ValueAssignment + valueOnly)
                + new LsToken(";", "delimiter", true);

            return parameters;
        }
    }

    public static ILangRoot SearchQuery
    {
        get
        {
            var nodeSyntax = new LsRoot("node")
                + (new LsGroup("(", ")", "node-group") + SearchFilter)
                + new LsValue("alias", true);

            var edgeSyntax = new LsRoot("edge")
                + (new LsGroup("[", "]", "edge-group") + SearchFilter)
                + new LsValue("alias", true);

            var search = new LsRepeat(nameof(SearchQuery)) + (new LsSwitch("instr-or") + nodeSyntax + edgeSyntax) + new LsToken("->", "select-next", true);
            return search;
        }
    }

    public static ILangRoot UpdateQuery
    {
        get
        {
            var nodeSyntax = new LsRoot() + (new LsGroup("(", ")", "node-group") + SearchFilter);
            var edgeSyntax = new LsRoot() + (new LsGroup("[", "]", "edge-group") + SearchFilter);

            var search = new LsSwitch(nameof(UpdateQuery)) + nodeSyntax + edgeSyntax;
            return search;
        }
    }


    public static ILangRoot SetValues
    {
        get
        {
            var properties = new LsRepeat("properties-rpt") + ValueAssignment + new LsToken(",", "value-delimiter", true);
            var setValues = new LsRoot(nameof(SetValues)) + properties;
            return setValues;
        }
    }

    public static ILangRoot Select { get; } = new LsRoot(nameof(Select)) + new LsSymbol("select") + SearchQuery + new LsToken(";", "term");

    public static ILangRoot AddOpr
    {
        get
        {
            var node = new LsRoot("addNode") + new LsSymbol("node") + SetValues;
            var edge = new LsRoot("addEdge") + new LsSymbol("edge") + SetValues;

            var rule = new LsRoot(nameof(AddOpr)) + new LsSymbol("add") + (new LsSwitch("add-sw") + node + edge) + new LsToken(";", "term");

            return rule;
        }
    }

    public static ILangRoot DeleteOpr { get; } = new LsRoot(nameof(DeleteOpr)) + new LsSymbol("delete") + SearchQuery + new LsToken(";", "term");

    public static ILangRoot Update
    {
        get
        {
            var rule = new LsRoot(nameof(Update))
                + new LsSymbol("update")
                + UpdateQuery
                + new LsSymbol("set", "update-set")
                + SetValues
                + new LsToken(";", "term");

            return rule;
        }
    }

    public static ILangRoot Root { get; } = new LsRepeat(true, "root-repeat")
        + (new LsSwitch("root-switch")
            + Select
            + AddOpr
            + DeleteOpr
            + Update
        );
}
