using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Storage;
using Toolbox.Azure.DataLake;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;
using Azure;

namespace SpinCluster.sdk.Storage;

public class DatalakeStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private readonly string _storageName;
    private readonly IDatalakeStore _datalakeStore;
    private readonly ILogger<DatalakeStorage> _logger;

    public DatalakeStorage(string storageName, IDatalakeStore datalakeStore, ILogger<DatalakeStorage> logger)
    {
        _storageName = storageName.NotEmpty();
        _datalakeStore = datalakeStore.NotNull();
        _logger = logger.NotNull();
    }

    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var context = new ScopeContext(_logger);
        context.Location().LogInformation("Clearing state for stateName={stateName}, grainId={grainId}", stateName, grainId);

        string path = CreatePath(grainId);

        var result = await _datalakeStore.Delete(path, context);
        if (result.IsError())
        {
            context.Location().LogError("Failed to delete state file on datalake, storageName={storageName}, stateName={stateName}, path={path}",
                _storageName, stateName, path);
        }

        grainState.State = default!;
        grainState.RecordExists = false;
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var context = new ScopeContext(_logger);
        context.Location().LogInformation("Reading state for stateName={stateName}, grainId={grainId}", stateName, grainId);

        string path = CreatePath(grainId);

        var result = await _datalakeStore.Read(path, context);
        if (result.IsError())
        {
            context.Location().LogError("Failed to read state file, storageName={storageName}, stateName={stateName}, path={path}",
                _storageName, stateName, path);

            grainState.State = default!;
            grainState.RecordExists = false;
            return;
        }

        grainState.State = result.Return().Data
            .BytesToString()
            .ToObject<T>()
            .NotNull();

        grainState.ETag = result.Return().ETag.ToString();

        context.Location().LogInformation("Read state file=file{path}, storageName={storageName}, stateName={stateName}", path, _storageName, stateName);
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var context = new ScopeContext(_logger);
        context.Location().LogInformation("Writing state for stateName={stateName}, grainId={grainId}", stateName, grainId);

        string path = CreatePath(grainId);

        byte[] data = grainState.State
            .ToJsonSafe(context.Location())
            .ToBytes();

        ETag etag = new ETag(grainState.ETag);
        var dataEtag = new DataETag(data, etag);

        var result = await _datalakeStore.Write(path, dataEtag, true, context);
        if (result.IsError())
        {
            context.Location().LogError("Failed to write state file, storageName={storageName}, stateName={stateName}, path={path}",
                _storageName, stateName, path);

            grainState.State = default!;
            grainState.RecordExists = false;
            return;
        }
    }

    private static string CreatePath(GrainId grainId) => grainId.ToString()
        .Split('/', StringSplitOptions.RemoveEmptyEntries)
        .Skip(1)
        .Join("/")
        .ToObjectId()
        .Func(x => x.Tenant + "/" + x.Path);

    public void Participate(ISiloLifecycle lifecycle) =>
    lifecycle.Subscribe(
        observerName: OptionFormattingUtilities.Name<DatalakeStorage>(_storageName),
        stage: ServiceLifecycleStage.ApplicationServices,
        onStart: (ct) =>
        {
            Console.WriteLine("Participating");
            return Task.CompletedTask;
        });
}
