//using System.Diagnostics;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.LangTools;

//public record MetaSyntaxContext
//{
//    public ProductionRule RootRule { get; init; } = new ProductionRule { Name = "Root", Type = ProductionRuleType.Root };
//    public Dictionary<string, IMetaSyntax> Nodes { get; init; } = new Dictionary<string, IMetaSyntax>(StringComparer.OrdinalIgnoreCase);

//    public void Add(IMetaSyntax syntax)
//    {
//        syntax.NotNull();

//        switch (syntax)
//        {
//            case ProductionRule rule:
//                Nodes.TryAdd(rule.Name, syntax).Assert(x => x == true, $"Syntax node '{rule.Name}' already exists");
//                RootRule.NotNull().Children.Add(syntax);
//                return;

//            case ProductionRuleReference:
//            case VirtualTerminalSymbol:
//                RootRule.NotNull().Children.Add(syntax);
//                return;

//            case TerminalSymbol terminalSymbol:
//                Nodes.TryAdd(terminalSymbol.Name, syntax).Assert(x => x == true, $"Syntax node '{terminalSymbol.Name}' already exists");
//                RootRule.NotNull().Children.Add(syntax);
//                return;

//            default:
//                throw new UnreachableException();
//        }
//    }
//}


//public static class MetaSyntaxContextExtensions
//{
//    public static MetaSyntaxContext Clone(this MetaSyntaxContext subject) => new MetaSyntaxContext
//    {
//        Nodes = subject.Nodes.ToDictionary(),
//        RootRule = subject.RootRule,
//    };

//    public static MetaSyntaxContext Clone(this MetaSyntaxContext subject, ProductionRuleGroup group) => new MetaSyntaxContext
//    {
//        Nodes = subject.Nodes.ToDictionary(),
//        RootRule = group,
//    };
//}
