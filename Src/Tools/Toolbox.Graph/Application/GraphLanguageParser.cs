using Microsoft.Extensions.DependencyInjection;
using Toolbox.LangTools;
using Toolbox.Tools;

namespace Toolbox.Graph;

public class GraphLanguageParser
{
    private readonly SyntaxParser _parse;

    static GraphLanguageParser()
    {
        string schema = ReadGraphLanguageRules();

        Root = MetaParser.ParseRules(schema);
    }

    public static MetaSyntaxRoot Root { get; }

    public GraphLanguageParser(IServiceProvider serviceProvider)
    {
        _parse = ActivatorUtilities.CreateInstance<SyntaxParser>(serviceProvider, Root);
    }

    public SyntaxResponse Parse(string rawData) => _parse.Parse(rawData);

    private static string ReadGraphLanguageRules() =>
        AssemblyResource.GetResourceString("Toolbox.Graph.Application.GraphLanguageRules.txt", typeof(GraphLanguageParser))
        .NotNull();
}
