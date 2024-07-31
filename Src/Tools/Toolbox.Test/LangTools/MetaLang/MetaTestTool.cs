using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.LangTools.Meta;

internal static class MetaTestTool
{
    public static string ReadGraphLanauge() => AssemblyResource.GetResourceString("Toolbox.Test.LangTools.MetaLang.GraphLanguage.txt", typeof(MetaTestTool)).NotNull();

    public static IReadOnlyList<string> GenerateTestCodeFromProductionRule(this ProductionRule subject)
    {
        var lines = ScanSyntaxTree(subject.Children);
        var formattedLines = HandleIndent(lines);

        return formattedLines;
    }

    private static IReadOnlyList<string> ScanSyntaxTree(IEnumerable<IMetaSyntax> children)
    {
        var seq = new Sequence<string>();

        foreach (var child in children)
        {
            IReadOnlyList<string> lines = child switch
            {
                ProductionRule rule => GenerateProductionRule(rule),
                TerminalSymbol terminal => GenerateTerminalSymbol(terminal).ToEnumerable().ToArray(),
                ProductionRuleReference reference => GenerateProductionRuleReference(reference).ToEnumerable().ToArray(),
                VirtualTerminalSymbol vir => GenerateVirtualTerminalSymbol(vir).ToEnumerable().ToArray(),

                _ => throw new InvalidOperationException(),
            };

            seq += lines;
        }

        return seq;
    }

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

    private static IReadOnlyList<string> GenerateProductionRule(ProductionRule rule)
    {
        var template = new string?[]
            {
            "new ProductionRule",
            "{",
           $"    Name = \"{rule.Name}\",",
           $"    Type = ProductionRuleType.{rule.Type},",
           $"    EvaluationType = EvaluationType.{rule.EvaluationType},",
            "    Children = new IMetaSyntax[]",
            "    {",
            null,
            "    },",
            "},",
            };

        var lines = template
            .SelectMany(x => x switch
            {
                null => ScanSyntaxTree(rule.Children),
                _ => [x],
            })
            .ToArray();

        return lines;
    }

    private static string GenerateTerminalSymbol(TerminalSymbol terminalSymbol)
    {
        string tags = terminalSymbol.Tags.Count == 0 ? string.Empty : $", Tags = [{(terminalSymbol.Tags.Select(x => $"\"{x}\"").Join(','))}]";
        return $"new TerminalSymbol {{ Name = \"{terminalSymbol.Name}\", Text = \"{terminalSymbol.Text}\", Type = TerminalType.{terminalSymbol.Type}{tags} }},";
    }

    private static string GenerateProductionRuleReference(ProductionRuleReference productionRuleReference)
    {
        return $"new ProductionRuleReference {{ Name = \"{productionRuleReference.Name}\", ReferenceSyntax = \"{productionRuleReference.ReferenceSyntax}\" }},";
    }

    private static string GenerateVirtualTerminalSymbol(VirtualTerminalSymbol virtualTerminalSymbol)
    {
        return $"new VirtualTerminalSymbol {{ Name = \"{virtualTerminalSymbol.Name}\", Text = \"{virtualTerminalSymbol.Text}\" }},";
    }

    public static void FlattenMatch(this IReadOnlyList<IMetaSyntax> syntaxes, IReadOnlyList<IMetaSyntax> expected)
    {
        var matchTo = syntaxes.Flatten();
        var matchFrom = expected.Flatten();
        matchTo.Count.Should().Be(matchFrom.Count);

        foreach (var (to, from) in matchTo.Zip(matchFrom))
        {
            bool match = to.Equals(from);
            match.Should().BeTrue($"to: {to}, from: {from}");
        }
    }

    public static IReadOnlyList<IMetaSyntax> Flatten(this IEnumerable<IMetaSyntax> syntaxes)
    {
        var seq = new Sequence<IMetaSyntax>();

        foreach (var syntax in syntaxes)
        {
            if (syntax is ProductionRule rule)
            {
                seq += rule;
                seq += rule.Children.Flatten();
            }
            else
            {
                seq += syntax;
            }
        }

        return seq;
    }   
}
