//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Graph.test.Application;

//public abstract class TestBase
//{
//    private readonly ITestOutputHelper _output;


//    public TestBase(ITestOutputHelper output)
//    {
//        _output = output.NotNull();

//        Services = new ServiceCollection()
//            .AddLogging(x =>
//            {
//                x.AddLambda(_output.WriteLine);
//                x.AddDebug();
//                x.AddConsole();
//            })
//            .BuildServiceProvider();
//    }

//    public ServiceProvider Services { get; }

//    //public ILogger<T> GetLogger<T>() where T : notnull => Services.GetRequiredService<ILogger<T>>();
//}

//public abstract class TestBase<T> : TestBase where T : notnull
//{
//    public TestBase(ITestOutputHelper output) : base(output) { }

//    public ILogger<T> GetLogger() => Services.GetRequiredService<ILogger<T>>();
//}