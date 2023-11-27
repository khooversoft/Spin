using LoanContract.sdk.Contract;
using LoanContract.sdk.Models;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Scheduler;
using SpinCluster.sdk.Actors.ScheduleWork;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace LoanContract.sdk.Client;

public class LoanSchedulerClient
{
    private readonly SchedulerClient _schedulerClient;
    private readonly ILogger<LoanSchedulerClient> _logger;
    private readonly LoanSchedulerContext _loanSchedulerContext;

    public LoanSchedulerClient(LoanSchedulerContext loanSchedulerContext, SchedulerClient schedulerClient, ILogger<LoanSchedulerClient> logger)
    {
        _loanSchedulerContext = loanSchedulerContext.NotNull();
        _schedulerClient = schedulerClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> CreateContract(LoanAccountDetail loanAccountDetail, LoanDetail loanDetail, ScopeContext context)
    {
        loanAccountDetail.NotNull();
        loanDetail.NotNull();

        var createRequest = new ScheduleCreateModel
        {
            SmartcId = _loanSchedulerContext.SmartcId,
            SchedulerId = _loanSchedulerContext.SchedulerId,
            PrincipalId = _loanSchedulerContext.PrincipalId,
            SourceId = _loanSchedulerContext.SourceId,
            Command = "create",
            Payloads = new DataObjectSet().Set(loanAccountDetail).Set(loanDetail),
        };

        var queueResult = await _schedulerClient.CreateSchedule(createRequest, context);
        return queueResult;
    }
}
