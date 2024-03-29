﻿using System.Diagnostics;
using Toolbox.Extensions;

namespace Toolbox.Types;

public static class ResourceIdExtensions
{
    public static string BuildPath(this ResourceId resourceId)
    {
        string path = resourceId.Type switch
        {
            ResourceType.System => $"{resourceId.Schema}/{resourceId.SystemName}",
            ResourceType.Tenant => $"{resourceId.Schema}/{resourceId.Domain}",
            ResourceType.Principal => $"user/{resourceId.Domain}/{resourceId.User}@{resourceId.Domain}",
            ResourceType.Owned => $"{resourceId.Schema}/{resourceId.Domain}/{resourceId.User}@{resourceId.Domain}/{resourceId.Path}",
            ResourceType.DomainOwned => $"{resourceId.Schema}/{resourceId.Domain}/{resourceId.Path}",
            _ => throw new UnreachableException()
        };

        path = path.TrimEnd('/');
        string directory = path.Split('/').Func(x => x.Take(x.Length - 1).Join('/'));

        string filePath = directory + "/" + path.Replace('/', '_');
        return filePath;
    }

    public static string BuildPathWithExtension(this ResourceId resourceId, string extension = ".json")
    {
        return resourceId.BuildPath() + extension;
    }
}
