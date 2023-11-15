using LoanContract.sdk.Activities;
using LoanContract.sdk.Contract;
using LoanContract.sdk.Models;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ScheduleWork;
using Toolbox.CommandRouter;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Loan_smartc_v1.Activitites;

internal class CreateContract : ICommandRoute
{
    private readonly ILogger<CreateContract> _logger;
    private readonly CreateContractActivity _createContractActivity;

    public CreateContract(CreateContractActivity createContractActivity, ILogger<CreateContract> logger)
    {
        _createContractActivity = createContractActivity;
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("create", "Create contract").Action(x =>
    {
        var workId = x.AddOption<string>("--workId", "WorkId to execute");
        x.SetHandler(_createContractActivity.Create, workId);
    });
}
