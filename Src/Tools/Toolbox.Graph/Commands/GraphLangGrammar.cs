using System.Collections.Frozen;
using Toolbox.LangTools;

namespace Toolbox.Graph;

/// <summary>
/// 
/// () = node
/// [] = edge
/// 
/// (...) = {* | v | k=v}
/// [...] = {* | v | k=v}
/// search = { (...) | [...] } [-> { (...) | [...] ...}
/// 
/// select search
/// add {node | {unique? edge} } {k | k=v}[, {k | k=v} ...]
/// upsert {node | edge} {k | k=v}[, {k | k=v} ...]
/// delete search
/// update search set {k=v}[, {k=v} ...]
/// 
/// "unique edge" constraint no duplicate for fromKey + toKey
/// 
/// </summary>
public static class GraphLangGrammar
{
    public static FrozenSet<string> ReserveTokens { get; } = new string[]
    {
        "select",       "add",          "node",
        "edge",         "delete",       "update",
        "set",          "key",          "tags",
        "upsert",       "unique",
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static ILangRoot ValueAssignment { get; } = new LsRoot(nameof(ValueAssignment))
        + new LsValue("lvalue")
        + ("=", "equal")
        + new LsValue("rvalue");

    public static ILangRoot TagParameters
    {
        get
        {
            var valueOnly = new LsRoot("valueOnly") + new LsValue("svalue");

            var parameters = new LsRepeat(nameof(TagParameters))
                + (new LsSwitch($"{nameof(TagParameters)}-or") + ValueAssignment + valueOnly)
                + new LsToken(",", "delimiter", true);

            return parameters;
        }
    }

    public static ILangRoot SearchQuery
    {
        get
        {
            var nodeSyntax = new LsRoot("node")
                + (new LsGroup("(", ")", "node-group") + TagParameters)
                + new LsValue("alias", true);

            var edgeSyntax = new LsRoot("edge")
                + (new LsGroup("[", "]", "edge-group") + TagParameters)
                + new LsValue("alias", true);

            var search = new LsRepeat(nameof(SearchQuery)) + (new LsSwitch($"{nameof(SearchQuery)}-or") + nodeSyntax + edgeSyntax) + new LsToken("->", "select-next", true);
            return search;
        }
    }

    public static ILangRoot Select { get; } = new LsRoot(nameof(Select)) + new LsSymbol("select") + SearchQuery + new LsToken(";", "term");

    public static ILangRoot AddOpr
    {
        get
        {
            var node = new LsRoot("addNode") + new LsSymbol("node") + TagParameters;

            var onlyAdd = new LsRoot("onlyAdd") + new LsSymbol("edge");
            var uniqueAdd = new LsRoot("uniqueAdd") + new LsSymbol("unique", "unique") + new LsSymbol("edge");
            var edge = new LsRoot("addEdge") + (new LsSwitch("addEdge-Option") + uniqueAdd + onlyAdd) + TagParameters;

            var rule = new LsRoot(nameof(AddOpr)) + new LsSymbol("add") + (new LsSwitch("add-sw") + node + edge) + new LsToken(";", "term");

            return rule;
        }
    }

    public static ILangRoot Upsert
    {
        get
        {
            var node = new LsRoot("addNode") + new LsSymbol("node") + TagParameters;
            var edge = new LsRoot("addEdge") + new LsSymbol("edge") + TagParameters;

            var rule = new LsRoot(nameof(Upsert)) + new LsSymbol("upsert") + (new LsSwitch("upsert-sw") + node + edge) + new LsToken(";", "term");

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
                + SearchQuery
                + new LsSymbol("set", "update-set")
                + TagParameters
                + new LsToken(";", "term");

            return rule;
        }
    }

    public static ILangRoot Root { get; } = new LsRepeat(true, "root-repeat")
        + (new LsSwitch("root-switch")
            + Select
            + AddOpr
            + Upsert
            + DeleteOpr
            + Update
        );
}
