using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spin.Common.Services;
using Toolbox.Abstractions.Tools;
using Toolbox.Logging;

namespace Spin.Common;

public static class Startup
{
    public static IServiceCollection ConfigurePingService(this IServiceCollection service, ILoggingBuilder logging)
    {
        service.NotNull();

        service.AddSingleton<IServiceStatus, ServiceStatus>();
        logging.AddLoggerBuffer();

        return service;
    }

    public static IServiceCollection ConfigureLogBindingErrors(this IServiceCollection service)
    {
        var logBindingErrors = new LogBindingErrors();
        service.AddSingleton(logBindingErrors);

        logBindingErrors.ConfigureModelBindingExceptionHandling(service);

        return service;
    }

    public static IApplicationBuilder UseLogBindingErrors(this IApplicationBuilder app)
    {
        ILogger logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger<LogBindingErrors>();
        app.ApplicationServices.GetRequiredService<LogBindingErrors>().Logger = logger;

        return app;
    }
}
