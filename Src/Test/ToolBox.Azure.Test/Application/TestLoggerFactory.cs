using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ToolBox.Azure.Test.Application
{
    public class TestLoggerFactory : ILoggerFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public TestLoggerFactory()
        {
            _loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
        }

        public void AddProvider(ILoggerProvider provider) => _loggerFactory.AddProvider(provider);

        public ILogger CreateLogger(string categoryName) => _loggerFactory.CreateLogger(categoryName);

        public void Dispose() => _loggerFactory.Dispose();
    }
}
