using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Application;

public interface IActorDataBase<T> : IGrainWithStringKey
{
    Task<StatusCode> Delete();
    Task<Option<T>> Get();
    Task<StatusCode> Set(T model);
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

    protected ScopeContext GetScopeContext() => RequestContext.Get(nameof(ScopeContext)) switch
    {
        null => new ScopeContext(_logger),
        ScopeContext v => v.With(_logger),
        _ => throw new InvalidOperationException(),
    };

    public async Task<StatusCode> Delete()
    {
        GetScopeContext().Location().LogInformation("Deleting {typeName}, id={id}", typeof(T).GetTypeName(), this.GetPrimaryKeyString());

        await _state.ClearStateAsync();
        return StatusCode.OK;
    }

    public Task<Option<T>> Get()
    {
        GetScopeContext().Location().LogInformation("Getting {typeName}, id={id}", typeof(T).GetTypeName(), this.GetPrimaryKeyString());

        return _state.RecordExists switch
        {
            false => Task.FromResult(Option<T>.None),
            true => Task.FromResult(_state.State.ToOption()),
        };
    }

    public async Task<StatusCode> Set(T model)
    {
        GetScopeContext().Location().LogInformation("Setting {typeName}, id={id}, model={model}",
            typeof(T).GetTypeName(), this.GetPrimaryKeyString(), model.ToJsonPascalSafe(new ScopeContext(_logger)));

        if (!_validator.Validate(model).IsValid(GetScopeContext().Location())) return StatusCode.BadRequest;

        _state.State = model;
        await _state.WriteStateAsync();

        return StatusCode.OK;
    }
}
