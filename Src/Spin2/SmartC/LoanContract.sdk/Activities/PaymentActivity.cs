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

public class PaymentActivity : ICommandRoute
{
    private readonly ScheduleWorkClient _scheduleWorkClient;
    private readonly LoanContractManager _manager;
    private readonly ILogger<PaymentActivity> _logger;
    private readonly ScheduleOption _option;

    public PaymentActivity(ScheduleOption option, ScheduleWorkClient scheduleWorkClient, LoanContractManager manager, ILogger<PaymentActivity> logger)
    {
        _option = option.NotNull();
        _scheduleWorkClient = scheduleWorkClient.NotNull();
        _manager = manager.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("payment", "Make payment").Action(x =>
    {
        var workId = x.AddOption<string>("--workId", "Work Id to retrive details", isRequired: true);
        x.SetHandler(MakePayment, workId);
    });

    public async Task MakePayment(string workId)
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

        var loanPaymentRequestOption = workOption.Return().Payloads.TryGetObject<LoanPaymentRequest>(out var loanPaymentRequest, validator: LoanPaymentRequest.Validator);
        await _scheduleWorkClient.AddRunResult(workId, loanPaymentRequestOption.ToOptionStatus(), "", context);
        if (loanPaymentRequestOption.IsError())
        {
            await _scheduleWorkClient.CompletedWork(_option.AgentId, work.WorkId, loanPaymentRequestOption.ToOptionStatus(), "Required payload", context);
            return;
        }

        var makePaymentResponse = await _manager.MakePayment(loanPaymentRequest, context);
        await _scheduleWorkClient.AddRunResult(workId, makePaymentResponse, "Make payment", context);
        if (makePaymentResponse.IsError())
        {
            await _scheduleWorkClient.CompletedWork(_option.AgentId, work.WorkId, makePaymentResponse, "Make payment", context);
            return;
        }

        await _scheduleWorkClient.CompletedWork(_option.AgentId, work.WorkId, StatusCode.OK, "Completed", context);
    }
}
