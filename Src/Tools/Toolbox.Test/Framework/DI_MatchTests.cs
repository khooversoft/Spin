using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Framework;

public class DI_MatchTests
{
    [Fact]
    public void TestSingleInterface()
    {
        var di = new ServiceCollection()
            .AddSingleton<ITest1, Test1>()
            .AddSingleton<Test>()
            .BuildServiceProvider();

        Test t1 = di.GetRequiredService<Test>();
        t1.NotNull();
        t1._test1.NotNull();
        t1._test2.BeNull();
    }

    [Fact]
    public void TestBothInterface()
    {
        var di = new ServiceCollection()
            .AddSingleton<ITest1, Test1>()
            .AddSingleton<ITest2, Test2>()
            .AddSingleton<Test>()
            .BuildServiceProvider();

        Test t1 = di.GetRequiredService<Test>();
        t1.NotNull();
        t1._test1.NotNull();
        t1._test2.NotNull();
    }

    [Fact]
    public void TestSingleInterfaceWithKeyed()
    {
        var di = new ServiceCollection()
            .AddKeyedSingleton<ITest1, Test1>("t1")
            .AddSingleton<TestKeyed>()
            .BuildServiceProvider();

        TestKeyed t1 = di.GetRequiredService<TestKeyed>();
        t1.NotNull();
        t1._test1.NotNull();
        t1._test2.BeNull();
    }

    [Fact]
    public void TestBothInterfaceWithKeyed()
    {
        var di = new ServiceCollection()
            .AddKeyedSingleton<ITest1, Test1>("t1")
            .AddKeyedSingleton<ITest2, Test2>("t2")
            .AddSingleton<TestKeyed>()
            .BuildServiceProvider();

        TestKeyed t1 = di.GetRequiredService<TestKeyed>();
        t1.NotNull();
        t1._test1.NotNull();
        t1._test2.NotNull();
    }

    public interface ITest1 { }
    public interface ITest2 { }

    public class Test
    {
        public readonly ITest1 _test1;
        public readonly ITest2? _test2;

        public Test(ITest1 test1)
        {
            _test1 = test1;
        }

        public Test(ITest1 test1, ITest2 test2)
        {
            _test1 = test1;
            _test2 = test2;
        }
    }

    public class TestKeyed
    {
        public readonly ITest1 _test1;
        public readonly ITest2? _test2;

        public TestKeyed([FromKeyedServices("t1")] ITest1 test1)
        {
            _test1 = test1;
        }

        public TestKeyed([FromKeyedServices("t1")] ITest1 test1, [FromKeyedServices("t2")] ITest2 test2)
        {
            _test1 = test1;
            _test2 = test2;
        }
    }

    public class Test1() : ITest1 { }
    public class Test2() : ITest2 { }
}
