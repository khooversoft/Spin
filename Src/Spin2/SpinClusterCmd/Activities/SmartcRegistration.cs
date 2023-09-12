using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Smartc;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class SmartcRegistration
{
    private readonly SmartcClient _client;
    private readonly ILogger<SmartcRegistration> _logger;

    public SmartcRegistration(SmartcClient client, ILogger<SmartcRegistration> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task Register(string smartcId)
    {
        var context = new ScopeContext(_logger);

        var model = new SmartcModel
        {
            SmartcId = smartcId,
            Enabled = true,
        };

        Option response = await _client.Set(model, context);
        context.Trace().LogStatus(response, "Creating/Updating SmartC, smartcId={smartcId}", smartcId);
    }

    public async Task Remove(string smartcId)
    {
        var context = new ScopeContext(_logger);

        Option response = await _client.Delete(smartcId, context);
        context.Trace().LogStatus(response, "Deleted SmartC, smartcId={smartcId}", smartcId);
    }
}
