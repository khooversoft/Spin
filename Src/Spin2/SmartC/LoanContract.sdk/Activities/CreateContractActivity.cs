using LoanContract.sdk.Contract;
using LoanContract.sdk.Models;
using Microsoft.Extensions.Logging;
using SpinClient.sdk;
using SpinCluster.abstraction;
using Toolbox.CommandRouter;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace LoanContract.sdk.Activities;

public class CreateContractActivity : ICommandRoute
{
    private readonly ScheduleWorkClient _scheduleWorkClient;
    private readonly LoanContractManager _manager;
    private readonly ILogger<CreateContractActivity> _logger;

    public CreateContractActivity(ScheduleWorkClient scheduleWorkClient, LoanContractManager manager, ILogger<CreateContractActivity> logger)
    {
        _scheduleWorkClient = scheduleWorkClient.NotNull();
        _manager = manager.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("create", "Create, clear, dump schedule queues").Action(x =>
    {
        var workId = x.AddArgument<string>("workId", "Work Id to retrive details");
        x.SetHandler(Create, workId);
    });

    public async Task Create(string workId)
    {
        var context = new ScopeContext(_logger);
        context.Location().LogInformation("Creating loan contract for workId={workId}", workId);

        var workOption = await _scheduleWorkClient.Get(workId, context);
        if (workOption.IsError())
        {
            context.Location().LogError("[Abort] Cannot get Schedule work detail for workId={workId}", workId);
            return;
        }

        Option resultOption = StatusCode.OK;
        ScheduleWorkModel work = workOption.Return();

        try
        {
            var testResult = new OptionTest()
                .Test(work.Payloads.TryGetObject<LoanAccountDetail>(out var loanAccountDetail, validator: LoanAccountDetail.Validator))
                .Test(work.Payloads.TryGetObject<LoanDetail>(out var loanDetail, validator: LoanDetail.Validator));

            if (testResult.IsError())
            {
                context.Location().LogError("Failed to get required payload from schedule, error={error}", testResult.Error);
                resultOption = testResult.Option;
                return;
            }

            var createResponse = await _manager.Create(loanAccountDetail, context);
            if (createResponse.IsError())
            {
                context.Location().LogStatus(createResponse, "Failed to create loan contract, loanAccountDetail={loanAccountDetail}", loanAccountDetail);
                resultOption = createResponse;
                return;
            }

            var loanDetailResponse = await _manager.SetLoanDetail(loanDetail, context);
            if (loanDetailResponse.IsError())
            {
                context.Location().LogStatus(loanDetailResponse, "Failed to set create loan contract, loanDetail={loanDetail}", loanDetail);
                resultOption = loanDetailResponse;
                return;
            }
        }
        finally
        {
            var response = new RunResultModel
            {
                WorkId = workId,
                StatusCode = resultOption.StatusCode,
                Message = resultOption.StatusCode.IsOk() ? "Created contract" : $"Failed to create contract, error={resultOption.Error}",
            };

            var writeRunResult = await _scheduleWorkClient.AddRunResult(response, context);
            if (writeRunResult.IsError())
            {
                context.Location().LogError("Failed to write 'RunResult' to loan contract, work={work}", workOption.Return());
            }
        }
    }
}
