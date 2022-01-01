using Microsoft.Extensions.Logging;
using System.Threading;

namespace Artifact.Test.Application
{
    internal static class TestApplication
    {
        private static ILoggerFactory? _loggerFactory;
        private static ArtificatTestHost? _host;
        private static object _lock = new object();

        public static ArtificatTestHost GetHost()
        {
            lock (_lock)
            {
                if (_host != null) return _host;

                _host = new ArtificatTestHost(LoggerFactory.Create(x => x.AddDebug()).CreateLogger<ArtificatTestHost>());
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