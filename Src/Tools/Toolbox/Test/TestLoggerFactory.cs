using Microsoft.Extensions.Logging;

namespace Toolbox.Test
{
    public class TestLoggerFactory : ILoggerFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public TestLoggerFactory() => _loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());

        public void AddProvider(ILoggerProvider provider) => _loggerFactory.AddProvider(provider);

        public ILogger CreateLogger(string categoryName) => _loggerFactory.CreateLogger(categoryName);

        public void Dispose() => _loggerFactory.Dispose();
    }
}
