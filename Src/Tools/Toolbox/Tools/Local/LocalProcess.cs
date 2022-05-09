using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Toolbox.Tools.Local
{
    /// <summary>
    /// Is responsible for starting, stopping, and timing out (if specified) of
    /// a local process.
    ///
    /// The "Task of LocalProcess" returned from Run will complete when the process has been completed
    /// or shutdown.
    /// </summary>
    public class LocalProcess : IDisposable
    {
        private readonly ILogger _logger;
        private readonly int? _successExitCode;
        private readonly string? _executeFile;
        private readonly string? _arguments;
        private readonly string? _workingDirectory;
        private readonly bool _useShellExecute;
        private readonly Action<string>? _captureOutput;
        private readonly SubjectScope<Action>? _onExitNotify;

        private CancellationTokenRegistration? _tokenRegistration;
        private SubjectScope<TaskCompletionSource<LocalProcess>>? _processCompletedTcs;
        private SubjectScope<Process>? _process;

        public LocalProcess(LocalProcessBuilder builder, ILogger logger)
        {
            builder.NotNull(nameof(builder));
            logger.NotNull(nameof(logger));

            _logger = logger;
            _successExitCode = builder.SuccessExitCode;
            _executeFile = builder.ExecuteFile;
            _arguments = builder.Arguments;
            _workingDirectory = builder.WorkingDirectory;
            _captureOutput = builder.CaptureOutput;
            _useShellExecute = builder.UseShellExecute;

            _onExitNotify = builder.OnExit != null ? new SubjectScope<Action>(builder.OnExit) : null;
        }

        public int? ExitCode { get; private set; }

        public Process? Process => _process?.SubjectOrDefault;

        public Task<LocalProcess> Completion => _processCompletedTcs?.SubjectOrDefault?.Task ?? Task.FromResult<LocalProcess>(null!);

        public bool IsRunning => Process?.HasExited == false && _processCompletedTcs != null;

        /// <summary>
        /// Start and forget
        /// </summary>
        public void Start()
        {
            if (_processCompletedTcs != null) throw new InvalidOperationException("Local process is running");

            _process = BuildProcess();

            bool started;
            try
            {
                started = _process.Subject.Start();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                _logger.LogError(ex, $"{nameof(LocalProcess)}: Start failed");
                started = false;
            }

            if (!started)
            {
                string msg = $"{nameof(LocalProcess)}: Process failed to start, File={_executeFile}";
                _logger.LogError(msg);
                throw new InvalidOperationException(msg);
            }
        }

        /// <summary>
        /// Run the process, Task is completed when the process has exited
        /// </summary>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>this</returns>
        public Task<LocalProcess> Run(CancellationToken cancellationToken)
        {
            if (_processCompletedTcs != null) throw new InvalidOperationException("Local process is running");

            _process = BuildProcess();
            _processCompletedTcs = new SubjectScope<TaskCompletionSource<LocalProcess>>(new TaskCompletionSource<LocalProcess>());

            _tokenRegistration ??= cancellationToken.Register(() =>
            {
                _logger.LogTrace($"{nameof(LocalProcess)}: Canceled local process, File={_executeFile}");
                Stop();
            });

            _logger.LogTrace($"{nameof(LocalProcess)}: Starting local process, File={_executeFile}, Arguments={_arguments}, WorkingDirectory={_workingDirectory}");

            // Start process
            bool started;
            try
            {
                started = _process.Subject.Start();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                _logger.LogError(ex, $"{nameof(LocalProcess)}: Start failed");
                started = false;
            }

            if (!started)
            {
                string msg = $"{nameof(LocalProcess)}: Process failed to start, File={_executeFile}";
                _logger.LogError(msg);
                throw new InvalidOperationException(msg);
            }

            _process.Subject.BeginOutputReadLine();
            _process.Subject.BeginErrorReadLine();

            return _processCompletedTcs.Subject.Task;
        }

        /// <summary>
        /// Stop any running process if running and completes the "run" task.
        /// </summary>
        public void Stop()
        {
            _logger.LogInformation("Stopping {executeFile}", _executeFile);

            TokenUnregister();

            try { Process?.Kill(); } catch { }
            try { Process?.Close(); } catch { }

            _process?.GetAndClear()?.Dispose();
            _processCompletedTcs?.GetAndClear()?.SetResult(this);
        }

        public void Dispose() => Stop();

        private void OnProcessExit(object? sender, EventArgs args)
        {
            if (_process == null) throw new ArgumentNullException(nameof(Process));
            if (_processCompletedTcs == null) return;

            TokenUnregister();
            ExitCode = _process.Subject.ExitCode;

            _logger.LogInformation("Process has exit, ExitCode={ExitCode}", ExitCode);

            switch (_process.Subject.ExitCode)
            {
                // Process has been forced closed
                case int v when v == -1:
                default:
                    _process.Subject.Close();
                    _process.Subject.Dispose();
                    _processCompletedTcs.GetAndClear()?.SetResult(this);
                    break;

                case int v when v != (_successExitCode ?? 0):
                    string msg = $"{nameof(LocalProcess)}: Exit code: {ExitCode} does not match required exit code {_successExitCode}";
                    _logger.LogError(msg);
                    _processCompletedTcs.GetAndClear()?.SetException(new ArgumentException(msg));
                    break;
            }

            _onExitNotify?.GetAndClear()?.Invoke();
        }

        private void LogOutput(string? data)
        {
            if (data == null) return;

            string message = $"LocalProcess: {data}";
            _logger.LogInformation(message);

            _captureOutput?.Invoke(message);
        }

        private SubjectScope<Process> BuildProcess()
        {
            var process = new Process()
            {
                EnableRaisingEvents = true,

                StartInfo = new ProcessStartInfo
                {
                    FileName = _executeFile ?? string.Empty,
                    Arguments = _arguments ?? string.Empty,
                    WorkingDirectory = _workingDirectory ?? string.Empty,
                    UseShellExecute = _useShellExecute,
                    CreateNoWindow = true,
                    RedirectStandardOutput = _useShellExecute == false,
                    RedirectStandardError = _useShellExecute == false,
                }
            };

            process.OutputDataReceived += (s, e) => LogOutput(e.Data);
            process.ErrorDataReceived += (s, e) => LogOutput(e.Data);
            process.Exited += OnProcessExit;

            return new SubjectScope<Process>(process);
        }

        private void TokenUnregister()
        {
            _tokenRegistration?.Dispose();
            _tokenRegistration = null;
        }
    }
}