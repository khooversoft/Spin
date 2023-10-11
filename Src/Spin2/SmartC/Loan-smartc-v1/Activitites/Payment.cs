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

internal class Payment : ICommandRoute
{
    private readonly ScheduleWorkClient _client;
    private readonly LoanContractManager _manager;
    private readonly ILogger<CreateContract> _logger;

    public Payment(ScheduleWorkClient client, LoanContractManager manager, ILogger<CreateContract> logger)
    {
        _client = client.NotNull();
        _manager = manager.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("payment", "Create payment").Action(x =>
    {
        var workId = x.AddOption<string>("--workId", "WorkId to execute");
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
            var extractResult = workOption.Return()
                .Extract(_logger)
                .TryGetObject<LoanPaymentRequest>(out var loanPaymentRequest);

            if (extractResult.Option.IsError())
            {
                resultOption = extractResult.Option;
                return;
            }

            var makePaymentResponse = await _manager.MakePayment(loanPaymentRequest, context);
            if (makePaymentResponse.IsError())
            {
                context.Location().LogStatus(
                    makePaymentResponse,
                    "Failed to create make payment on loan contract, loanPaymentRequest={loanPaymentRequest}",
                    loanPaymentRequest
                    );

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
