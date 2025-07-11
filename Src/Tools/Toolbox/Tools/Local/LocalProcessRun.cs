using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;

public class LocalProcessRun
{
    private readonly Action<string>? _captureOutput;
    private Process _process;
    private TaskCompletionSource<Option<int>> _tcs = null!;
    private CancellationTokenSource _tokenSource = null!;
    private int _hasProcessedExit;
    private int _isRunning;
    private ScopeContext _context;

    public LocalProcessRun(Process process, Action<string>? captureOutput)
    {
        _process = process.NotNull();
        _captureOutput = captureOutput;
    }

    public Task<Option<int>> Run(ScopeContext context)
    {
        _context = context;
        context.Location().LogInformation("Starting to run executeFile={executeFile}, args={args}, workingDirectory={workingDirectory}",
            _process.StartInfo.FileName, _process.StartInfo.Arguments, _process.StartInfo.WorkingDirectory);

        int isRunning = Interlocked.CompareExchange(ref _isRunning, 1, 0);
        if (isRunning == 1) return new Option<int>(-1).ToTaskResult();

        _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
        _tcs = new TaskCompletionSource<Option<int>>();

        _process.OutputDataReceived += (s, e) => LogOutput(e.Data);
        _process.ErrorDataReceived += (s, e) => LogOutput(e.Data);
        _process.Exited += OnProcessExit;

        _tokenSource.Token.Register(() =>
        {
            context.Location().LogError($"Canceled local process, File={_process.StartInfo.FileName}");
            Stop(context);
        });

        // Start process
        bool started;
        try
        {
            started = _process.Start();
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            _context.Location().LogError(ex, "Start failed");
            started = false;
        }

        if (!started)
        {
            string msg = $"Process failed to start, File={_process.StartInfo.FileName}";
            context.Location().LogError(msg);
            return new Option<int>(StatusCode.BadRequest, msg).ToTaskResult();
        }

        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        return _tcs.Task;
    }

    public void Stop(ScopeContext context)
    {
        _isRunning.Assert(x => x == 1, "Not running");
        context.Location().LogInformation("Stopping process, fileName={fileName}", _process.StartInfo.FileName);

        var hasProcessed = Interlocked.CompareExchange(ref _hasProcessedExit, 1, 0);
        if (hasProcessed == 1) return;

        try
        {
            try { _process?.Kill(); } catch { }
        }
        finally
        {
            _tcs.NotNull().SetResult(-1);
        }
    }

    private void OnProcessExit(object? sender, EventArgs args)
    {
        var hasProcessed = Interlocked.CompareExchange(ref _hasProcessedExit, 1, 0);
        if (hasProcessed == 1) return;

        _context.Location().LogInformation("Process has exit, ExitCode={ExitCode}", _process.ExitCode);
        _tcs.NotNull().SetResult(_process.ExitCode);
    }

    private void LogOutput(string? data)
    {
        if (data == null) return;

        string message = $"LocalProcess: {data}";
        _context.Location().LogInformation(message);

        _captureOutput?.Invoke(message);
    }
}
