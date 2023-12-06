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
    private readonly ScheduleOption _option;

    public CreateContractActivity(ScheduleOption option, ScheduleWorkClient scheduleWorkClient, LoanContractManager manager, ILogger<CreateContractActivity> logger)
    {
        _scheduleWorkClient = scheduleWorkClient.NotNull();
        _manager = manager.NotNull();
        _option = option.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("create", "Create, clear, dump schedule queues").Action(x =>
    {
        var workId = x.AddOption<string>("--workId", "Work Id to retrive details", isRequired: true);
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

        ScheduleWorkModel work = workOption.Return();

        var testResult = new OptionTest()
            .Test(work.Payloads.TryGetObject<LoanAccountDetail>(out var loanAccountDetail, validator: LoanAccountDetail.Validator))
            .Test(work.Payloads.TryGetObject<LoanDetail>(out var loanDetail, validator: LoanDetail.Validator));

        await _scheduleWorkClient.AddRunResult(workId, testResult, "Required payload", context);
        if (testResult.IsError())
        {
            await _scheduleWorkClient.CompletedWork(_option.AgentId, work.WorkId, testResult, "Get required payload", context);
            return;
        }

        var createResponse = await _manager.Create(loanAccountDetail, context);
        await _scheduleWorkClient.AddRunResult(workId, testResult, "Create account", context);
        if (createResponse.IsError())
        {
            await _scheduleWorkClient.CompletedWork(_option.AgentId, work.WorkId, createResponse, "Create account", context);
            return;
        }

        var loanDetailResponse = await _manager.SetLoanDetail(loanDetail, context);
        await _scheduleWorkClient.AddRunResult(workId, testResult, "Set loan detail", context);
        if (loanDetailResponse.IsError())
        {
            await _scheduleWorkClient.CompletedWork(_option.AgentId, work.WorkId, createResponse, "Set loan detail", context);
            return;
        }

        await _scheduleWorkClient.CompletedWork(_option.AgentId, work.WorkId, StatusCode.OK, "Completed", context);
    }
}
