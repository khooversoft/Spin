using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Services;

internal class SiloConfigStore
{
    private readonly IDatalakeStore _datalakeStore;
    private readonly ILogger<SiloConfigStore> _logger;
    private readonly DatalakeLocation _datalakeLocation;

    public SiloConfigStore(DatalakeLocation datalakeLocation, IDatalakeStore datalakeStore, ILogger<SiloConfigStore> logger)
    {
        _datalakeLocation = datalakeLocation.NotNull();
        _datalakeStore = datalakeStore.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<SiloConfigOption>> Get(ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Getting silo configuration, path={path}", _datalakeLocation.Path);

        Option<DataETag> result = await _datalakeStore.Read(_datalakeLocation.Path, context);
        if (result.IsError())
        {
            context.Location().LogCritical("Reading Datalake file={file} failed", _datalakeLocation.Path);
            return new Option<SiloConfigOption>(StatusCode.NotFound);
        }

        return result.Return().Data.ToObject<SiloConfigOption>().NotNull();
    }

    public async Task<StatusCode> Set(SiloConfigOption option, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Writting silo configuration, path={path}", _datalakeLocation.Path);

        DataETag data = new DataETag(option.ToBytes());

        Option<Azure.ETag> result = await _datalakeStore.Write(_datalakeLocation.Path, data, true, context);
        if (result.IsError())
        {
            context.Location().LogCritical("Failed to write silo configuration, , path={path}", _datalakeLocation.Path);
            return StatusCode.BadRequest;
        }

        return StatusCode.OK;
    }
}
