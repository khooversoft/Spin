using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Tools.Local
{
    public static class Extensions
    {
        public static MonitorLocalProcess Build(this LocalProcessBuilder builder, Func<string, MonitorState?> monitor, ILogger logger) => new MonitorLocalProcess(builder, monitor, logger);
    }
}
