//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Orleans.Concurrency;
//using Orleans.Runtime;
//using SpinCluster.sdk.Actors.Contract;
//using SpinCluster.sdk.Application;
//using Toolbox.Block;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace SpinCluster.sdk.Actors.Directory;

//public interface IDirectoryActor : IGrainWithStringKey
//{
//    Task<Option> Delete(string resourceId, string traceId);
//    Task<Option<DirectoryEntry>> Get(string resourceId, string traceId);
//    Task<Option<IReadOnlyList<DirectoryEntry>>> List(DirectoryQuery query, string traceId);
//    Task<Option> Set(DirectoryEntry value, string traceId);
//}


//[StatelessWorker]
//[Reentrant]
//public class DirectoryActor : Grain, IDirectoryActor
//{
//    private readonly IPersistentState<DirectoryModel> _state;
//    private readonly ILogger<ContractActor> _logger;

//    public DirectoryActor(
//        [PersistentState(stateName: SpinConstants.Extension.BlockStorage, storageName: SpinConstants.SpinStateStore)] IPersistentState<DirectoryModel> state,
//        ILogger<ContractActor> logger
//        )
//    {
//        _state = state.NotNull();
//        _logger = logger.NotNull();
//    }

//    public override Task OnActivateAsync(CancellationToken cancellationToken)
//    {
//        this.VerifyIdentity(SpinConstants.Schema.Directory);
//        return base.OnActivateAsync(cancellationToken);
//    }

//    public async Task<Option> Delete(string resourceId, string traceId)
//    {
//        var context = new ScopeContext(traceId, _logger);
//        if (!_state.RecordExists) return StatusCode.NotFound;

//        if (!_state.State.Directory.Remove(resourceId)) return StatusCode.NotFound;

//        await _state.WriteStateAsync();
//        context.Location().LogInformation("Removing resourceId={resourceId} from directory", resourceId);
//        return StatusCode.OK;
//    }

//    public Task<Option<DirectoryEntry>> Get(string resourceId, string traceId)
//    {
//        Option<DirectoryEntry> result = _state.RecordExists switch
//        {
//            false => StatusCode.NotFound,
//            true => _state.State.Directory.TryGetValue(resourceId, out DirectoryEntry? entry) switch
//            {
//                false => StatusCode.NotFound,
//                true => entry,
//            }
//        };

//        return result.ToTaskResult();
//    }

//    public Task<Option<IReadOnlyList<DirectoryEntry>>> List(DirectoryQuery query, string traceId)
//    {
//        Option<IReadOnlyList<DirectoryEntry>> result = _state.RecordExists switch
//        {
//            false => StatusCode.NotFound,
//            true => _state.State.Directory.Values.Where(x => query.IsMatchQuery(x)).ToArray(),
//        };

//        return result.ToTaskResult();
//    }

//    public async Task<Option> Set(DirectoryEntry value, string traceId)
//    {
//        var context = new ScopeContext(traceId, _logger);
//        var v = value.Validate().LogResult(context.Location());
//        if (v.IsError()) return v;

//        _state.State = _state.RecordExists ? _state.State : new DirectoryModel();

//        if (_state.State.Directory.TryGetValue(value.ResourceId, out DirectoryEntry? entry))
//        {
//            if (value == entry) return StatusCode.OK;
//        }

//        _state.State.Directory[value.ResourceId] = value;
//        await _state.WriteStateAsync();

//        context.Location().LogInformation("Writing directory, directoryEntry={directoryEntry}", value);
//        return StatusCode.OK;
//    }
//}
