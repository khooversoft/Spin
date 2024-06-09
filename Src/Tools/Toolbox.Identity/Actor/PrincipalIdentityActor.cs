//using Microsoft.Extensions.Logging;
//using Orleans.Runtime;
//using Toolbox.Extensions;
//using Toolbox.Orleans;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Identity;

//public static class PrincipalIdentityActorTool
//{
//    public static IPrincipalIdentityActor GetPrincipalIdentityActor(this IClusterClient client, string resourceId) =>
//        client.GetGrain<IPrincipalIdentityActor>(resourceId);
//}

//public interface IPrincipalIdentityActor : IGrainWithStringKey
//{
//    Task Clear();
//    Task<Option<PrincipalIdentity>> Get();
//    Task<Option> Set(PrincipalIdentity principalIdentity);
//}

//public class PrincipalIdentityActor : Grain, IPrincipalIdentityActor
//{
//    private readonly ILogger<PrincipalIdentityActor> _logger;
//    private readonly ActorCacheState<PrincipalIdentity> _state;

//    public PrincipalIdentityActor(
//        [PersistentState("json", OrleansConstants.StorageProviderName)] IPersistentState<PrincipalIdentity> state,
//        ILogger<PrincipalIdentityActor> logger
//        )
//    {
//        _logger = logger.NotNull();
//        _state = new ActorCacheState<PrincipalIdentity>(state, TimeSpan.FromMinutes(15));
//    }

//    public Task Clear() => _state.Clear();

//    public Task<Option<PrincipalIdentity>> Get() => _state.GetState();

//    public async Task<Option> Set(PrincipalIdentity principalIdentity)
//    {
//        if (!principalIdentity.Validate(out Option v)) return v;
//        var context = new ScopeContext(_logger);

//        string id = this.GetPrimaryKeyString().Split('/').Skip(1).Join('/');
//        if (principalIdentity.Id != id) return (StatusCode.BadRequest, $"Actor Id={id} does not match PrincipalIdentity.Id={principalIdentity.Id}");

//        return await _state.SetState(principalIdentity);
//    }
//}
