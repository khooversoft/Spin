using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Orleans.Types;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ActorBase;


public abstract class ActorDataBase2<T> : Grain, IActionOperation<T>
{
    private readonly IPersistentState<T> _state;
    private readonly IValidator<T> _validator;
    private readonly ILogger _logger;

    public ActorDataBase2(IPersistentState<T> state, IValidator<T> validator, ILogger logger)
    {
        _state = state;
        _validator = validator;
        _logger = logger;
    }

    public virtual Task<SpinResponse> Delete(string traceId) => _state.Delete<T>(this.GetPrimaryKeyString(), new ScopeContext(traceId, _logger).Location());
    public virtual Task<SpinResponse> Exist(string traceId) => Task.FromResult(new SpinResponse(_state.RecordExists ? StatusCode.OK : StatusCode.NoContent));
    public virtual Task<SpinResponse<T>> Get(string traceId) => _state.Get<T>(this.GetPrimaryKeyString(), new ScopeContext(traceId, _logger).Location());
    public virtual Task<SpinResponse> Set(T model, string traceId) => _state.Set(model, this.GetPrimaryKeyString(), _validator, new ScopeContext(traceId, _logger).Location());
}
