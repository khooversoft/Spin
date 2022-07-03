using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;

namespace Toolbox.Work;

public abstract class Activity
{
    public abstract Task<string> Run(OrchestrationContext context, string input);
}


public abstract class Activity<TResult, TInput> : Activity
{
    protected readonly ILogger _logger;

    public Activity(ILogger logger)
    {
        _logger = logger.NotNull();
    }

    public abstract Task<TResult> Execute(OrchestrationContext context, TInput input);

    public override async Task<string> Run(OrchestrationContext context, string input)
    {
        using var ls = _logger.LogEntryExit();
        _logger.LogTrace("Input={input}", input);

        try
        {
            TInput inputParameter = input.ToObject<TInput>().NotNull();

            TResult result = await Execute(context, inputParameter);
            string resultJson = result.ToJson();

            _logger.LogTrace("Result={result}", resultJson);
            return resultJson;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed work activity, input={input}", input);
            throw;
        }
    }
}
