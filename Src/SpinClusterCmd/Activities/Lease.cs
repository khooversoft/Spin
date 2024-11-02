using Microsoft.Extensions.Logging;
using SpinClient.sdk;
using SpinCluster.abstraction;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class Lease : ICommandRoute
{
    private readonly LeaseClient _client;
    private readonly ILogger<Lease> _logger;

    public Lease(LeaseClient client, ILogger<Lease> logger)
    {
        _client = client;
        _logger = logger;
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("lease", "Cluster lease resource management")
    {
        new CommandSymbol("get", "Get lease details").Action(x =>
        {
            var leaseKey = x.AddArgument<string>("leaseKey", "Lease key to get");
            x.SetHandler(Get, leaseKey);
        }),
        new CommandSymbol("isValid", "Is lease valid").Action(x =>
        {
            var leaseKey = x.AddArgument<string>("leaseKey", "Lease key to get");

            x.SetHandler(IsValid, leaseKey);
        }),
        new CommandSymbol("list", "List active leases").Action(x =>
        {
            x.SetHandler(List);
        }),
        new CommandSymbol("release", "Release an active lease").Action(x =>
        {
            var leaseKey = x.AddArgument<string>("leaseKey", "Lease key to get");
            x.SetHandler(Release, leaseKey);
        }),
    };

    public async Task Get(string leaseKey)
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Get lease details for leaseKey={leaseKey}", leaseKey);

        Option<LeaseData> response = await _client.Get(leaseKey, context);
        if (response.IsError())
        {
            context.LogError("Failed to get lease details for leaseKey={leaseKey}", leaseKey);
            return;
        }

        string result = response.Return()
            .ToDictionary()
            .Select(x => $" - {x.Key}={x.Value}")
            .Prepend($"Lease detail...")
            .Join(Environment.NewLine) + Environment.NewLine;

        context.LogInformation(result);
    }

    public async Task IsValid(string leaseKey)
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Checking is lease is valid for leaseKey={leaseKey}", leaseKey);

        Option response = await _client.IsValid(leaseKey, context);
        response.LogStatus(context, "Is lease valid");
    }

    public async Task List()
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Listing valid leases");

        Option<IReadOnlyList<LeaseData>> response = await _client.List(QueryParameter.Parse("**/*"), context);
        if (response.IsError())
        {
            context.LogError("Failed to get lease details");
            return;
        }

        foreach (var item in response.Return())
        {
            string result = item
                .ToDictionary()
                .Select(x => $" - {x.Key}={x.Value}")
                .Prepend($"Lease detail...")
                .Join(Environment.NewLine) + Environment.NewLine;

            context.LogInformation(result);
        }
    }

    public async Task Release(string leaseKey)
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Releasing lease for leaseKey={leaseKey}", leaseKey);

        Option response = await _client.Release(leaseKey, context);
        response.LogStatus(context, "Release lease");
    }
}
