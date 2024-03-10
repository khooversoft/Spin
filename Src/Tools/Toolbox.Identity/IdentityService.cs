using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
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

    public async Task<Option<ApplicationUser>> GetById(string id, ScopeContext context)
    {
        context = context.With(_logger);

        string command = $"select (key={ToUserKey(id)});";
        var result = await _identityClient.Execute(command, context.TraceId);
        if (result.IsError()) return result.ToOptionStatus<ApplicationUser>();

        var user = result.Return().Get<GraphNode>().First().Tags.ToObject<ApplicationUser>();
        if (user.Validate().IsError(out Option v)) return v.ToOptionStatus<ApplicationUser>();

        return user;
    }

    public async Task<Option<ApplicationUser>> GetByUserName(string userName, ScopeContext context)
    {
        context = context.With(_logger);

        string command = $"select [fromKey={ToUserNameIndex(userName)}] -> (*);";
        var resultOption = await _identityClient.Execute(command, context.TraceId);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<ApplicationUser>();

        GraphQueryResults results = resultOption.Return();
        if (!results.HasScalarResult()) return StatusCode.NotFound;

        var user = results.Get<GraphNode>().First().Tags.ToObject<ApplicationUser>();
        if (user.Validate().IsError(out Option v)) return v.ToOptionStatus<ApplicationUser>();

        return user;
    }

    public async Task<Option<ApplicationUser>> GetByEmail(string email, ScopeContext context)
    {
        context = context.With(_logger);

        string command = $"select [fromKey={ToEmailIndex(email)}] -> (*);";
        var resultOption = await _identityClient.Execute(command, context.TraceId);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<ApplicationUser>();

        GraphQueryResults results = resultOption.Return();
        if (!results.HasScalarResult()) return StatusCode.NotFound;

        var user = resultOption.Return().Get<GraphNode>().First().Tags.ToObject<ApplicationUser>();
        if (user.Validate().IsError(out Option v)) return v.ToOptionStatus<ApplicationUser>();

        return user;
    }

    public async Task<Option> Set(ApplicationUser user, string? tags, ScopeContext context)
    {
        context.With(_logger);
        if (user.Validate().LogStatus(context, $"UserId={user.Id}").IsError(out Option v)) return v;

        var t1 = new Tags()
            .SetObject(user)
            .Set(tags);

        // Build graph commands
        var cmds = new Sequence<string>();
        cmds += $"upsert node key={ToUserKey(user.Id)}, {t1};";

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

        string command = cmds.Join(Environment.NewLine);
        var result = await _identityClient.Execute(command, context.TraceId);
        if (result.IsError()) return result.LogStatus(context, command).ToOptionStatus();

        return StatusCode.OK;
    }

    private static string ToUserKey(string id) => $"user:{id.NotEmpty().ToLower()}";
    private static string ToUserNameIndex(string userName) => $"userName:{userName.NotEmpty().ToLower()}";
    private static string ToEmailIndex(string userName) => $"userEmail:{userName.NotEmpty().ToLower()}";
}
