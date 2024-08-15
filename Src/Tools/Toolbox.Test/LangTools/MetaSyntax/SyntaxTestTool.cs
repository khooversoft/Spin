using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.LangTools.Meta;
using Toolbox.Types;

namespace Toolbox.Test.LangTools.MetaSyntax;

internal static class SyntaxTestTool
{
    public static IReadOnlyList<string> GenerateTestCodeSyntaxTree(this SyntaxTree subject)
    {
        var lines = GenerateSyntaxTree(subject);
        var formattedLines = HandleIndent(lines);

        return formattedLines;
    }

    public static IReadOnlyList<string> GenerateSyntaxPairs(this IReadOnlyList<SyntaxPair> pairs)
    {
        var seq = new Sequence<string>();

        foreach (var pair in pairs)
        {
            IReadOnlyList<string> lines = GenerateSyntaxPair(pair);
            seq += lines;
        }

        return seq;
    }

    private static IReadOnlyList<string> GenerateSyntaxTree(SyntaxTree rootTree)
    {
        var seq = new Sequence<string>();
        seq += "new SyntaxTree";
        seq += "{";

        if (rootTree.MetaSyntaxName != null)
        {
            seq += $"MetaSyntaxName = \"{rootTree.MetaSyntaxName}\",";
        }

        seq += "Children = new ISyntaxTree[]";
        seq += "{";

        foreach (var child in rootTree.Children)
        {
            IReadOnlyList<string> lines = child switch
            {
                SyntaxTree tree => GenerateSyntaxTree(tree),
                SyntaxPair pair => GenerateSyntaxPair(pair),

                _ => throw new InvalidOperationException(),
            };

            seq += lines;
        }

        seq += "},";
        seq += "},";

        return seq;
    }

    private static IReadOnlyList<string> GenerateSyntaxPair(SyntaxPair pair)
    {
        var lines = new string[]
        {
            $"new SyntaxPair {{ Token = {GenerateToken(pair.Token)}, MetaSyntaxName = \"{pair.MetaSyntaxName}\" }},",
        };

        return lines;
    }

    private static string GenerateToken(IToken token) => token switch
    {
        TokenValue v => $"new TokenValue(\"{v.Value}\")",
        BlockToken v => $"new BlockToken(\"{v.StartSignal}{v.Value}{v.StopSignal}\", '{HandleChar(v.StartSignal)}', '{HandleChar(v.StopSignal)}', {v.Index})",
        _ => throw new ArgumentException(),
    };

    private static string HandleChar(char chr) => chr switch
    {
        '\'' => @"\'",
        _ => chr.ToString(),
    };

    private static IReadOnlyList<string> GenerateMetaSyntax(IMetaSyntax metaSyntax) => metaSyntax switch
    {
        ProductionRule v => MetaTestTool.GenerateProductionRule(v),
        TerminalSymbol v => MetaTestTool.GenerateTerminalSymbol(v).ToEnumerable().ToArray(),
        VirtualTerminalSymbol v => MetaTestTool.GenerateVirtualTerminalSymbol(v).ToEnumerable().ToArray(),

        _ => throw new InvalidOperationException(),
    };

    private static IReadOnlyList<string> HandleIndent(IReadOnlyList<string> lines)
    {
        int indent = 0;

        var output = new Sequence<string>();

        foreach (var item in lines)
        {
            string line = item.Trim();
            if (line.StartsWith("}")) indent--;

            output += new string(' ', indent * 4) + line;

            if (line.StartsWith("{")) indent++;
        }

        return output;
    }
}
