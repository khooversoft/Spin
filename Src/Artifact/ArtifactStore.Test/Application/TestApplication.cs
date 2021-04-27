using Microsoft.Extensions.Logging;
using System.Threading;

namespace ArtifactStore.Test.Application
{
    internal static class TestApplication
    {
        private static ILoggerFactory? _loggerFactory;
        private static ArtifactTestHost? _host;
        private static object _lock = new object();

        public static ArtifactTestHost GetHost()
        {
            lock (_lock)
            {
                if (_host != null) return _host;

                _host = new ArtifactTestHost(LoggerFactory.Create(x => x.AddDebug()).CreateLogger<ArtifactTestHost>());
                _host.StartApiServer();

                return _host;
            }
        }

        public static void Shutdown() => Interlocked.Exchange(ref _host, null)?.Shutdown();

        public static ILoggerFactory GetLoggerFactory() => _loggerFactory ??= LoggerFactory.Create(x =>
        {
            x.AddConsole();
            x.AddDebug();
        });
    }
}