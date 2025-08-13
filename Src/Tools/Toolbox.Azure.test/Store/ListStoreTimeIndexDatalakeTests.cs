using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Store;

public class ListStoreTimeIndexDatalakeTests : Test.Store.ListStoreTimeIndexTests
{
    public ListStoreTimeIndexDatalakeTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    protected override void AddStore(IServiceCollection services)
    {
        var datalakeOption = TestApplication.ReadOption("ListStoreTimeIndexTests");
        services.AddDatalakeFileStore(datalakeOption);
    }
}
