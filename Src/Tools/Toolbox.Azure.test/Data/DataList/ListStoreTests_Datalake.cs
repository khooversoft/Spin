using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Data.DataList;

public class ListStoreTests_Datalake : Toolbox.Test.Data.Client.DataListClientTests
{
    public ListStoreTests_Datalake(ITestOutputHelper outputHelper) : base(outputHelper) { }

    protected override void AddStore(IServiceCollection services)
    {
        var datalakeOption = TestApplication.ReadOption("ListStoreTests_Datalake");
        services.AddDatalakeFileStore(datalakeOption);
    }
}
