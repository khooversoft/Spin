using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalPrivateKey;

public interface IPrincipalPrivateKeyActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<PrincipalPrivateKeyModel>> Get(string traceId);
    Task<Option> Set(PrincipalPrivateKeyModel model, string traceId);
}

public class PrincipalPrivateKeyActor : Grain, IPrincipalPrivateKeyActor
{
    private readonly IPersistentState<PrincipalPrivateKeyModel> _state;
    private readonly IValidator<PrincipalPrivateKeyModel> _validator;
    private readonly ILogger<PrincipalPrivateKeyActor> _logger;

    public PrincipalPrivateKeyActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<PrincipalPrivateKeyModel> state,
        IValidator<PrincipalPrivateKeyModel> validator,
        ILogger<PrincipalPrivateKeyActor> logger
        )
    {
        _state = state;
        _validator = validator;
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.PrincipalPrivateKey, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting PrivatePrincipalKey, actorKey={actorKey}", this.GetPrimaryKeyString());

        await _state.ClearStateAsync();
        return StatusCode.OK;
    }
    public Task<Option> Exist(string _) => new Option(_state.RecordExists && _state.State.IsActive ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();

    public Task<Option<PrincipalPrivateKeyModel>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get PrivatePrincipalKey, actorKey={actorKey}", this.GetPrimaryKeyString());

        var option = _state.RecordExists switch
        {
            true => _state.State.ToOption<PrincipalPrivateKeyModel>(),
            false => new Option<PrincipalPrivateKeyModel>(StatusCode.NotFound),
        };

        return option.ToTaskResult();
    }

    public async Task<Option> Set(PrincipalPrivateKeyModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set PrivatePrincipalKey, actorKey={actorKey}", this.GetPrimaryKeyString());

        ValidatorResult validatorResult = _validator.Validate(model).LogResult(context.Location());
        if (!validatorResult.IsValid) return new Option(StatusCode.BadRequest, validatorResult.FormatErrors());

        _state.State = model;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }
}