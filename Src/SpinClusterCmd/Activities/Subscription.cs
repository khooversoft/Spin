﻿using Microsoft.Extensions.Logging;
using SpinClient.sdk;
using SpinCluster.abstraction;
using SpinClusterCmd.Application;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class Subscription : ICommandRoute
{
    private readonly SubscriptionClient _client;
    private readonly ILogger<Subscription> _logger;

    public Subscription(SubscriptionClient client, ILogger<Subscription> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("subscription", "Subscription management")
    {
        new CommandSymbol("delete", "Delete a subscription").Action(command =>
        {
            var nameId = command.AddArgument<string>("nameId", "Name of subscription");
            command.SetHandler(Delete, nameId);
        }),
        new CommandSymbol("get", "Get subscription details").Action(command =>
        {
            var nameId = command.AddArgument<string>("nameId", "Name of subscription");
            command.SetHandler(Get, nameId);
        }),
        new CommandSymbol("set", "Create or update subscription details").Action(command =>
        {
            var jsonFile = command.AddArgument<string>("jsonFile", "Json with subscription details");
            command.SetHandler(Set, jsonFile);
        }),
    };

    public async Task Delete(string nameId)
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Deleting Subscription nameId={nameId}", nameId);

        Option response = await _client.Delete(nameId, context);
        response.LogStatus(context, "Deleting subscription");
    }

    public async Task Get(string nameId)
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Get Subscription nameId={nameId}", nameId);

        var response = await _client.Get(nameId, context);
        if (response.IsError())
        {
            context.LogError("Cannot get Subscription nameId={nameId}", nameId);
            return;
        }

        var result = response.Return()
            .ToDictionary()
            .Select(x => $" - {x.Key}={x.Value}")
            .Prepend($"Subscription...")
            .Join(Environment.NewLine) + Environment.NewLine;

        context.LogInformation(result);
    }

    public async Task Set(string jsonFile)
    {
        var context = new ScopeContext(_logger);

        var readResult = CmdTools.LoadJson<SubscriptionModel>(jsonFile, SubscriptionModel.Validator, context);
        if (readResult.IsError()) return;

        SubscriptionModel model = readResult.Return();

        Option response = await _client.Set(model, context);
        response.LogStatus(context, "Creating/Updating Subscription, model={model}", model);
    }
}
