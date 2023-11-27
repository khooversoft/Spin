using LoanContract.sdk.Activities;
using Microsoft.Extensions.Logging;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Loan_smartc_v1.Activitites;

internal class Payment : ICommandRoute
{
    private readonly ILogger<Payment> _logger;
    private readonly PaymentActivity _paymentActivity;

    public Payment(PaymentActivity paymentActivity, ILogger<Payment> logger)
    {
        _paymentActivity = paymentActivity.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("payment", "Create payment").Action(x =>
    {
        var workId = x.AddOption<string>("--workId", "WorkId to execute");
        x.SetHandler(_paymentActivity.MakePayment, workId);
    });
}
