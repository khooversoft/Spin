using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Types;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.ActorBase;

public interface IActorDataBase<T> : IGrainWithStringKey
{
    Task<StatusCode> Delete(string traceId);
    Task<SpinResponse<T>> Get(string traceId);
    Task<StatusCode> Set(T model, string traceId);
}

public abstract class ActorDataBase<T> : Grain, IActorDataBase<T>
{
    private readonly IPersistentState<T> _state;
    private readonly Validator<T> _validator;
    private readonly ILogger _logger;

    public ActorDataBase(IPersistentState<T> state, Validator<T> validator, ILogger logger)
    {
        _state = state;
        _validator = validator;
        _logger = logger;
    }

    public virtual async Task<StatusCode> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting {typeName}, id={id}", typeof(T).GetTypeName(), this.GetPrimaryKeyString());

        await _state.ClearStateAsync();
        return StatusCode.OK;
    }

    public virtual Task<SpinResponse<T>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Getting {typeName}, id={id}", typeof(T).GetTypeName(), this.GetPrimaryKeyString());

        return _state.RecordExists switch
        {
            false => Task.FromResult(new SpinResponse<T>()),
            true => Task.FromResult(_state.State.ToSpinResponse()),
        };
    }

    public virtual async Task<StatusCode> Set(T model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        context.Location().LogInformation("Setting {typeName}, id={id}, model={model}",
            typeof(T).GetTypeName(), this.GetPrimaryKeyString(), model.ToJsonPascalSafe(new ScopeContext(_logger)));

        if (!_validator.Validate(model).IsValid(context.Location())) return StatusCode.BadRequest;

        _state.State = model;
        await _state.WriteStateAsync();

        return StatusCode.OK;
    }
}
