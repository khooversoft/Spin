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

public class InterestChargeActivity : ICommandRoute
{
    private readonly ScheduleWorkClient _scheduleWorkClient;
    private readonly LoanContractManager _manager;
    private readonly ILogger<PaymentActivity> _logger;
    private readonly SmartcClient _smartcClient;

    public InterestChargeActivity(ScheduleWorkClient client, LoanContractManager manager, SmartcClient smartcClient, ILogger<PaymentActivity> logger)
    {
        _scheduleWorkClient = client.NotNull();
        _manager = manager.NotNull();
        _smartcClient = smartcClient.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("interestCharge", "Post interest charges").Action(x =>
    {
        var workId = x.AddArgument<string>("workId", "Work Id of schedule");
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

        Option resultOption = StatusCode.OK;
        ScheduleWorkModel work = workOption.Return();

        try
        {
            var getPayloadOption = work.Payloads.TryGetObject<LoanPaymentRequest>(out var loanPaymentRequest, validator: LoanPaymentRequest.Validator);
            if (getPayloadOption.IsError())
            {
                context.Location().LogError("Failed to get required payload 'LoanPaymentRequest' from schedule, error={error}", getPayloadOption.Error);
                resultOption = getPayloadOption.ToOptionStatus();
                return;
            }

            var applyInterestChargeResult = await _manager.PostInterestCharge(loanPaymentRequest, context);
            if (applyInterestChargeResult.IsError())
            {
                context.Location().LogError("Apply interset charge failed, error={error}", applyInterestChargeResult.Error);
                resultOption = applyInterestChargeResult;
                return;
            }
        }
        finally
        {
            var response = new RunResultModel
            {
                WorkId = workId,
                StatusCode = resultOption.StatusCode,
                Message = resultOption.StatusCode.IsOk() ? "Posted interest charages" : $"Failed to post interest charages, error={resultOption.Error}",
            };

            var writeRunResult = await _scheduleWorkClient.AddRunResult(response, context);
            if (writeRunResult.IsError())
            {
                context.Location().LogError("Failed to write 'RunResult' to loan contract, work={work}", workOption.Return());
            }
        }
    }
}
