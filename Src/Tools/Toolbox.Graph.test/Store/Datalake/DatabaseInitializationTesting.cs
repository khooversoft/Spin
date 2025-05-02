//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.DependencyInjection;
//using Toolbox.Graph.test.Application;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Graph.test.Store.Datalake;

//public class DatabaseInitializationTesting
//{
//    const string _basePath = $"graphTesting-{nameof(DatabaseInitializationTesting)}";
//    private readonly ITestOutputHelper _outputHelper;
//    public DatabaseInitializationTesting(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

//    [Fact]
//    public async Task ExclusiveLockDbNotInitialized()
//    {
//        (IServiceProvider service, ScopeContext context) = TestApplication.CreateDatalakeDirect<DatabaseInitializationTesting>(_basePath, _outputHelper);

//        IFileStore fileStore = service.GetRequiredService<IFileStore>().NotNull();

//        var deleteOption = await fileStore.File(GraphConstants.MapDatabasePath).Delete(context);

//        var exclusiveLease = 

//    }
//}
