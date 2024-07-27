using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Test.LangTools.Meta;

internal static class MetaTestTool
{
    public static string ReadGraphLanauge() => AssemblyResource.GetResourceString("Toolbox.Test.LangTools.Meta.GraphLanguage.txt", typeof(MetaTestTool)).NotNull();
}
