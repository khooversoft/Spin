using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ActorBase;

public interface IActorDataBase<T> : IGrainWithStringKey
{
    Task<StatusCode> Delete(string traceId);
    Task<SpinResponse<T>> Get(string traceId);
    Task<StatusCode> Set(T model, string traceId);
}

public abstract class ActorDataBase<T> : Grain, IActorDataBase<T>
{
    private readonly IPersistentState<T> _state;
    private readonly IValidator<T> _validator;
    private readonly ILogger _logger;

    public ActorDataBase(IPersistentState<T> state, IValidator<T> validator, ILogger logger)
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
            false => Task.FromResult(new SpinResponse<T>(StatusCode.NotFound)),
            true => Task.FromResult((SpinResponse<T>)_state.State),
        };
    }

    public virtual async Task<StatusCode> Set(T model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        context.Location().LogInformation("Setting {typeName}, id={id}, model={model}",
            typeof(T).GetTypeName(), this.GetPrimaryKeyString(), model.ToJsonPascalSafe(new ScopeContext(_logger)));

        if (!_validator.Validate(model).LogResult(context.Location()).IsValid) return StatusCode.BadRequest;

        _state.State = model;
        await _state.WriteStateAsync();

        return StatusCode.OK;
    }
}
