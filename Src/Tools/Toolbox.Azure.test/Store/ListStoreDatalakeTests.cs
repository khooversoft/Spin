using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Store;

public class ListStoreDatalakeTests : Test.Store.ListStoreTests
{
    public ListStoreDatalakeTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    protected override void AddStore(IServiceCollection services)
    {
        var datalakeOption = TestApplication.ReadOption("ListStoreTests");
        services.AddDatalakeFileStore(datalakeOption);
    }
}
