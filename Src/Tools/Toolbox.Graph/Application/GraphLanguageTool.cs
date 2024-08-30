using Toolbox.Tools;

namespace Toolbox.Graph;

public static class GraphLanguageTool
{
    public static string ReadGraphLanguageRules() =>
        AssemblyResource.GetResourceString("Toolbox.Graph.Application.GraphLanguageRules.txt", typeof(GraphLanguageTool))
        .NotNull();
}
