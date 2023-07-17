using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Configuration;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Services;

public class SiloConfigStore
{
    private readonly IDatalakeStore _datalakeStore;
    private readonly ILogger<SiloConfigStore> _logger;

    public SiloConfigStore(DatalakeLocation datalakeLocation, IDatalakeStore datalakeStore, ILogger<SiloConfigStore> logger)
    {
        DatalakeLocation = datalakeLocation.NotNull();
        _datalakeStore = datalakeStore.NotNull();
        _logger = logger.NotNull();
    }

    public DatalakeLocation DatalakeLocation { get; }

    public async Task<Option<SiloConfigOption>> Get(ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Getting silo configuration, path={path}", DatalakeLocation.Path);

        Option<DataETag> result = await _datalakeStore.Read(DatalakeLocation.Path, context);
        if (result.IsError())
        {
            context.Location().LogCritical("Reading datalake file={file} failed", DatalakeLocation.Path);
            return new Option<SiloConfigOption>(StatusCode.NotFound);
        }

        return result.Return().Data.ToObject<SiloConfigOption>().NotNull();
    }

    public async Task<StatusCode> Set(SiloConfigOption option, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Writting silo configuration, path={path}", DatalakeLocation.Path);

        DataETag data = new DataETag(option.ToBytes());

        Option<Azure.ETag> result = await _datalakeStore.Write(DatalakeLocation.Path, data, true, context);
        if (result.IsError())
        {
            context.Location().LogCritical("Failed to write silo configuration, path={path}", DatalakeLocation.Path);
            return StatusCode.BadRequest;
        }

        return StatusCode.OK;
    }
}
