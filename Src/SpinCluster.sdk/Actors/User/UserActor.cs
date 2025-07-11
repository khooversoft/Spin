using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using SpinCluster.sdk.Actors.Domain;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;
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
        [PersistentState(stateName: SpinConstants.Ext.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<UserModel> state,
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
        this.VerifySchema(ResourceType.Owned, SpinConstants.Schema.User, new ScopeContext(_logger).Location());
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting user, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (_state.RecordExists) await _state.ClearStateAsync();

        var userKeys = UserKeyModel.Create(getPrincipalId());

        await _clusterClient.GetResourceGrain<IPrincipalKeyActor>(userKeys.PublicKeyId).Delete(context.TraceId);
        await _clusterClient.GetResourceGrain<IPrincipalPrivateKeyActor>(userKeys.PrivateKeyId).Delete(context.TraceId);

        return StatusCode.OK;

        string getPrincipalId() => ResourceId.Create(this.GetPrimaryKeyString()).Return().PrincipalId.NotEmpty();
    }

    public async Task<Option> Create(UserCreateModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Create user, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (_state.RecordExists) return (StatusCode.Conflict, $"Record already exist, actorKey={this.GetPrimaryKeyString()}");
        if (!model.Validate(out var v)) return v;
        if (!model.UserId.EqualsIgnoreCase(this.GetPrimaryKeyString())) return (StatusCode.BadRequest, "User id does not match actorKey");

        var v2 = await VerifyDomain(model.UserId, context);
        if (v2.IsError()) return v2;

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

    public Task<Option> Exist(string traceId) => new Option(_state.RecordExists ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();

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


    public async Task<Option> Update(UserModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Update user, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!model.Validate(out var v)) return v;
        var v2 = await VerifyDomain(model.UserId, context);
        if (v2.IsError()) return v2;

        if (!_state.RecordExists)
        {
            var createKeyOption = await CreateKeys(model, context);
            if (createKeyOption.IsError()) return createKeyOption;
        }

        _state.State = model;
        await _state.WriteStateAsync();

        return StatusCode.OK;
    }

    public async Task<Option<SignResponse>> SignDigest(string messageDigest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Signing message digest, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists) return StatusCode.BadRequest;

        ResourceId privateKeyId = _state.State.UserKey.PrivateKeyId;

        Option<string> signResponse = await _clusterClient
            .GetResourceGrain<IPrincipalPrivateKeyActor>(privateKeyId)
            .Sign(messageDigest, traceId);

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

    private async Task<Option> VerifyDomain(string userId, ScopeContext context)
    {
        Option<ResourceId> user = ResourceId.Create(userId);
        if (user.IsError()) return user.ToOptionStatus();

        string domain = user.Return().Domain!;
        if (!IdPatterns.IsDomain(domain)) return StatusCode.BadRequest;

        // Handle special domain
        if (domain.EqualsIgnoreCase("spin.com")) return StatusCode.OK;

        var domainDetail = await _clusterClient
            .GetResourceGrain<IDomainActor>(SpinConstants.DomainActorKey)
            .GetDetails(domain, context.TraceId);

        if (domainDetail.IsError())
        {
            context.Location().LogError("Domain={domain} for userId={userId} is not a tenant or valid external domain", userId, domain);
            return new Option(StatusCode.Conflict, $"Domain={domain}, for principalId={userId} is not valid");
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
