using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Security.Sign;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.User;

public interface IUserActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<UserModel>> Get(string traceId);
    Task<Option> Create(UserCreateModel model, string traceId);
    Task<Option> Update(UserModel model, string traceId);
    Task<Option<SignResponse>> SignDigest(string messageDigest, string traceId);
}

// actor key: user:{user}@{domain}
public class UserActor : Grain, IUserActor
{
    private readonly IPersistentState<UserModel> _state;
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<UserActor> _logger;

    public UserActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<UserModel> state,
        IClusterClient clusterClient,
        ILogger<UserActor> logger
        )
    {
        _state = state;
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
            return StatusCode.NotFound;
        }

        string publicKeyId = _state.State.UserKey.PublicKeyId;
        await _state.ClearStateAsync();

        await _clusterClient.GetResourceGrain<IPrincipalKeyActor>(publicKeyId).Delete(context.TraceId);

        return StatusCode.OK;
    }

    public async Task<Option> Exist(string traceId)
    {
        if (!_state.RecordExists || !_state.State.IsActive) return StatusCode.NotFound;

        ResourceId resourceId = IdTool.CreatePublicKeyId(_state.State.PrincipalId);
        var publicKeyExist = await _clusterClient.GetResourceGrain<IPrincipalKeyActor>(resourceId).Delete(traceId);

        return publicKeyExist;
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

    public async Task<Option> Create(UserCreateModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Create user, actorKey={actorKey}", this.GetPrimaryKeyString());

        var test = await new OptionTest()
            .Test(() => _state.RecordExists ? StatusCode.Conflict : StatusCode.OK)
            .Test(() => model.Validate().LogResult(context.Location()))
            .TestAsync(async () => await VerifyTenant(model.UserId, context));
        if (test.IsError()) return test;

        var userModel = new UserModel
        {
            UserId = model.UserId,
            PrincipalId = model.PrincipalId,
            DisplayName = model.DisplayName,
            FirstName = model.FirstName,
            LastName = model.LastName,
            AccountEnabled = true,
            UserKey = UserKeyModel.Create(model.PrincipalId),
        };

        var createOption = await CreateKeys(userModel, context);
        if (createOption.IsError()) return createOption;

        _state.State = userModel;
        await _state.WriteStateAsync();

        return StatusCode.OK;
    }

    public async Task<Option> Update(UserModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Update user, actorKey={actorKey}", this.GetPrimaryKeyString());

        var test = await new OptionTest()
            .Test(() => model.Validate().LogResult(context.Location()))
            .TestAsync(async () => await VerifyTenant(model.UserId, context));
        if (test.IsError()) return test;

        if (!_state.RecordExists)
        {
            var createKeyOption = await CreateKeys(model, context);
            if (createKeyOption.IsError()) return createKeyOption;
        }

        _state.State = model;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }

    public async Task<Option<SignResponse>> SignDigest(string messageDigest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Signing message digest, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists) return StatusCode.BadRequest;

        ResourceId privateKeyId = _state.State.UserKey.PrivateKeyId;

        Option<string> signResponse = await _clusterClient.GetResourceGrain<IPrincipalPrivateKeyActor>(privateKeyId)
            .Sign(messageDigest, traceId)
            .LogResult(context.Location());

        if (signResponse.IsError())
        {
            context.Location().LogError("Failed to sign, actorKey={actorKey}", this.GetPrimaryKeyString());
            return signResponse.ToOptionStatus<SignResponse>();
        }

        context.Location().LogInformation("Digest signed, actorKey={actorKey}", this.GetPrimaryKeyString());

        var response = new SignResponse
        {
            PrincipleId = _state.State.UserId,
            Kid = _state.State.UserKey.KeyId,
            MessageDigest = messageDigest,
            JwtSignature = signResponse.Return()
        };

        return response;
    }

    private async Task<Option> VerifyTenant(string userId, ScopeContext context)
    {
        Option<ResourceId> user = ResourceId.Create(userId);
        if (user.IsError()) return user.ToOptionStatus();

        string tenantId = user.Return().Domain!;
        if (!IdPatterns.IsDomain(tenantId)) return StatusCode.BadRequest;

        var id = $"{SpinConstants.Schema.Tenant}:{tenantId}";
        Option isTenantActive = await _clusterClient.GetResourceGrain<ITenantActor>(id).Exist(context.TraceId);

        if (isTenantActive.IsError())
        {
            context.Location().LogError("Tenant={tenantId} does not exist, error={error}", userId, isTenantActive.Error);
            return new Option(StatusCode.Conflict, $"Tenant={userId}, for principalId={userId} does not exist");
        }

        return StatusCode.OK;
    }

    private async Task<Option> CreateKeys(UserModel model, ScopeContext context)
    {
        var keyCreate = new PrincipalKeyCreateModel
        {
            PrincipalKeyId = model.UserKey.PublicKeyId,
            KeyId = model.UserKey.KeyId,
            PrincipalId = model.PrincipalId,
            Name = "sign",
            PrincipalPrivateKeyId = model.UserKey.PrivateKeyId,
        };

        var createOption = await _clusterClient.GetResourceGrain<IPrincipalKeyActor>(keyCreate.PrincipalKeyId).Create(keyCreate, context.TraceId);
        return createOption;
    }
}
