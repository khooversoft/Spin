using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Contract;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class DumpContract
{
    private readonly ILogger<DumpContract> _logger;
    private readonly ContractClient _client;

    public DumpContract(ContractClient client, ILogger<DumpContract> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task Dump(string contractId, string principalId)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Dumping account ID {contractId}, principalId={principalId}", contractId, principalId);

        var test = new OptionTest()
            .Test(() => IdPatterns.IsContractId(contractId).ToOptionStatus(error: "not valid contractId"))
            .Test(() => IdPatterns.IsPrincipalId(principalId).ToOptionStatus(error: "not valid principalId"));
        if (test.IsError())
        {
            context.Trace().LogError("Error: {error}", test.Error);
            return;
        }

        var query = new ContractQuery
        {
            PrincipalId = principalId,
        };

        var result = await _client.Query(contractId, query, context);
        context.Trace().LogTrace("Dumping contract {contractId}", contractId);
        if (result.IsError()) return;

        IReadOnlyList<DataBlock> list = result.Return();

        foreach (var item in list)
        {
            string line = item.GetConfigurationValues()
                .Select(x => x switch
                {
                    { Key: "JwtSignature" } => new KeyValuePair<string, string>(x.Key, "..."),
                    _ => x,
                })
                .Select(x => $"{x.Key}={x.Value}".Replace("{", "{{").Replace("}", "}}"))
                .Join(Environment.NewLine);

            context.Trace().LogInformation(line);
        }
    }
}
