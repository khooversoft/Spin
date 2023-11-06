using Toolbox.Extensions;
using Toolbox.LangTools;

namespace Toolbox.Test.Tokenizer;

/// <summary>
/// select (key=key1;tags=t1) n1 -> [schedulework:active] -> (schedule) n2
/// select (key=key1;tags=t1) n1 -> [edgeType=abc*;schedulework:active] -> (schedule) n2
/// 
/// add node key=key1,tags=t1;
/// add edge fromKey=key1,toKey=key2,edgeType=et,tags=t2;
/// 
/// delete (key=key1;tags=t1)
/// delete [edgeType=abc*;schedulework:active]
/// delete (key=key1;tags=t1) -> [schedulework:active]
/// 
/// update (key=key1;tags=t1) set key=key1,tags=t1;
/// update [edgeType=abc*;schedulework:active] set fromKey=key1,toKey=key2,edgeType=et,tags=t2;
/// 
/// </summary>
public static class GraphLangGrammer
{
    public static ILangRoot ValueAssignment { get; } = new LsRoot(nameof(ValueAssignment)) + new LsValue("lvalue") + ("=", "equal") + new LsValue("rvalue");

    public static ILangRoot NodeFilterParameters
    {
        get
        {
            var valueOnly = new LsRoot("valueOnly") + new LsValue("svalue");
            var parameters = new LsRepeat("rpt-parms") + (new LsSwitch("or") + ValueAssignment + valueOnly) + new LsToken(";", "delimiter", true);

            return parameters;
        }
    }

    public static ILangRoot NodeOrEdgeFilter
    {
        get
        {
            var nodeSyntax = new LsRoot("node")
                + (new LsGroup("(", ")", "pgroup") + NodeFilterParameters)
                + new LsValue("alias", true);

            var edgeSyntax = new LsRoot("edge")
                + (new LsGroup("[", "]", "bgroup") + NodeFilterParameters)
                + new LsValue("alias", true);

            var search = new LsRepeat("repeat-root") + (new LsSwitch("instr-or") + nodeSyntax + edgeSyntax) + new LsToken("->", "next", true);

            var term = new LsRoot(nameof(NodeOrEdgeFilter)) + search + new LsToken(";", "searchTerm", true);
            return term;
        }
    }

    public static ILangRoot SetValues
    {
        get
        {
            var property = new LsRoot("property") + new LsValue("lvalue") + ("=", "equal") + new LsValue("rvalue");
            var properties = new LsRepeat("properties-rpt") + ValueAssignment + new LsToken(",", "delimiter", true);

            var setValues = new LsRoot("setValues") + properties + new LsToken(";", "term", true);
            return setValues;
        }
    }

    //public static ILangRoot AddOperation
    //{
    //    get
    //    {
    //        var rule = 
    //    }
    //}
}
