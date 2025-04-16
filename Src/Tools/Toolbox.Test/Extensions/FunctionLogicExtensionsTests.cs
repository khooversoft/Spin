using Toolbox.Extensions;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Extensions;

public class FunctionLogicExtensionsTests
{
    [Fact]
    public void IfTrueWithValue()
    {
        bool test = false;
        string? state = null;

        test.IfTrue(x => x, x => state = x.ToString());
        state.IsEmpty().Should().BeTrue();

        test = true;
        state = null;
        test.IfTrue(x => x, x => state = x.ToString());
        state.Should().Be("True");
    }

    [Fact]
    public void IfTrueAction()
    {
        bool test = false;
        string? state = null;

        test.IfTrue(() => state = test.ToString());
        state.IsEmpty().Should().BeTrue();

        test = true;
        state = null;
        test.IfTrue(() => state = test.ToString());
        state.Should().Be("True");
    }

    [Fact]
    public void IfElseWithValue()
    {
        string? state = null;
        bool test = false;

        test.IfElse(x => x, x => state = x.ToString(), x => state = "*");
        state.Should().Be("*");

        test = true;
        state = null;
        test.IfElse(x => x, x => state = x.ToString(), x => state = "*");
        state.Should().Be("True");
    }

    [Fact]
    public void IfElseAction()
    {
        string? state = null;

        bool test = false;
        test.IfElse(() => state = "true", () => state = "*");
        state.Should().Be("*");

        test = true;
        state = null;
        test.IfElse(() => state = "true", () => state = "*");
        state.Should().Be("true");
    }
}
