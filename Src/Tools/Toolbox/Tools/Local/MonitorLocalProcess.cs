using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;

namespace Toolbox.Tools.Local
{
    public class MonitorLocalProcess
    {
        private const string _restartProcess = "***RESTART***";
        private readonly MonitorStream _monitorStream;
        private readonly LocalProcessBuilder _localProcessBuilder;
        private readonly Func<string, MonitorState?> _monitor;
        private readonly ILogger _logger;
        private readonly MonitorStateScope _monitorStateScope = new MonitorStateScope(MonitorState.Stopped);

        private SubjectState<LocalProcess>? _localProcess;
        private SubjectState<TaskCompletionSource<bool>>? _runningTcs;
        private CancellationTokenRegistration? _tokenRegistration;
        private CancellationToken? _token;

        public MonitorLocalProcess(LocalProcessBuilder localProcessBuilder, Func<string, MonitorState?> monitor, ILogger logger)
        {
            localProcessBuilder.NotNull();
            monitor.NotNull();
            logger.NotNull();

            _localProcessBuilder = new LocalProcessBuilder(localProcessBuilder);
            _monitor = monitor;
            _logger = logger;
            _monitorStream = new MonitorStream(Monitor);
        }

        public Task<bool> Completion => _runningTcs?.SubjectOrDefault?.Task ?? Task.FromResult<bool>(false);

        public Task<bool> Start(CancellationToken token)
        {
            _logger.LogInformation($"{nameof(MonitorLocalProcess)}: Starting the local process monitoring");

            _token ??= token;
            _tokenRegistration ??= token.Register(async () =>
            {
                _logger.LogTrace($"{nameof(MonitorLocalProcess)}: Canceled MonitorLocalProcess");
                await Stop();
            });

            _runningTcs = new TaskCompletionSource<bool>().ToSubjectScope();

            InternalStart();

            return _runningTcs.Subject.Task;
        }

        public async Task Stop()
        {
            _logger.LogInformation($"{nameof(MonitorLocalProcess)}: Stopping local process monitoring");
            _runningTcs?.GetAndClear()?.SetResult(true);
            await InternalStop();
        }

        public void Dispose() => Stop().Wait();

        private void InternalStart()
        {
            _logger.LogTrace($"{nameof(MonitorLocalProcess)}: Internal starting local process");

            _localProcess = new LocalProcessBuilder(_localProcessBuilder)
            {
                CaptureOutput = x => _monitorStream.Post(x),
                OnExit = OnProcessExit
            }
            .Build(_logger)
            .ToSubjectScope();

            _localProcess.Subject.Run((CancellationToken)_token!);
            _monitorStateScope.Set(MonitorState.Started);
        }

        public Task InternalStop()
        {
            _logger.LogTrace($"{nameof(MonitorLocalProcess)}: Internal stopping local process");

            LocalProcess? localProcess = _localProcess?.GetAndClear();
            localProcess?.Stop();

            _monitorStream.NewMessageBlockId();
            return localProcess?.Completion ?? Task.CompletedTask;
        }

        private void OnProcessExit()
        {
            if (!_monitorStateScope.IsRunning || _token?.IsCancellationRequested == true) return;

            _logger.LogTrace($"{nameof(MonitorLocalProcess)}: Internal process exited");
            _monitorStateScope.Set(MonitorState.Stopped);

            _monitorStream.NewMessageBlockId();
            _monitorStream.Post(_restartProcess);
        }

        private async Task Monitor(string lineData)
        {
            if (_monitorStateScope.Current == MonitorState.Failed || _token?.IsCancellationRequested == true) return;

            MonitorState? monitorState = lineData switch
            {
                string s when s == _restartProcess => MonitorState.Restart,

                _ => _monitor(lineData),
            };

            if (monitorState == null) return;

            _logger.LogTrace($"{nameof(MonitorLocalProcess)}:{nameof(Monitor)}: MonitorState={(MonitorState)monitorState}");

            switch (monitorState)
            {
                case MonitorState.Running:
                    _monitorStateScope.Set(MonitorState.Running);
                    _logger.LogInformation($"{_localProcessBuilder.ExecuteFile} process is running");
                    break;

                case MonitorState.Restart:
                    _monitorStateScope.Set(MonitorState.Stopped);
                    await InternalStop();

                    _logger.LogInformation("ML process is restarting");
                    InternalStart();
                    break;

                case MonitorState.Failed:
                    _monitorStateScope.Set(MonitorState.Failed);
                    await InternalStop();

                    "Local monitor process failed, normally retry policy failed"
                        .Action(x => _runningTcs?.GetAndClear()?.SetResult(false))
                        .Action(x => _logger.LogError(x));
                    break;

                default:
                    string msg = $"Unhanded monitor state {monitorState}"
                        .Action(x => _logger.LogError(x))
                        .Action(x => _runningTcs?.GetAndClear()?.SetException(new InvalidOperationException(x)));

                    throw new InvalidOperationException(msg);
            }
        }
    }
}