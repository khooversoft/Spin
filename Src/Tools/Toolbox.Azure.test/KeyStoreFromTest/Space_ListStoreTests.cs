using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Toolbox.Test.Store.ListStore;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Store;

public class Space_ListStoreTests : DataSpaceListTests
{
    public Space_ListStoreTests(ITestOutputHelper output) : base(output) { }

    protected override void AddStore(IServiceCollection services, string basePath)
    {
        var datalakeOption = TestApplication.ReadOption(basePath);
        services.AddDatalakeFileStore(datalakeOption);
    }
}