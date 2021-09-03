using Spin.Common.Model;

namespace Spin.Common.Services
{
    public interface IServiceStatus
    {
        ServiceStatusLevel Level { get; }
        string? Message { get; }

        ServiceStatus SetStatus(ServiceStatusLevel serviceStatusLevel, string? message);
    }
}