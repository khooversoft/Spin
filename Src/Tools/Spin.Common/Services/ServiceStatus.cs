using Microsoft.Extensions.Logging;
using Spin.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spin.Common.Services
{
    public class ServiceStatus : IServiceStatus
    {
        private readonly ILogger<ServiceStatus> _logger;
        private readonly object _lock = new object();

        public ServiceStatus(ILogger<ServiceStatus> logger)
        {
            _logger = logger;
        }

        public ServiceStatusLevel Level { get; private set; }

        public string? Message { get; private set; }

        public void SetStatus(ServiceStatusLevel serviceStatusLevel, string? message)
        {
            lock (_lock)
            {
                Level = serviceStatusLevel;
                Message = message;
            }
        }
    }
}
