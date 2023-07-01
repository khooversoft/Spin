using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Types;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ActorBase;


public abstract class ActorDataBase2<T> : Grain, IActionOperation<T>
{
    private readonly IPersistentState<T> _state;
    private readonly Validator<T> _validator;
    private readonly ILogger _logger;

    public ActorDataBase2(IPersistentState<T> state, Validator<T> validator, ILogger logger)
    {
        _state = state;
        _validator = validator;
        _logger = logger;
    }

    public virtual async Task<SpinResponse<T>> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting {typeName}, id={id}", typeof(T).GetTypeName(), this.GetPrimaryKeyString());

        await _state.ClearStateAsync();
        return new SpinResponse<T>(StatusCode.OK);
    }

    public virtual Task<SpinResponse<T>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Getting {typeName}, id={id}", typeof(T).GetTypeName(), this.GetPrimaryKeyString());

        return _state.RecordExists switch
        {
            false => Task.FromResult(new SpinResponse<T>(StatusCode.NotFound)),
            true => Task.FromResult(_state.State.ToSpinResponse()),
        };
    }

    public virtual async Task<SpinResponse<T>> Set(T model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        context.Location().LogInformation("Setting {typeName}, id={id}, model={model}",
            typeof(T).GetTypeName(), this.GetPrimaryKeyString(), model.ToJsonPascalSafe(new ScopeContext(_logger)));

        ValidatorResult validatorResult = _validator.Validate(model);
        if (!validatorResult.IsValid)
        {
            context.Location().LogError(validatorResult.FormatErrors());
            return new SpinResponse<T>(StatusCode.BadRequest, validatorResult.FormatErrors());
        }

        _state.State = model;
        await _state.WriteStateAsync();

        return new SpinResponse<T>(StatusCode.OK);
    }
}
