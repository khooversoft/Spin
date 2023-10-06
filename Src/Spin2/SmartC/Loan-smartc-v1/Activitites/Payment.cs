using LoanContract.sdk.Contract;
using LoanContract.sdk.Models;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ScheduleWork;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace Loan_smartc_v1.Activitites;

internal class Payment
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

        ScheduleWorkModel workSchedule = workOption.Return();

        if (!workSchedule.Payloads.TryGetObject<LoanPaymentRequest>(out var loanPaymentRequest))
        {
            context.Location().LogError("[Abort] LoanAccountDetail not in payload");
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
        }

        var response = new RunResultModel
        {
            WorkId = workId,
            StatusCode = makePaymentResponse.StatusCode,
            Message = makePaymentResponse.Error,
        };

        var writeRunResult = await _client.AddRunResult(response, context);
        if (writeRunResult.IsError())
        {
            context.Location().LogError("Failed to write 'RunResult' to loan contract, loanPaymentRequest={loanPaymentRequest}", loanPaymentRequest);
            return;
        }
    }
}
