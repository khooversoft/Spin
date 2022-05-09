using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using Toolbox.Tools.Local;

namespace Toolbox.Tools.Local
{
    public class LocalProcessBuilder
    {
        public LocalProcessBuilder() { }

        public LocalProcessBuilder(LocalProcessBuilder clone)
        {
            clone.NotNull(nameof(clone));

            SuccessExitCode = clone.SuccessExitCode;
            ExecuteFile = clone.ExecuteFile;
            Arguments = clone.Arguments;
            WorkingDirectory = clone.WorkingDirectory;
            CaptureOutput = clone.CaptureOutput;
            OnExit = clone.OnExit;
        }

        public int? SuccessExitCode { get; set; }

        public string? ExecuteFile { get; set; }

        public string? Arguments { get; set; }

        public string? WorkingDirectory { get; set; }

        public Action<string>? CaptureOutput { get; set; }

        public Action? OnExit { get; set; }

        public bool UseShellExecute { get; set; }

        public LocalProcess Build(ILogger logger) => new LocalProcess(this, logger);
    }
}