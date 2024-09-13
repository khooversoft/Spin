//using System.Collections.Frozen;
//using Toolbox.LangTools;

//namespace Toolbox.Graph;

///// <summary>
///// 
///// () = node
///// [] = edge
///// 
///// (...) = {* | v | k=v}
///// [...] = {* | v | k=v}
///// search = { (...) | [...] } [-> { (...) | [...] ...}
///// search = { (...) | [...] } [- { (...) | [...] ...}
///// tag = {k | k=v}[, {k | k=v}...]
///// data = {name} {{ k, k=v, ... }}
///// 
///// store data = {name} {{ [schema=json,] name=name, data64=base64 }}
///// 
///// select search
///// add {node | {unique? edge} } { tag, data }
///// upsert {node | {unique? edge} } { tag, data }
///// delete search [force]
///// update search set { tag, data }
///// 
///// "unique edge" constraint no duplicate for fromKey + toKey
///// 
///// </summary>
//public static class GraphLangGrammar
//{
//    public static FrozenSet<string> ReserveTokens { get; } = new string[]
//    {
//        "select",       "add",          "node",
//        "edge",         "delete",       "update",
//        "set",          "key",          "tags",
//        "upsert",       "unique",       "force"
//    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

//    public static ILangRoot ValueAssignment { get; } = new LsRoot(nameof(ValueAssignment))
//        + new LsValue("lvalue")
//        + ("=", "equal")
//        + new LsValue("rvalue");

//    public static ILangRoot ValueOnly { get; } = new LsRoot("valueOnly") + new LsValue("svalue");
//    public static ILangSyntax Term { get; } = new LsToken(";", "term");
//    public static ILangSyntax Delimiter { get; } = new LsToken(",", "delimiter", true);

//    public static ILangRoot DataParameter { get; } = new LsRoot(nameof(DataParameter))
//        + new LsValue("dataName")
//        + (new LsGroup("{", "}", "dataGroup")
//            + (new LsRepeat(nameof(TagParameters))
//                + (new LsSwitch($"{nameof(TagParameters)}-or") + ValueAssignment + ValueOnly)
//                + Delimiter
//                )
//            );

//    public static ILangRoot TagParameters { get; } = new LsRepeat(nameof(TagParameters))
//        + (new LsSwitch($"{nameof(TagParameters)}-or") + DataParameter + ValueAssignment + ValueOnly)
//        + Delimiter;

//    public static ILangRoot SearchQuery
//    {
//        get
//        {
//            var nodeSyntax = new LsRoot("node")
//                + (new LsGroup("(", ")", "node-group") + TagParameters)
//                + new LsValue("alias", true);

//            var edgeSyntax = new LsRoot("edge")
//                + (new LsGroup("[", "]", "edge-group") + TagParameters)
//                + new LsValue("alias", true);

//            var directedSearch = new LsRoot("directed-search")
//                + (new LsSwitch($"{nameof(SearchQuery)}-or") + nodeSyntax + edgeSyntax)
//                + new LsToken("->", "select-next", true);

//            var fullSearch = new LsRoot("full-search")
//                + (new LsSwitch($"{nameof(SearchQuery)}-or") + nodeSyntax + edgeSyntax)
//                + new LsToken("<->", "select-next", true);

//            var search = new LsRepeat(true, nameof(SearchQuery))
//                + (new LsSwitch($"{nameof(SearchQuery)}-or") + directedSearch + fullSearch);
//            //+ new LsToken("->", "select-next", true);
//            //+ new LsToken("<->", "select-both", true);

//            //+ (new LsOption("select-next-option")
//            //    + (new LsSwitch($"{nameof(SearchQuery)}-select")
//            //        + new LsToken("->", "select-next", true)
//            //        + new LsToken("-", "select-both", true)
//            //        )
//            //    );
//            return search;
//        }
//    }

//    public static ILangRoot Return { get; } = new LsRoot(nameof(Return))
//        + new LsSymbol("return")
//        + (new LsRepeat("select-return-repeat")
//            + new LsValue("svalue")
//            + Delimiter
//        );

//    public static ILangRoot Select { get; } = new LsRoot(nameof(Select))
//        + new LsSymbol("select")
//        + SearchQuery
//        + (new LsOption("select-return-option") + Return)
//        + Term;

//    public static ILangRoot AddOpr
//    {
//        get
//        {
//            var node = new LsRoot("addNode") + new LsSymbol("node") + TagParameters;

//            var onlyAdd = new LsRoot("onlyAdd") + new LsSymbol("edge");
//            var uniqueAdd = new LsRoot("uniqueAdd") + new LsSymbol("unique") + new LsSymbol("edge");
//            var edge = new LsRoot("addEdge") + (new LsSwitch("addEdge-Option") + uniqueAdd + onlyAdd) + TagParameters;

//            var rule = new LsRoot(nameof(AddOpr))
//                + new LsSymbol("add")
//                + (new LsSwitch("add-sw") + node + edge)
//                + Term;

//            return rule;
//        }
//    }

//    public static ILangRoot Upsert
//    {
//        get
//        {
//            var node = new LsRoot("addNode") + new LsSymbol("node") + TagParameters;

//            var onlyAdd = new LsRoot("onlyAdd") + new LsSymbol("edge");
//            var uniqueAdd = new LsRoot("uniqueAdd") + new LsSymbol("unique") + new LsSymbol("edge");
//            var edge = new LsRoot("addEdge") + (new LsSwitch("addEdge-Option") + uniqueAdd + onlyAdd) + TagParameters;

//            //var edge = new LsRoot("addEdge") + new LsSymbol("edge") + TagParameters;

//            var rule = new LsRoot(nameof(Upsert))
//                + new LsSymbol("upsert")
//                + (new LsSwitch("upsert-sw") + node + edge)
//                + Term;

//            return rule;
//        }
//    }

//    public static ILangRoot DeleteOpr
//    {
//        get
//        {
//            var normal = new LsRoot("delete-no-force") + SearchQuery;
//            var force = new LsRoot("delete-force") + new LsSymbol("force") + SearchQuery;

//            var rule = new LsRoot(nameof(DeleteOpr))
//                + new LsSymbol("delete")
//                + (new LsSwitch("delete-sw") + normal + force)
//                + Term;
//            return rule;
//        }
//    }

//    public static ILangRoot Update
//    {
//        get
//        {
//            var rule = new LsRoot(nameof(Update))
//                + new LsSymbol("update")
//                + SearchQuery
//                + new LsSymbol("set", "update-set")
//                + TagParameters
//                + Term;

//            return rule;
//        }
//    }

//    public static ILangRoot Root { get; } = new LsRepeat(true, "root-repeat")
//        + (new LsSwitch("root-switch")
//            + Select
//            + AddOpr
//            + Upsert
//            + DeleteOpr
//            + Update
//        );
//}
