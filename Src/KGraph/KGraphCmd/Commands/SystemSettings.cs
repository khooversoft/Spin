using KGraphCmd.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Journal;
using Toolbox.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace KGraphCmd.Commands;

internal class SystemSettings : ICommandRoute
{
    private readonly GraphHostManager _graphHostManager;
    private readonly AbortSignal _abortSignal;
    private readonly ScopeContext _context;
    private readonly ILogger<SystemSettings> _logger;

    public SystemSettings(GraphHostManager graphHostManager, AbortSignal abortSignal, ILogger<SystemSettings> logger)
    {
        _graphHostManager = graphHostManager.NotNull();
        _abortSignal = abortSignal.NotNull();
        _logger = logger.NotNull();

        _context = new ScopeContext(_logger);
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("settings", "KGraph's database management and settings")
    {
        new CommandSymbol("clear-database", "Clear all data, transactions, payloads, and graph database").Action(x =>
        {
            var jsonFile = x.AddOption<string?>("--config", "Json file with data lake connection details");
            var confirm = x.AddOption<bool>("--confirm", "Confirm destructive operation");
            x.SetHandler(ClearDatabase, jsonFile, confirm);
        }),
        new CommandSymbol("clear-transactions-log", "Clear transactions log files").Action(x =>
        {
            var jsonFile = x.AddArgument<string>("jsonFile", "Json file with data lake connection details");
            var confirm = x.AddOption<bool>("--confirm", "Confirm destructive operation");
            x.SetHandler(ClearTransactionLogs, jsonFile, confirm);
        }),
        //new CommandSymbol("clear-trace-log", "Clear trace log files").Action(x =>
        //{
        //    var jsonFile = x.AddArgument<string>("jsonFile", "Json file with data lake connection details");
        //    var confirm = x.AddOption<bool>("--confirm", "Confirm destructive operation");
        //    x.SetHandler(ClearTraceLogs, jsonFile, confirm);
        //}),
    };

    private Task ClearTransactionLogs(string jsonFile, bool confirm) => ClearLogs(jsonFile, GraphConstants.TrxJournal.DiKeyed, confirm);
    //private Task ClearTraceLogs(string jsonFile, bool confirm) => ClearLogs(jsonFile, GraphConstants.Trace.DiKeyed, confirm);

    private bool CheckConfirm(bool confirm)
    {
        if (confirm) return true;

        _context.LogError("Requires '--confirm' switch");
        return false;
    }

    private async Task ClearDatabase(string? jsonFile, bool confirm)
    {
        if (!CheckConfirm(confirm)) return;

        if (jsonFile.IsNotEmpty()) _graphHostManager.Start(jsonFile);

        var fileStore = _graphHostManager.ServiceProvider.GetRequiredService<IFileStore>().NotNull();
        var files = await fileStore.Search(GraphConstants.DbDatabaseSearchPath, _context);
        files = files.OrderByDescending(x => x).ToArray();

        foreach (var file in files)
        {
            _context.LogInformation("Deleting file {file}", file);
            var option = await fileStore.File(file.Path).Delete(_context);
            if (option.IsError()) option.LogStatus(_context, "Failed to delete file {file}", [file]);
        }
    }

    private async Task ClearLogs(string jsonFile, string keyedType, bool confirm)
    {
        if (!CheckConfirm(confirm)) return;

        var services = _graphHostManager.Start(jsonFile);

        var fileStore = services.GetRequiredService<IFileStore>().NotNull();

        var traceLog = services.GetRequiredKeyedService<IJournalFile>(keyedType).NotNull();
        var files = await traceLog.GetFiles(_context);

        foreach (var file in files)
        {
            _context.LogInformation($"Deleting file {keyedType} {file}", file);
            var option = await fileStore.File(file).Delete(_context);
            if (option.IsError()) option.LogStatus(_context, "Failed to delete file {file}", [file]);
        }
    }
}
