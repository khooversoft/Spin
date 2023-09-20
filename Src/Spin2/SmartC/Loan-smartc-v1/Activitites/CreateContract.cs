using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Contract;
using Toolbox.Tools;

namespace Loan_smartc_v1.Activitites;

internal class CreateContract
{
    private readonly ContractClient _client;
    private readonly ILogger<CreateContract> _logger;

    public CreateContract(ContractClient client, ILogger<CreateContract> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    //public async Task Create(string jsonFile)
    //{
    //    var context = new ScopeContext(_logger);

    //    if (!File.Exists(jsonFile))
    //    {
    //        context.Trace().LogError("File {file} does not exist", jsonFile);
    //        return;
    //    }
    //}
}
