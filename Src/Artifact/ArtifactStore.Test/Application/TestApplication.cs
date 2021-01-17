using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArtifactStore.Test.Application
{
    internal static class TestApplication
    {
        private static ILoggerFactory? _loggerFactory;
        private static TestWebsiteHost? _host;
        private static object _lock = new object();

        public static TestWebsiteHost GetHost()
        {
            lock (_lock)
            {
                if (_host != null) return _host;

                _host = new TestWebsiteHost(LoggerFactory.Create(x => x.AddDebug()).CreateLogger<TestWebsiteHost>());
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
