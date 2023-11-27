using LoanContract.sdk.Contract;
using LoanContract.sdk.Models;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ScheduleWork;
using Toolbox.CommandRouter;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace LoanContract.sdk.Activities;

public class PaymentActivity : ICommandRoute
{
    private readonly ScheduleWorkClient _client;
    private readonly LoanContractManager _manager;
    private readonly ILogger<PaymentActivity> _logger;

    public PaymentActivity(ScheduleWorkClient client, LoanContractManager manager, ILogger<PaymentActivity> logger)
    {
        _client = client.NotNull();
        _manager = manager.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("payment", "Make payment").Action(x =>
    {
        var workId = x.AddArgument<string>("workId", "Work Id of schedule");
        x.SetHandler(MakePayment, workId);
    });

    public async Task MakePayment(string workId)
    {
        var context = new ScopeContext(_logger);
        context.Location().LogInformation("Creating loan contract for workId={workId}", workId);

        var workOption = await _client.Get(workId, context);
        if (workOption.IsError())
        {
            context.Location().LogError("[Abort] Cannot get Schedule work detail for workId={workId}", workId);
            return;
        }

        Option resultOption = StatusCode.OK;

        try
        {
            var loanPaymentRequestOption = workOption.Return().Payloads.TryGetObject<LoanPaymentRequest>(out var loanPaymentRequest, validator: LoanPaymentRequest.Validator);
            if (loanPaymentRequestOption.IsError())
            {
                resultOption = loanPaymentRequestOption.ToOptionStatus();
                return;
            }

            var makePaymentResponse = await _manager.MakePayment(loanPaymentRequest, context);
            if (makePaymentResponse.IsError())
            {
                context.Location().LogStatus(makePaymentResponse, "Failed to create make payment on loan contract, loanPaymentRequest={loanPaymentRequest}", loanPaymentRequest);
                resultOption = makePaymentResponse;
                return;
            }
        }
        finally
        {
            var response = new RunResultModel
            {
                WorkId = workId,
                StatusCode = resultOption.StatusCode,
                Message = resultOption.StatusCode.IsOk() ? "Created contract" : $"Failed to post payment to contract, error={resultOption.Error}",
            };

            var writeRunResult = await _client.AddRunResult(response, context);
            if (writeRunResult.IsError())
            {
                context.Location().LogError("Failed to write 'RunResult' to loan contract for payment, work={work}", workOption.Return());
            }
        }
    }

}
