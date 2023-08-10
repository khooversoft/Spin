using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
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

        await _state.ClearStateAsync();
        return StatusCode.OK;
    }
    public Task<Option> Exist(string _) => new Option(_state.RecordExists && _state.State.IsActive ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();

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

        string tenantId = PrincipalId.Create(model.PrincipalId).LogResult(context.Location())
            .Return()
            .Domain;

        Option isTenantActive = await _clusterClient
            .GetObjectGrain<ITenantActor>(TenantModel.CreateId(tenantId))
            .Exist(traceId);

        if (isTenantActive.StatusCode.IsError())
        {
            context.Location().LogError("Tenant={tenantId} does not exist, error={error}", tenantId, isTenantActive.Error);
            return new Option(StatusCode.Conflict, $"Tenant={tenantId}, for principalId={model.PrincipalId} does not exist");
        }

        _state.State = model;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }
}
