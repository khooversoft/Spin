//using FluentAssertions;
//using Toolbox.LangTools;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Test.LangTools;

//public class LangOptionTests
//{
//    private readonly ILangRoot _root;

//    public LangOptionTests()
//    {
//        var equalValue = new LsRoot("equal") + new LsValue("lvalue") + ("=", "equal") + new LsValue("rvalue");
//        var addValue = new LsRoot("plus") + new LsValue("lvalue") + ("+", "plusSign") + new LsValue("rvalue");

//        _root = new LsRoot() + (new LsOption("optional") + equalValue + addValue);
//    }

//    [Fact]
//    public void SimpleFormatWithRootShouldFail()
//    {
//        var ins = new LsOption() + (new LsRoot() + new LsToken("=", "equal")) + (new LsRoot() + new LsToken("+", "plus"));
//        LangResult tree = ins.Parse("=");
//        tree.StatusCode.IsOk().Should().BeTrue(tree.StatusCode.ToString());

//        LangNodes nodes = tree.LangNodes.NotNull();
//        nodes.Children.Count.Should().Be(1);
//        nodes.Children[0].SyntaxNode.Name.Should().Be("equal");

//        tree = ins.Parse("+");
//        tree.StatusCode.IsOk().Should().BeTrue();

//        nodes = tree.LangNodes.NotNull();
//        nodes.Children.Count.Should().Be(1);
//        nodes.Children[0].SyntaxNode.Name.Should().Be("plus");

//        tree = ins.Parse("*");
//        tree.StatusCode.IsError().Should().BeTrue(tree.StatusCode.ToString());
//    }

//    [Fact]
//    public void SimpleFormat()
//    {
//        var ins = new LsRoot("root")
//            + (new LsOption("option") + new LsToken("=", "equal") + new LsToken("+", "plus"))
//            + new LsValue("catch", true);

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

//        tree = ins.Parse("*");
//        tree.StatusCode.IsOk().Should().BeTrue(tree.StatusCode.ToString());
//        nodes = tree.LangNodes.NotNull();
//        nodes.Children.Count.Should().Be(1);
//        nodes.Children[0].SyntaxNode.Name.Should().Be("catch");
//    }

//    [Fact]
//    public void Failures()
//    {
//        var tests = new QueryTest[]
//        {
//            new QueryTest { RawData = "key", Results = new List<IQueryResult>() },
//            new QueryTest { RawData = "=", Results = new List<IQueryResult>() },
//        };

//        foreach (var test in tests)
//        {
//            LangTestTools.Verify(_root, test);
//        }
//    }

//    [Fact]
//    public void SingleAssignment()
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
//    public void SingleMath()
//    {
//        var test = new QueryTest
//        {
//            RawData = "value+5",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsValue>("value","lvalue"),
//                new QueryResult<LsToken>("+", "plusSign"),
//                new QueryResult<LsValue>("5", "rvalue"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }
//}
