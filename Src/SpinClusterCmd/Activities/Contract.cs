﻿using Microsoft.Extensions.Logging;
using SpinClient.sdk;
using SpinCluster.abstraction;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class Contract : ICommandRoute
{
    private readonly ILogger<Contract> _logger;
    private readonly ContractClient _client;

    public Contract(ContractClient client, ILogger<Contract> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("contract", "Manage contracts")
    {

        new CommandSymbol("delete", "Delete").Action(x =>
        {
            var contractId = x.AddArgument<string>("contractId", "Contract ID to dump");
            var principalId = x.AddArgument<string>("principalId", "Principal ID (ex. user@domain.com) that has rights to query contract");

            x.SetHandler(Delete, contractId, principalId);
        }),

        new CommandSymbol("dump", "Dump contract").Action(x =>
        {
            var contractId = x.AddArgument<string>("contractId", "Contract ID to dump");
            var principalId = x.AddArgument<string>("principalId", "Principal ID (ex. user@domain.com) that has rights to query contract");

            x.SetHandler(Dump, contractId, principalId);
        }),
    };

    public async Task Delete(string contractId, string principalId)
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Dumping account ID {contractId}, principalId={principalId}", contractId, principalId);

        var response = await _client.Delete(contractId, context);
        response.LogOnError(context, "Delete failed");
        return;
    }

    public async Task Dump(string contractId, string principalId)
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Dumping account ID {contractId}, principalId={principalId}", contractId, principalId);

        var query = new ContractQuery
        {
            PrincipalId = principalId,
        };

        Option<ContractQueryResponse> result = await _client.Query(contractId, query, context);
        context.LogTrace("Dumping contract {contractId}", contractId);
        result.LogOnError(context, "Query failed to contract failed");

        ContractQueryResponse response = result.Return();

        foreach (var item in response.Items)
        {
            string line = item.ToDictionary()
                .Select(x => x switch
                {
                    { Key: "JwtSignature" } => new KeyValuePair<string, string>(x.Key, "..."),
                    _ => x,
                })
                .Select(x => $"{x.Key}={x.Value}".Replace("{", "{{").Replace("}", "}}"))
                .Join(Environment.NewLine);

            context.LogInformation(line);
        }
    }
}
