using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Data.DataList;

public class ListStoreTimeIndexTests_Datalake : Toolbox.Test.Store.ListStoreTimeIndexTests
{
    public ListStoreTimeIndexTests_Datalake(ITestOutputHelper outputHelper) : base(outputHelper) { }

    protected override void AddStore(IServiceCollection services)
    {
        var datalakeOption = TestApplication.ReadOption("ListStoreTimeIndexTests_Datalake");
        services.AddDatalakeFileStore(datalakeOption);
    }
}
