//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.DependencyInjection;
//using Toolbox.Azure;
//using Toolbox.Graph.test.Application;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Graph.test.Store.Datalake;

//public class DatabaseInitializationTesting
//{
//    const string _basePath = $"graphTesting-{nameof(DatabaseInitializationTesting)}";
//    private readonly ITestOutputHelper _output;
//    public DatabaseInitializationTesting(ITestOutputHelper output) => _output = output;

//    [Fact]
//    public async Task ExclusiveLockDbNotInitialized()
//    {
//        (IServiceProvider service, ScopeContext context) = TestApplication.CreateDatalakeDirect<DatabaseInitializationTesting>(_basePath, _output);
//        IFileStore fileStore = service.GetRequiredService<IFileStore>().NotNull();

//        (await fileStore.File(GraphConstants.MapDatabasePath).ForceDelete(context)).BeOk();

//        var exclusiveLease =

//    }
//}
