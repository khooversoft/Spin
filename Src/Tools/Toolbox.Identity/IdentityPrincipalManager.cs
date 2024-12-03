//using Microsoft.Extensions.Logging;
//using Toolbox.Logging;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Identity;

//public class IdentityPrincipalManager
//{
//    private readonly IdentityClient _identityClient;
//    private readonly ILogger<IdentityPrincipalManager> _logger;
//    private CacheObject<PrincipalIdentity> _principalCache;

//    public IdentityPrincipalManager(IdentityClient identityClient, ILogger<IdentityPrincipalManager> logger)
//    {
//        _identityClient = identityClient.NotNull();
//        _logger = logger.NotNull();

//        _principalCache = new CacheObject<PrincipalIdentity>(TimeSpan.FromSeconds(15));
//    }

//    public async Task<Option<PrincipalIdentity>> GetPrincipalId(string principalId)
//    {
//        principalId.NotEmpty();
//        var context = new ScopeContext(_logger);

//        if (_principalCache.TryGetValue(out var principal) && principal.PrincipalId == principalId) return principal;

//        var result = await _identityClient.GetByPrincipalId(principalId, context).ConfigureAwait(false);
//        result.LogStatus(context, "PrincipalId={principalId}", [principalId]);
//        if (result.IsNotFound()) return result;

//        _principalCache.Set(result.Return());
//        return result;
//    }
//}
