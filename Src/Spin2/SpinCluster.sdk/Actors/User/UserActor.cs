using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.User;

public interface IUserActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<UserModel>> Get(string traceId);
    Task<Option> Set(UserModel model, string traceId);
}


public class UserActor : Grain, IUserActor
{
    private readonly IPersistentState<UserModel> _state;
    private readonly IValidator<UserModel> _validator;
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<UserActor> _logger;

    public UserActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<UserModel> state,
        IValidator<UserModel> validator,
        IClusterClient clusterClient,
        ILogger<UserActor> logger
        )
    {
        _state = state;
        _validator = validator;
        _clusterClient = clusterClient;
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.User, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting user, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists)
        {
            await _state.ClearStateAsync();
            return StatusCode.OK;
        }

        ObjectId publicKeyId = PrincipalKeyModel.CreateId(_state.State.PrincipalId);
        await _clusterClient.GetObjectGrain<IPrincipalKeyActor>(publicKeyId).Delete(context.TraceId);

        await _state.ClearStateAsync();
        return StatusCode.OK;
    }

    public async Task<Option> Exist(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!_state.RecordExists || !_state.State.IsActive) return StatusCode.NotFound;

        var principalKeyId = PrincipalKeyModel.CreateId(_state.State.PrincipalId);
        var privateKeyExist = await _clusterClient.GetObjectGrain<IPrincipalKeyActor>(principalKeyId).Exist(context.TraceId);

        return privateKeyExist;
    }

    public Task<Option<UserModel>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get user, actorKey={actorKey}", this.GetPrimaryKeyString());

        var option = _state.RecordExists switch
        {
            true => _state.State.ToOption<UserModel>(),
            false => new Option<UserModel>(StatusCode.NotFound),
        };

        return option.ToTaskResult();
    }

    public async Task<Option> Set(UserModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set user, actorKey={actorKey}", this.GetPrimaryKeyString());

        ValidatorResult validatorResult = _validator.Validate(model).LogResult(context.Location());
        if (!validatorResult.IsValid) return new Option(StatusCode.BadRequest, validatorResult.FormatErrors());

        PrincipalId principalId = model.PrincipalId;

        var verifyTenant = await VerifyTenant(principalId, context);
        if (verifyTenant.StatusCode.IsError()) return verifyTenant;

        if (!_state.RecordExists)
        {
            var createKeyOption = await CreateKeys(model, context);
            if (createKeyOption.StatusCode.IsError()) return createKeyOption;
        }

        _state.State = model;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }

    private async Task<Option> VerifyTenant(PrincipalId principalId, ScopeContext context)
    {
        string tenantId = PrincipalId.Create(principalId).LogResult(context.Location())
            .Return()
            .Domain;

        Option isTenantActive = await _clusterClient
            .GetObjectGrain<ITenantActor>(TenantModel.CreateId(tenantId))
            .Exist(context.TraceId);

        if (isTenantActive.StatusCode.IsError())
        {
            context.Location().LogError("Tenant={tenantId} does not exist, error={error}", tenantId, isTenantActive.Error);
            return new Option(StatusCode.Conflict, $"Tenant={tenantId}, for principalId={principalId} does not exist");
        }

        return StatusCode.OK;
    }

    private async Task<Option> CreateKeys(UserModel model, ScopeContext context)
    {
        ObjectId keyId = PrincipalKeyModel.CreateId(model.PrincipalId);

        var keyCreate = new PrincipalKeyCreateModel
        {
            KeyId = keyId,
            PrincipalId = model.PrincipalId,
            Name = "signVerify",
            AccountEnabled = true,
        };

        var createOption = await _clusterClient.GetObjectGrain<IPrincipalKeyActor>(keyId).Create(keyCreate, context.TraceId);
        return createOption;
    }
}
