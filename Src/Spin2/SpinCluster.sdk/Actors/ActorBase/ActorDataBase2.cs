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
    private readonly IValidator<T> _validator;
    private readonly ILogger _logger;

    public ActorDataBase2(IPersistentState<T> state, IValidator<T> validator, ILogger logger)
    {
        _state = state;
        _validator = validator;
        _logger = logger;
    }

    public virtual async Task<SpinResponse> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting {typeName}, id={id}", typeof(T).GetTypeName(), this.GetPrimaryKeyString());

        await _state.ClearStateAsync();
        return new SpinResponse(StatusCode.OK);
    }

    public virtual async Task<SpinResponse> Exist(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        await _state.ReadStateAsync();
        StatusCode state = _state.RecordExists ? StatusCode.OK : StatusCode.NotFound;
        context.Location().LogInformation("Checking if {typeName} exist, id={id}, statusCode={statusCode}", typeof(T).GetTypeName(), this.GetPrimaryKeyString(), state);
        return new SpinResponse(state);
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

    public virtual async Task<SpinResponse> Set(T model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        context.Location().LogInformation("Setting {typeName}, id={id}, model={model}",
            typeof(T).GetTypeName(), this.GetPrimaryKeyString(), model.ToJsonPascalSafe(new ScopeContext(_logger)));

        ValidatorResult validatorResult = _validator.Validate(model);
        if (!validatorResult.IsValid)
        {
            context.Location().LogError(validatorResult.FormatErrors());
            return new SpinResponse(StatusCode.BadRequest, validatorResult.FormatErrors());
        }

        _state.State = model;
        await _state.WriteStateAsync();

        return new SpinResponse(StatusCode.OK);
    }
}
