//using Microsoft.Extensions.DependencyInjection;
//using Toolbox.Azure.test.Application;
//using Xunit.Abstractions;

//namespace Toolbox.Azure.test.Data.DataClient;

//public class FileUpdateTests : Test.Data.Client.FileUpdateTests
//{
//    public FileUpdateTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

//    protected override void AddStore(IServiceCollection services)
//    {
//        var datalakeOption = TestApplication.ReadOption("FileUpdateTests");
//        services.AddDatalakeFileStore(datalakeOption);
//    }
//}
