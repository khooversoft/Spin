using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Data;

public class FileUpdateTests : Toolbox.Test.Data.Client.FileUpdateTests
{
    private const string _basePath = "data/single-file-updates";
    private const int _count = 100;

    public FileUpdateTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    protected override void AddStore(IServiceCollection services)
    {
        var datalakeOption = TestApplication.ReadOption("datastore-hybridCache-tests");
        services.AddDatalakeFileStore(datalakeOption);
    }
}
