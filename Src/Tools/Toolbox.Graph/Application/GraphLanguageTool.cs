using System.Runtime.CompilerServices;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

[assembly: InternalsVisibleTo("Toolbox.Graph.test")]
namespace Toolbox.Graph;

public static class GraphLanguageTool
{
    private static readonly MetaSyntaxRoot _root;
    private static readonly SyntaxParser _parse;

    static GraphLanguageTool()
    {
        string schema = ReadGraphLanguageRules();
        _root = MetaParser.ParseRules(schema);
        if (_root.StatusCode.IsError())
        {
            throw new ArgumentException("Failed to parse meta syntax: " + _root.Error);
        }

        _parse = new SyntaxParser(_root);
    }

    public static string ReadGraphLanguageRules() =>
        AssemblyResource.GetResourceString("Toolbox.Graph.Application.GraphLanguageRules.txt", typeof(GraphLanguageTool))
        .NotNull();

    public static SyntaxParser GetSyntaxRoot() => _parse;

    public static MetaSyntaxRoot GetMetaSyntaxRoot() => _root;
}
