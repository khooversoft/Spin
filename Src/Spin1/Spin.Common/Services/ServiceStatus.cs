using Microsoft.Extensions.Logging;
using Spin.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spin.Common.Services;

public class ServiceStatus : IServiceStatus
{
    private readonly object _lock = new();

    public ServiceStatusLevel Level { get; private set; }

    public string? Message { get; private set; }

    public ServiceStatus SetStatus(ServiceStatusLevel serviceStatusLevel, string? message = null)
    {
        message ??= serviceStatusLevel switch
        {
            ServiceStatusLevel.Ready => ServiceStatusLevel.Ready.ToString(),
            ServiceStatusLevel.Running => ServiceStatusLevel.Running.ToString(),

            _ => ServiceStatusLevel.Unknown.ToString(),
        };

        lock (_lock)
        {
            Level = serviceStatusLevel;
            Message = message;
        }

        return this;
    }
}

public class ServiceStatus<T> : ServiceStatus, IServiceStatus<T>
{
}

