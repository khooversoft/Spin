using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphLanguageTool
{
    private static readonly MetaSyntaxRoot _root;
    private static readonly SyntaxParser _parse;

    static GraphLanguageTool()
    {
        string schema = ReadGraphLanguageRules();
        _root = MetaParser.ParseRules(schema);
        _root.Assert(x => x.StatusCode.IsOk(), "Failed tp parse meta syntax");
        _parse = new SyntaxParser(_root);
    }

    public static string ReadGraphLanguageRules() =>
        AssemblyResource.GetResourceString("Toolbox.Graph.Application.GraphLanguageRules.txt", typeof(GraphLanguageTool))
        .NotNull();

    public static SyntaxParser GetSyntaxRoot() => _parse;
}
