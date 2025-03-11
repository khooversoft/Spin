using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;

namespace Toolbox.Azure;

public static class DatalakeTool
{
    public static string WithBasePath(this DatalakeOption subject, string? path) => (subject.BasePath, path) switch
    {
        (string v, null) => v,
        (string v1, string v2) => (v1 + "/" + v2)
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Join('/'),

        _ => throw new ArgumentException("BasePath and Path is null"),
    };

    public static string RemoveBaseRoot(this DatalakeOption subject, string path)
    {
        string newPath = path[(subject.BasePath?.Length ?? 0)..];
        if (newPath.StartsWith("/")) newPath = newPath[1..];

        return newPath;
    }
}
