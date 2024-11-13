//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Graph;
//using Toolbox.Identity;
//using Toolbox.Logging;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace TicketShare.sdk;

//public class IdentityMessagesClient
//{
//    private readonly ILogger<IdentityMessagesClient> _logger;
//    private readonly IGraphClient _graphClient;

//    public IdentityMessagesClient(IGraphClient graphClient, IServiceProvider service, ILogger<IdentityMessagesClient> logger)
//    {
//        _graphClient = graphClient.NotNull();
//        _logger = logger.NotNull();

//        Messages = ActivatorUtilities.CreateInstance<MessageItemClient>(service, this);
//    }

//    public MessageItemClient Messages { get; }

//    public Task<Option> Add(IdentityMessagesRecord identityMessage, ScopeContext context) => AddOrSet(false, identityMessage, context);

//    public async Task<Option> Delete(string principalId, ScopeContext context)
//    {
//        principalId.NotEmpty();
//        return await _graphClient.DeleteNode(ToIdentityMessageId(principalId), context);
//    }

//    public async Task<Option<IdentityMessagesRecord>> Get(string principalId, ScopeContext context)
//    {
//        principalId.NotEmpty();
//        return await _graphClient.GetNode<IdentityMessagesRecord>(ToIdentityMessageId(principalId), context);
//    }


//    public Task<Option> Set(IdentityMessagesRecord identityMessage, ScopeContext context) => AddOrSet(true, identityMessage, context);

//    private async Task<Option> AddOrSet(bool useSet, IdentityMessagesRecord identityMessage, ScopeContext context)
//    {
//        context = context.With(_logger);
//        if (!identityMessage.Validate(out var r)) return r.LogStatus(context, nameof(IdentityMessagesRecord));

//        string nodeKey = ToIdentityMessageId(identityMessage.PrincipalId);

//        var cmd = new NodeCommandBuilder()
//            .UseSet(useSet)
//            .SetNodeKey(nodeKey)
//            .AddForeignKeyTag("owns", IdentityClient.ToUserKey(identityMessage.PrincipalId))
//            .AddData("entity", identityMessage)
//            .Build();

//        var result = await _graphClient.Execute(cmd, context);
//        if (result.IsError())
//        {
//            context.LogError("Failed to set nodeKey={nodeKey}", nodeKey);
//            return result.LogStatus(context, $"nodeKey={nodeKey}").ToOptionStatus();
//        }

//        return result.ToOptionStatus();
//    }

//    private static string ToIdentityMessageId(string principalId) => $"identity-message:{principalId.NotEmpty().ToLowerInvariant()}";
//}
