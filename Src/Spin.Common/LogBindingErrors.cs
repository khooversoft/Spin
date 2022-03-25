using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Spin.Common;

public class LogBindingErrors
{
    public ILogger? Logger { get; set; }

    public void ConfigureModelBindingExceptionHandling(IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                ValidationProblemDetails? error = actionContext.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .Select(e => new ValidationProblemDetails(actionContext.ModelState))
                    .FirstOrDefault();

                Logger
                    .VerifyNotNull("Logger not set, execute: app.UseLogBindingErrors()")
                    .LogError("{requestPath} received invalid message format: {errors}",
                        actionContext.HttpContext.Request.Path.Value,
                        error?.Errors?.Select(x => $"{x.Key}={x.Value.Join(", ")}")
                        );

                return new BadRequestObjectResult(error);
            };
        });
    }
}
