using Toolbox.Tools;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel;

public static class GraphModelTool
{
    public static string ReadGraphLanauge2() =>
        AssemblyResource.GetResourceString("Toolbox.Test.LangTools.MetaSyntax.GraphModel.GraphLanguage2.txt", typeof(GraphModelTool)).NotNull();
}
