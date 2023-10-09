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
    private readonly ScheduleWorkClient _client;
    private readonly LoanContractManager _manager;
    private readonly ILogger<CreateContract> _logger;

    public CreateContract(ScheduleWorkClient client, LoanContractManager manager, ILogger<CreateContract> logger)
    {
        _client = client.NotNull();
        _manager = manager.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("create", "Create contract").Action(x =>
    {
        var workId = x.AddOption<string>("--workId", "WorkId to execute");
        x.SetHandler(Create, workId);
    });

    public async Task Create(string workId)
    {
        var context = new ScopeContext(_logger);
        context.Location().LogInformation("Creating loan contract for workId={workId}", workId);

        var workOption = await _client.Get(workId, context);
        if (workOption.IsError())
        {
            context.Location().LogError("[Abort] Cannot get Schedule work detail for workId={workId}", workId);
            return;
        }

        ScheduleWorkModel workSchedule = workOption.Return();

        var extractResult = workOption.Return()
            .Extract(_logger)
            .TryGetObject<LoanAccountDetail>(out var loanAccountDetail)
            .TryGetObject<LoanDetail>(out var loanDetail);

        if (extractResult.Option.IsError()) return;

        var createResponse = await _manager.Create(loanAccountDetail, context);
        if (createResponse.IsError())
        {
            context.Location().LogStatus(createResponse, "Failed to create loan contract, loanAccountDetail={loanAccountDetail}", loanAccountDetail);
        }

        var loanDetailResponse = await _manager.SetLoanDetail(loanDetail, context);
        if (loanDetailResponse.IsError())
        {
            context.Location().LogStatus(loanDetailResponse, "Failed to set create loan contract, loanDetail={loanDetail}", loanDetail);
        }

        var response = new RunResultModel
        {
            WorkId = workId,
            StatusCode = createResponse.StatusCode,
            Message = createResponse.Error,
        };

        var writeRunResult = await _client.AddRunResult(response, context);
        if (writeRunResult.IsError())
        {
            context.Location().LogError("Failed to write 'RunResult' to loan contract, loanAccountDetail={loanAccountDetail}", loanAccountDetail);
            return;
        }
    }
}
