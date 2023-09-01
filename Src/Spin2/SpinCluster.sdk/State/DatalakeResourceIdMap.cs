using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Tools;
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

        var map = new Map(resourceId.Schema.NotEmpty(), resourceId.BuildPath());

        context.Location().LogInformation("Mapped resourceId={resourceId} to map={map}", resourceId.ToString(), map);
        return map;
    }

    public readonly record struct Map
    {
        public Map(string schema, string filePath) => (Schema, FilePath) = (schema, filePath);
        public string Schema { get; }
        public string FilePath { get; }
    }
}
