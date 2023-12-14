using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Tools;

public static class DirectoryTool
{
    public static IReadOnlyList<string> Find(string searchRoot, string findDirectory)
    {
        searchRoot.NotEmpty();
        findDirectory.NotEmpty();

        try
        {
            var list = Directory.GetDirectories(searchRoot, "*.*", SearchOption.AllDirectories);
            if (list.Length == 0) return Array.Empty<string>();

            var foundList = list
                .Select(x => (path: x, index: x.IndexOf(findDirectory, StringComparison.OrdinalIgnoreCase)))
                .Where(x => x.index >= 0)
                .Select(x => x.path[0..(x.index + findDirectory.Length)])
                .ToArray();

            var distinctList = foundList.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            return distinctList;
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}
