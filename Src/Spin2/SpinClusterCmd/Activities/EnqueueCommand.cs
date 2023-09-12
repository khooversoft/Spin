using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Smartc;
using SpinClusterCmd.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class EnqueueCommand
{
    private readonly ScheduleClient _client;
    private readonly ILogger<EnqueueCommand> _logger;

    public EnqueueCommand(ScheduleClient client, ILogger<EnqueueCommand> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task Enqueue(string jsonFile)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Processing file {file}", jsonFile);

        var readResult = CmdTools.LoadJson<EnqueueOption>(jsonFile, EnqueueOption.Validator, context);
        if (readResult.IsError()) return;

        EnqueueOption option = readResult.Return();

        context.Trace().LogInformation("Enqueuing command, enqueueOption={enqueueOption}", option);

        if (option.Clear && option.ClearPrincipalId.IsNotEmpty())
        {
            context.Trace().LogInformation("Clearing queue");

            var clearOption = await _client
                .Clear(option.ClearPrincipalId, context)
                .LogResult(context.Location());

            if (clearOption.IsError()) return;
        }

        var work = new ScheduleWorkModel
        {
            SmartcId = option.SmartcId,
            SourceId = option.SourceId,
            Command = option.Command,
        };

        var queueResult = await _client.EnqueueSchedule(work, context).LogResult(context.Location());
        if( queueResult.IsError()) return;

        context.Trace().LogInformation("Queued command, workId={workId}", work.WorkId);
    }

    private record EnqueueOption
    {
        public string SmartcId { get; init; } = null!;
        public string SourceId { get; init; } = null!;
        public string Command { get; init; } = null!;
        public string? ClearPrincipalId { get; init; }
        public bool Clear { get; init; }

        public static IValidator<EnqueueOption> Validator { get; } = new Validator<EnqueueOption>()
            .RuleFor(x => x.SmartcId).ValidResourceId(ResourceType.DomainOwned, "smartc")
            .RuleFor(x => x.SourceId).NotEmpty()
            .RuleFor(x => x.Command).NotEmpty()
            .RuleFor(x => x.ClearPrincipalId).ValidResourceIdOption(ResourceType.Principal)
            .Build();
    }
}
