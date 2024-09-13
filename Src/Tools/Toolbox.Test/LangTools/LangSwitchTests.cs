//using FluentAssertions;
//using Toolbox.LangTools;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Test.LangTools;

//public class LangSwitchTests
//{
//    private readonly ILangRoot _root;

//    public LangSwitchTests()
//    {
//        var equalValue = new LsRoot("equal") + new LsValue("lvalue") + ("=", "equal") + new LsValue("rvalue");
//        var valueOnly = new LsRoot("single") + new LsValue("svalue");

//        _root = new LsRoot() + (new LsSwitch("or") + equalValue + valueOnly);
//    }


//    [Fact]
//    public void SimpleFormatWithRoot()
//    {
//        var ins = new LsSwitch() + (new LsRoot() + new LsToken("=", "equal")) + (new LsRoot() + new LsToken("+", "plus"));
//        LangResult tree = ins.Parse("=");
//        tree.StatusCode.IsOk().Should().BeTrue();

//        LangNodes nodes = tree.LangNodes.NotNull();
//        nodes.Children.Count.Should().Be(1);
//        nodes.Children[0].SyntaxNode.Name.Should().Be("equal");

//        tree = ins.Parse("+");
//        tree.StatusCode.IsOk().Should().BeTrue();

//        nodes = tree.LangNodes.NotNull();
//        nodes.Children.Count.Should().Be(1);
//        nodes.Children[0].SyntaxNode.Name.Should().Be("plus");
//    }

//    [Fact]
//    public void SimpleFormat()
//    {
//        var ins = new LsRoot("root") + (new LsSwitch("option") + new LsToken("=", "equal") + new LsToken("+", "plus"));
//        LangResult tree = ins.Parse("=");
//        tree.StatusCode.IsOk().Should().BeTrue();

//        LangNodes nodes = tree.LangNodes.NotNull();
//        nodes.Children.Count.Should().Be(1);
//        nodes.Children[0].SyntaxNode.Name.Should().Be("equal");

//        tree = ins.Parse("+");
//        tree.StatusCode.IsOk().Should().BeTrue();

//        nodes = tree.LangNodes.NotNull();
//        nodes.Children.Count.Should().Be(1);
//        nodes.Children[0].SyntaxNode.Name.Should().Be("plus");
//    }

//    [Fact]
//    public void Failure()
//    {
//        var test = new QueryTest { RawData = "=", Results = new List<IQueryResult>() };
//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void FirstOrPattern()
//    {
//        var test = new QueryTest
//        {
//            RawData = "key='string value'",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("string value", "rvalue"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void SecondOrPattern()
//    {
//        var test = new QueryTest
//        {
//            RawData = "key",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsValue>("key","svalue"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }
//}
