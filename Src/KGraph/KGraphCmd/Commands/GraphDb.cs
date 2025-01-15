using KGraphCmd.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace KGraphCmd.Commands;

internal class GraphDb : ICommandRoute
{
    private readonly AbortSignal _abortSignal;
    private readonly ILogger<TraceLog> _logger;
    private readonly ScopeContext _context;

    public GraphDb(AbortSignal abortSignal, ILogger<TraceLog> logger)
    {
        _abortSignal = abortSignal.NotNull();
        _logger = logger.NotNull();
        _context = new ScopeContext(_logger);
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("db", "Query or reset KGraph's database files")
    {
        new CommandSymbol("clear", "Clear transactions").Action(x =>
        {
            var jsonFile = x.AddArgument<string>("jsonFile", "Json file with data lake connection details");
            x.SetHandler(Clear, jsonFile);
        }),
    };

    private async Task Clear(string jsonFile)
    {
        await using var services = HostTool.StartHost(jsonFile);

        var fileStore = services.GetRequiredService<IFileStore>().NotNull();
        var files = await fileStore.Search(GraphConstants.DbDatabaseSearchPath, _context);
        files = files.OrderByDescending(x => x).ToArray();

        foreach (var file in files)
        {
            _context.LogInformation("Deleting file {file}", file);
            var option = await fileStore.Delete(file, _context);
            if (option.IsError()) option.LogStatus(_context, "Failed to delete file {file}", [file]);
        }
    }
}
