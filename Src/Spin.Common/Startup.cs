using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spin.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Logging;
using Toolbox.Tools;

namespace Spin.Common;

public static class Startup
{
    public static IServiceCollection ConfigurePingService(this IServiceCollection service, ILoggingBuilder logging)
    {
        service.VerifyNotNull(nameof(service));

        service.AddSingleton<IServiceStatus, ServiceStatus>();
        logging.AddLoggerBuffer();

        return service;
    }
}
