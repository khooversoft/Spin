using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class ResourceIdExtensions
{
    public static string BuildPath(this ResourceId resourceId)
    {
        string path = resourceId.Type switch
        {
            ResourceType.System => $"$system/{resourceId.SystemName}",
            ResourceType.Tenant => $"$system/{resourceId.Domain}",
            ResourceType.Principal => $"{resourceId.Domain}/{resourceId.User}@{resourceId.Domain}",
            ResourceType.Owned => $"{resourceId.Domain}/{resourceId.User}@{resourceId.Domain}/{resourceId.Path}",
            ResourceType.Account => $"{resourceId.Domain}/{resourceId.Path}",
            _ => throw new UnreachableException()
        };

        path = path.TrimEnd('/');
        string directory = path.Split('/').Func(x => x.Take(x.Length-1).Join('/'));

        string filePath = directory + "/" + path.Replace('/', '_') + ".json";
        return filePath;
    }
}
