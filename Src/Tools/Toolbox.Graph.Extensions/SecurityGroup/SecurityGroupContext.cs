//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph.Extensions;

//public readonly struct SecurityGroupContext
//{
//    private readonly IGraphClient _graphClient;
//    private readonly string _securityGroupId;
//    private readonly string _principalId;
//    private readonly ILogger _logger;

//    public SecurityGroupContext(IGraphClient graphClient, string securityGroupId, string principalId, ILogger logger)
//    {
//        _graphClient = graphClient.NotNull();
//        _securityGroupId = securityGroupId.NotEmpty();
//        _principalId = principalId.NotNull();
//        _logger = logger.NotNull();
//    }

//    public async Task<Option> Delete(ScopeContext context)
//    {
//        if ((await GetInternal(SecurityAccess.Contributor, context)).IsError(out var status)) return status.LogStatus(context, "Delete");

//        return await _graphClient.DeleteNode(SecurityGroupTool.ToNodeKey(_securityGroupId), context).ConfigureAwait(false);
//    }

//    public Task<Option<SecurityGroupRecord>> Get(ScopeContext context) => GetInternal(SecurityAccess.Reader, context);

//    public async Task<Option> DeleteAccess(string principalId, ScopeContext context)
//    {
//        principalId.NotEmpty();

//        var read = await GetInternal(SecurityAccess.Owner, context).ConfigureAwait(false);
//        if (read.IsError()) return read.ToOptionStatus();
//        var record = read.Return();

//        var newRecord = record with
//        {
//            Members = record.Members
//                .ToDictionary(StringComparer.OrdinalIgnoreCase)
//                .Action(x => x.Remove(principalId)),
//        };

//        return await Set(record, context).ConfigureAwait(false);
//    }

//    public async Task<Option> Set(SecurityGroupRecord securityGroupRecord, ScopeContext context)
//    {
//        context = context.With(_logger);
//        if (securityGroupRecord.Validate().IsError(out var r)) return r.LogStatus(context, nameof(SecurityGroupRecord));
//        if (securityGroupRecord.SecurityGroupId != _securityGroupId) return (StatusCode.Conflict, "SecurityGroupId does not match context");

//        var hasAccess = await GetInternal(SecurityAccess.Contributor, context);
//        if (!hasAccess.IsNotFound() && hasAccess.IsError(out var r2)) return r2.LogStatus(context, "Set");

//        var cmdOption = securityGroupRecord.CreateQuery(true, context);
//        if (cmdOption.IsError()) return cmdOption.ToOptionStatus();

//        var cmd = cmdOption.Return();
//        var result = await _graphClient.Execute(cmd, context).ConfigureAwait(false);
//        result.LogStatus(context, "Set security group, nodeKey={nodeKey}", [securityGroupRecord.SecurityGroupId]);

//        return result.ToOptionStatus();
//    }

//    public async Task<Option> SetAccess(string principalId, SecurityAccess access, ScopeContext context)
//    {
//        principalId.NotEmpty();

//        var read = await GetInternal(SecurityAccess.Owner, context).ConfigureAwait(false);
//        if (read.IsError()) return read.ToOptionStatus();
//        var record = read.Return();

//        var memberAccess = new PrincipalAccess { PrincipalId = principalId, Access = access };
//        var newRecord = record with
//        {
//            Members = record.Members
//                .ToDictionary(StringComparer.OrdinalIgnoreCase)
//                .Action(x => x[principalId] = memberAccess),
//        };

//        return await Set(newRecord, context).ConfigureAwait(false);
//    }

//    private async Task<Option<SecurityGroupRecord>> GetInternal(SecurityAccess accessRequired, ScopeContext context)
//    {
//        var subject = await _graphClient.GetNode<SecurityGroupRecord>(SecurityGroupTool.ToNodeKey(_securityGroupId), context).ConfigureAwait(false);
//        if (subject.IsError()) return subject;

//        var read = subject.Return();
//        if (read.HasAccess(_principalId, accessRequired).IsError(out var status))
//        {
//            context.LogWarning("Access denied, securityGroupId={securityGroupId}, principalId={principalId}", _securityGroupId, _principalId);
//            return status.ToOptionStatus<SecurityGroupRecord>();
//        }

//        return read;
//    }
//}
