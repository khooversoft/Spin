using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
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

        string command = $"delete (key={ToUserKey(id)})";
        Option<GraphQueryResults> result = await _identityClient.Execute(command, context.TraceId);
        result.LogStatus(context, $"Delete user {id}");

        return result.ToOptionStatus();
    }

    public async Task<Option<IdentityUser>> GetById(string id, ScopeContext context)
    {
        context = context.With(_logger);

        string command = $"select (key={ToUserKey(id)})";
        var result = await _identityClient.Execute(command, context.TraceId);
        if (result.IsError()) return result.ToOptionStatus<IdentityUser>();

        var user = result.Return().Get<GraphNode>().First().Tags.ToObject<IdentityUser>();
        if (user.Validate().IsError(out Option v)) return v.ToOptionStatus<IdentityUser>();

        return user;
    }

    public async Task<Option<IdentityUser>> GetByUserName(string userName, ScopeContext context)
    {
        context = context.With(_logger);

        string command = $"select [fromKey={ToUserNameIndex(userName)}] -> (*);";
        var resultOption = await _identityClient.Execute(command, context.TraceId);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<IdentityUser>();

        GraphQueryResults results = resultOption.Return();
        if (!results.HasScalarResult()) return StatusCode.NotFound;

        var user = results.Get<GraphNode>().First().Tags.ToObject<IdentityUser>();
        if (user.Validate().IsError(out Option v)) return v.ToOptionStatus<IdentityUser>();

        return user;
    }

    public async Task<Option<IdentityUser>> GetByEmail(string email, ScopeContext context)
    {
        context = context.With(_logger);

        string command = $"select [fromKey={ToEmailIndex(email)}] -> (*);";
        var resultOption = await _identityClient.Execute(command, context.TraceId);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<IdentityUser>();

        GraphQueryResults results = resultOption.Return();
        if (!results.HasScalarResult()) return StatusCode.NotFound;

        var user = resultOption.Return().Get<GraphNode>().First().Tags.ToObject<IdentityUser>();
        if (user.Validate().IsError(out Option v)) return v.ToOptionStatus<IdentityUser>();

        return user;
    }

    public async Task<Option> Set(IdentityUser user, string? tags, ScopeContext context)
    {
        context.With(_logger);
        if (user.Validate().LogStatus(context, $"UserId={user.Id}").IsError(out Option v)) return v;

        var t1 = new Tags()
            .SetObject(user)
            .Set(tags);

        string userNodeKey = ToUserKey(user.Id);

        var command = $"upsert node key={userNodeKey} {t1}";
        var result = await _identityClient.Execute(command, context.TraceId);
        if (result.IsError()) return result.LogStatus(context, command).ToOptionStatus();

        // User Name node -> user node
        if (user.UserName.IsNotEmpty())
        {
            if ((await AddIndex(ToUserNameIndex(user.UserName), context)).IsError(out Option v1)) return v1;
            if ((await AddEdge(ToUserNameIndex(user.UserName), ToUserKey(user.Id), context)).IsError(out Option v2)) return v2;
        }

        // Email node -> user node
        if (user.Email.IsNotEmpty())
        {
            if ((await AddIndex(ToEmailIndex(user.Email), context)).IsError(out Option v1)) return v1;
            if ((await AddEdge(ToEmailIndex(user.Email), ToUserKey(user.Id), context)).IsError(out Option v2)) return v2;
        }

        return StatusCode.OK;
    }

    private async Task<Option> AddIndex(string indexName, ScopeContext context)
    {
        var command = $"upsert node key={indexName}";

        var result = await _identityClient.Execute(command, context.TraceId);
        if (result.IsError()) return result.LogStatus(context, command).ToOptionStatus();

        return StatusCode.OK;
    }

    private async Task<Option> AddEdge(string indexName, string toKey, ScopeContext context)
    {
        var command = $"add unique edge fromKey={indexName}, toKey={toKey}";
        var result = await _identityClient.Execute(command, context.TraceId);
        if (result.IsError()) return result.LogStatus(context, command).ToOptionStatus();

        return StatusCode.OK;
    }

    private static string ToUserKey(string id) => $"user:{id.NotEmpty().ToLower()}";
    private static string ToUserNameIndex(string userName) => $"userName:{userName.NotEmpty().ToLower()}";
    private static string ToEmailIndex(string userName) => $"userEmail:{userName.NotEmpty().ToLower()}";
}
