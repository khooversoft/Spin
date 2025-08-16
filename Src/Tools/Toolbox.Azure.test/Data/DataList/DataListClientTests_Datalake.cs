//using Microsoft.Extensions.DependencyInjection;
//using Toolbox.Azure.test.Application;
//using Xunit.Abstractions;

//namespace Toolbox.Azure.test.Data.DataList;

//public class DataListClientTests_Datalake : Toolbox.Test.Data.Client.DataListClientTests
//{
//    public DataListClientTests_Datalake(ITestOutputHelper outputHelper) : base(outputHelper) { }

//    protected override void AddStore(IServiceCollection services)
//    {
//        var datalakeOption = TestApplication.ReadOption("DataListClientTests_Datalake");
//        services.AddDatalakeFileStore(datalakeOption);
//    }
//}
