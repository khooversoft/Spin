using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Graph.test.Application;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Store.Datalake;

public class DatabaseInitializationTesting
{
    const string _basePath = $"graphTesting-{nameof(DatabaseInitializationTesting)}";
    private readonly ITestOutputHelper _outputHelper;

    private readonly ScopeContext _context;
    private readonly ITestOutputHelper _output;
    public DatabaseInitializationTesting(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task ExclusiveLockDbNotInitialized()
    {
        (IServiceProvider service, ScopeContext context) = TestApplication.CreateDatalakeDirect<DatabaseInitializationTesting>(_basePath, _outputHelper);
        IFileStore fileStore = service.GetRequiredService<IFileStore>().NotNull();

        await ForceDelete(fileStore, GraphConstants.MapDatabasePath);
        var deleteOption = await fileStore.File(GraphConstants.MapDatabasePath).Delete(context);

        var exclusiveLease =

    }

    private async Task ForceDelete(IFileStore fileStore, string path)
    {
        var deleteOption = (await fileStore.File(path).Delete(_context)).LogStatus(_context, "Delete file {path}", [path]);
        if (deleteOption.IsOk() || !deleteOption.IsLocked()) return;

        var breakOption = (await fileStore.File(path).BreakLease(_context)).LogStatus(_context, "Break lease {path}", [path]);
        breakOption.BeOk();

        (await fileStore.File(path).Delete(_context)).LogStatus(_context, "Delete file {path}", [path]).BeOk();
    }

    private async Task ForceSet(IFileStore fileStore, string path, DataETag data)
    {
        var writeOption = (await fileStore.File(path).Set(data, _context)).LogStatus(_context, "Delete file {path}", [path]);
        if (writeOption.IsOk() || !writeOption.IsLocked()) return;

        var breakOption = (await fileStore.File(path).BreakLease(_context)).LogStatus(_context, "Break lease {path}", [path]);
        breakOption.BeOk();

        (await fileStore.File(path).Set(data, _context)).LogStatus(_context, "Delete file {path}", [path]).BeOk();
    }
}
