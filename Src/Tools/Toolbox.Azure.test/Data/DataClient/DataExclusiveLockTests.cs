using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Data.DataClient;

public class DataExclusiveLockTests : Test.Data.Client.DataExclusiveLockTests
{
    public DataExclusiveLockTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    protected override void AddStore(IServiceCollection services)
    {
        var datalakeOption = TestApplication.ReadOption("DataExclusiveLockTests");
        services.AddDatalakeFileStore(datalakeOption);
    }
}
