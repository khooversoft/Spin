using ArtifactStore.Test.Application;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Identity.Test.Application
{
    internal static class TestApplication
    {
        private static ILoggerFactory? _loggerFactory;
        private static ArtifactTestHost? _artificatHost;
        private static IdentityTestHost? _identityHost;
        private static object _lock = new object();

        public static IdentityTestHost GetHost()
        {
            lock (_lock)
            {
                if (_identityHost != null) return _identityHost;

                _artificatHost = new ArtifactTestHost(LoggerFactory.Create(x => x.AddDebug()).CreateLogger<ArtifactTestHost>());
                _artificatHost.StartApiServer();

                _identityHost = new IdentityTestHost(LoggerFactory.Create(x => x.AddDebug()).CreateLogger<IdentityTestHost>());
                _identityHost.StartApiServer(_artificatHost.Client);

                return _identityHost;
            }
        }

        public static void Shutdown() => Interlocked.Exchange(ref _identityHost, null)?.Shutdown();

        public static ILoggerFactory GetLoggerFactory() => _loggerFactory ??= LoggerFactory.Create(x =>
        {
            x.AddConsole();
            x.AddDebug();
        });
    }
}