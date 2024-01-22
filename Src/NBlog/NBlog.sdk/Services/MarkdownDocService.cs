using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class MarkdownDocService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<MarkdownDocService> _logger;

    public MarkdownDocService(IClusterClient clusterClient, ILogger<MarkdownDocService> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<MarkdownDoc>> Read(string fileId, ScopeContext context)
    {
        fileId.NotEmpty();
        context = context.With(_logger);

        context.LogInformation("Reading fileId={fileId}", fileId);
        var dataOption = await _clusterClient.GetStorageActor(fileId).Get(context.TraceId);
        if (dataOption.IsError())
        {
            context.Location().LogError("Could not find fileId={fileId}", fileId);
            return (StatusCode.NotFound, "No fileId");
        }

        DataETag data = dataOption.Return();
        if (!data.Validate(out var v)) return v.LogOnError(context, "DataETag").ToOptionStatus<MarkdownDoc>();

        return new MarkdownDoc(data.Data);
    }
}
