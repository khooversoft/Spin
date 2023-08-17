using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.State;

public class DatalakeResourceIdMap
{
    private readonly ILogger<DatalakeResourceIdMap> _logger;
    public DatalakeResourceIdMap(ILogger<DatalakeResourceIdMap> logger) => _logger = logger;

    public Option<Map> MapResource(ResourceId resourceId, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Mapping resourceId={resourceId}", resourceId.ToString());

        Option<Map> map = resourceId switch
        {
            { Schema: SpinConstants.Schema.Subscription } v when IsDomain(v) => new Map(v.Schema, $"$system/{v.Domain}.json"),
            { Schema: SpinConstants.Schema.Tenant } v when IsDomain(v) => new Map(v.Schema, $"$system/{v.Domain}.json"),
            { Schema: SpinConstants.Schema.User } v when IsUser(v) => new Map(v.Schema, BuildPath(v)),
            { Schema: SpinConstants.Schema.PrincipalKey } v when IsUser(v) => new Map(v.Schema, BuildPath(v)),
            { Schema: SpinConstants.Schema.PrincipalPrivateKey } v when IsUser(v) => new Map(v.Schema, BuildPath(v)),

            _ => StatusCode.BadRequest,
        };

        context.Location().LogInformation("Mapped resourceId={resourceId} to map={map}", resourceId.ToString(), map);
        return map;
    }

    private static bool IsUser(ResourceId resourceId) => IdPatterns.IsPrincipalId($"{resourceId.User}@{resourceId.Domain}");
    private static bool IsDomain(ResourceId resourceId) => IdPatterns.IsDomain(resourceId.Domain);
    private static string BuildPath(ResourceId resourceId) => $"{resourceId.Domain}/{resourceId.User}@{resourceId.Domain}.json";

    public readonly record struct Map
    {
        public Map(string schema, string filePath) => (Schema, FilePath) = (schema, filePath);
        public string Schema { get; }
        public string FilePath { get; }
    }
}
