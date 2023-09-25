using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Subscription;
using SpinClusterCmd.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class Subscription
{
    private readonly SubscriptionClient _client;
    private readonly ILogger<Subscription> _logger;

    public Subscription(SubscriptionClient client, ILogger<Subscription> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task Delete(string nameId)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Deleting Subscription nameId={nameId}", nameId);

        Option response = await _client.Delete(nameId, context);
        context.Trace().LogStatus(response, "Deleting subscription");
    }

    public async Task Get(string nameId)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Get Subscription nameId={nameId}", nameId);

        var response = await _client.Get(nameId, context);
        if (response.IsError())
        {
            context.Trace().LogError("Cannot get Subscription nameId={nameId}", nameId);
            return;
        }

        var result = response.Return()
            .GetConfigurationValues()
            .Select(x => $" - {x.Key}={x.Value}")
            .Prepend($"Subscription...")
            .Join(Environment.NewLine) + Environment.NewLine;

        context.Trace().LogInformation(result);
    }

    public async Task Set(string jsonFile)
    {
        var context = new ScopeContext(_logger);

        var readResult = CmdTools.LoadJson<SubscriptionModel>(jsonFile, SubscriptionModel.Validator, context);
        if (readResult.IsError()) return;

        SubscriptionModel model = readResult.Return();

        Option response = await _client.Set(model, context);
        context.Trace().LogStatus(response, "Creating/Updating Subscription, model={model}", model);
    }
}
