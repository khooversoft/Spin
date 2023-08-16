using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Security.Jwt;
using Toolbox.Security.Principal;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalPrivateKey;

public interface IPrincipalPrivateKeyActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<PrincipalPrivateKeyModel>> Get(string traceId);
    Task<Option> Set(PrincipalPrivateKeyModel model, string traceId);
    Task<Option<string>> Sign(string messageDigest, string traceId);
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

        if (!this.GetPrimaryKeyString().EqualsIgnoreCase(model.KeyId))
        {
            return new Option(StatusCode.BadRequest, $"KeyId {model.KeyId} does not match actor id={this.GetPrimaryKeyString()}");
        }

        if (_state.RecordExists)
        {
            model = model with { PrivateKey = _state.State.PrivateKey };
        }

        _state.State = model;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }

    public async Task<Option<string>> Sign(string messageDigest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Signing with private key, actorKey={actorKey}", this.GetPrimaryKeyString());

        await _state.ReadStateAsync();
        if (!_state.RecordExists) return StatusCode.BadRequest;

        var signature = PrincipalSignature.CreateFromPrivateKeyOnly(
            _state.State.PrivateKey,
            _state.State.KeyId,
            _state.State.PrincipalId,
            _state.State.Audience
            );

        string jwtSignature = new JwtTokenBuilder()
            .SetDigest(messageDigest)
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .SetPrincipleSignature(signature)
            .Build();

        if (jwtSignature.IsEmpty())
        {
            context.Location().LogError("Failed to build JWT");
            return new Option<string>(StatusCode.BadRequest, "JWT builder failed");
        }

        return jwtSignature;
    }
}