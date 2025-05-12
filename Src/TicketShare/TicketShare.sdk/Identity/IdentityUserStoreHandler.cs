using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Graph.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk.Identity;

public class IdentityUserStoreHandler : IdentityUserStore
{
    private readonly ILogger<IdentityUserStore> _logger;
    private readonly IGraphClient _graphClient;

    public IdentityUserStoreHandler(IdentityClient identityClient, IGraphClient graphClient, ILogger<IdentityUserStore> logger)
        : base(identityClient, logger)
    {
        _graphClient = graphClient.NotNull();
        _logger = logger.NotNull();
    }

    public override async Task<IdentityResult> CreateAsync(PrincipalIdentity user, CancellationToken cancellationToken = default)
    {
        var context = new ScopeContext(_logger);

        Option<string> cmdsOption = new CommandBatchBuilder()
            .AddIdentity(user)
            .AddAccount(new AccountRecord { PrincipalId = user.PrincipalId, Name = user.Name }, true)
            .Build(context);

        if (cmdsOption.IsError(out var r)) return r.ToIdentityResult();

        string cmd = cmdsOption.Return();

        var result = await _graphClient.Execute(cmd, context).ConfigureAwait(false);
        result.LogStatus(context, "Create user principal, account, security group for channel and channel, principalId={principalId}", [user.PrincipalId]);

        return result.ToOptionStatus().ToIdentityResult();
    }
}
