using LoanContract.sdk.Contract;
using Microsoft.Extensions.Logging;
using SpinClient.sdk;
using SpinCluster.abstraction;
using Toolbox.CommandRouter;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace LoanContract.sdk.Activities;

public class InterestChargeActivity : ICommandRoute
{
    private readonly ScheduleWorkClient _scheduleWorkClient;
    private readonly LoanContractManager _manager;
    private readonly ILogger<PaymentActivity> _logger;
    private readonly SmartcClient _smartcClient;
    private readonly ScheduleOption _option;

    public InterestChargeActivity(ScheduleOption option, ScheduleWorkClient client, LoanContractManager manager, SmartcClient smartcClient, ILogger<PaymentActivity> logger)
    {
        _option = option.NotNull();
        _scheduleWorkClient = client.NotNull();
        _manager = manager.NotNull();
        _smartcClient = smartcClient.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("interestCharge", "Post interest charges").Action(x =>
    {
        var workId = x.AddOption<string>("--workId", "Work Id to retrive details", isRequired: true);
        x.SetHandler(PostInterestCharge, workId);
    });

    public async Task PostInterestCharge(string workId)
    {
        var context = new ScopeContext(_logger);
        context.Location().LogInformation("Posting interest charages for workId={workId}", workId);

        var workOption = await _scheduleWorkClient.Get(workId, context);
        if (workOption.IsError())
        {
            context.Location().LogError("[Abort] Cannot get Schedule work detail for workId={workId}", workId);
            return;
        }

        ScheduleWorkModel work = workOption.Return();

        var getPayloadOption = work.Payloads.TryGetObject<LoanInterestRequest>(out var loanInterestRequest, validator: LoanInterestRequest.Validator);
        await _scheduleWorkClient.AddRunResult(workId, getPayloadOption.ToOptionStatus(), "Required payload", context);
        if (getPayloadOption.IsError())
        {
            await _scheduleWorkClient.CompletedWork(_option.AgentId, work.WorkId, getPayloadOption.ToOptionStatus(), "Get required payload", context);
            return;
        }

        var applyInterestChargeResult = await _manager.PostInterestCharge(loanInterestRequest, context);
        await _scheduleWorkClient.AddRunResult(workId, applyInterestChargeResult, "Post interest charge", context);
        if (applyInterestChargeResult.IsError())
        {
            await _scheduleWorkClient.CompletedWork(_option.AgentId, work.WorkId, applyInterestChargeResult, "Post interest charge", context);
            return;
        }

        await _scheduleWorkClient.CompletedWork(_option.AgentId, work.WorkId, StatusCode.OK, "Completed", context);
    }
}
