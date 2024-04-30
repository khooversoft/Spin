using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Identity;

public class IdentityService
{
    private readonly ILogger<IdentityService> _logger;
    private readonly IIdentityClient _identityClient;

    public IdentityService(IIdentityClient identityActor, ILogger<IdentityService> logger)
    {
        _identityClient = identityActor.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Delete(string id, ScopeContext context)
    {
        context = context.With(_logger);

        string command = $"delete (key={ToUserKey(id)});";
        Option<GraphQueryResults> result = await _identityClient.Execute(command, context.TraceId);
        result.LogStatus(context, $"Delete user {id}");

        return result.ToOptionStatus();
    }

    public async Task<Option<PrincipalIdentity>> GetById(string id, ScopeContext context)
    {
        context = context.With(_logger);

        string command = $"select (key={ToUserKey(id)});";
        var result = await _identityClient.Execute(command, context.TraceId);
        if (result.IsError()) return result.ToOptionStatus<PrincipalIdentity>();

        var principalOption = await _identityClient.GetPrincipalIdentityActor(id).Get();
        if (principalOption.IsError()) return principalOption;

        return principalOption.Return();
    }

    public async Task<Option<PrincipalIdentity>> FindByLogin(string loginProvider, string providerKey, ScopeContext context)
    {
        context = context.With(_logger);

        string command = $"select [fromKey={ToLoginIndex(loginProvider, providerKey)}] -> (*);";
        var resultOption = await _identityClient.Execute(command, context.TraceId);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<PrincipalIdentity>();

        GraphQueryResults results = resultOption.Return();
        if (!results.HasScalarResult()) return StatusCode.NotFound;

        var id = RemoveNs(results.Get<GraphNode>().First().Key);
        var principalOption = await _identityClient.GetPrincipalIdentityActor(id).Get();
        return principalOption;
    }

    public async Task<Option<PrincipalIdentity>> GetByUserName(string userName, ScopeContext context)
    {
        context = context.With(_logger);

        string command = $"select [fromKey={ToUserNameIndex(userName)}] -> (*);";
        var resultOption = await _identityClient.Execute(command, context.TraceId);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<PrincipalIdentity>();

        GraphQueryResults results = resultOption.Return();
        if (!results.HasScalarResult()) return StatusCode.NotFound;

        var user = results.Get<GraphNode>().First().Tags.ToObject<PrincipalIdentity>();
        if (user.Validate().IsError(out Option v)) return v.ToOptionStatus<PrincipalIdentity>();

        return user;
    }

    public async Task<Option<PrincipalIdentity>> GetByEmail(string email, ScopeContext context)
    {
        context = context.With(_logger);

        string command = $"select [fromKey={ToEmailIndex(email)}] -> (*);";
        var resultOption = await _identityClient.Execute(command, context.TraceId);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<PrincipalIdentity>();

        GraphQueryResults results = resultOption.Return();
        if (!results.HasScalarResult()) return StatusCode.NotFound;

        var user = resultOption.Return().Get<GraphNode>().First().Tags.ToObject<PrincipalIdentity>();
        if (user.Validate().IsError(out Option v)) return v.ToOptionStatus<PrincipalIdentity>();

        return user;
    }

    public async Task<Option> Set(PrincipalIdentity user, string? tags, ScopeContext context)
    {
        context.With(_logger);
        if (user.Validate().LogStatus(context, $"UserId={user.Id}").IsError(out Option v)) return v;

        // Build graph commands
        var cmds = new Sequence<string>();
        cmds += $"upsert node key={ToUserKey(user.Id)}, {tags};";

        // User Name node -> user node
        if (user.UserName.IsNotEmpty())
        {
            cmds += $"upsert node key={ToUserNameIndex(user.UserName)};";
            cmds += $"add unique edge fromKey={ToUserNameIndex(user.UserName)}, toKey={ToUserKey(user.Id)};";
        }

        // Email node -> user node
        if (user.Email.IsNotEmpty())
        {
            cmds += $"upsert node key={ToEmailIndex(user.Email)};";
            cmds += $"add unique edge fromKey={ToEmailIndex(user.Email)}, toKey={ToUserKey(user.Id)};";
        }

        if (user.LoginProvider.IsNotEmpty() && user.ProviderKey.IsNotEmpty())
        {
            cmds += $"upsert node key={ToLoginIndex(user.LoginProvider, user.ProviderKey)};";
            cmds += $"add unique edge fromKey={ToLoginIndex(user.LoginProvider, user.ProviderKey)}, toKey={ToUserKey(user.Id)};";
        }

        string command = cmds.Join(Environment.NewLine);
        var result = await _identityClient.Execute(command, context.TraceId);
        if (result.IsError()) return result.LogStatus(context, command).ToOptionStatus();

        return StatusCode.OK;
    }

    private static string ToUserKey(string id) => $"user:{id.NotEmpty().ToLower()}";
    private static string ToUserNameIndex(string userName) => $"userName:{userName.NotEmpty().ToLower()}";
    private static string ToEmailIndex(string userName) => $"userEmail:{userName.NotEmpty().ToLower()}";
    private static string ToLoginIndex(string provider, string providerKey) => $"logonProvider:{provider.NotEmpty().ToLower() + "/" + providerKey.NotEmpty().ToLower()}";

    private static FrozenSet<string> _ns = new string[] { "user", "userName", "userEmail", "logonProvider" }.ToFrozenSet();
    private static string RemoveNs(string key) => _ns.Aggregate(key, (a, x) => key.StartsWith(x) ? key[0..(x.Length - 1)] : key);


    //private record class PrincipalIdentityTags
    //{
    //    public PrincipalIdentityTags(PrincipalIdentity user)
    //    {
    //        Email = user.Email;
    //        LoginProvider = user.LoginProvider;
    //        ProviderKey = user.ProviderKey;
    //    }

    //    public string? Email { get; init; }
    //    public string? LoginProvider { get; init; }
    //    public string? ProviderKey { get; init; }

    //    public override string ToString()
    //    {
    //        var query = new string?[]
    //        {
    //            Email != null ? $"email={Email}" : null,
    //            LoginProvider != null ? $"loginProvider={LoginProvider}" : null,
    //            ProviderKey != null ? $"providerKey={ProviderKey}" : null,
    //        }.Join(',');

    //        return query;
    //    }
    //}
}
