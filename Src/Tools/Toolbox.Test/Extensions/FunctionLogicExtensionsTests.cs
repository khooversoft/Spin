using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Extensions;

public class FunctionLogicExtensionsTests
{
    [Fact]
    public void IfTrueWithTestAndActionT()
    {
        bool test = false;
        string? state = null;

        test.IfTrue(x => x, x => state = x.ToString());
        state.IsEmpty().Should().BeTrue();

        test = true;
        test.IfTrue(x => x, x => state = x.ToString());
        state.Should().Be("True");
    }

    [Fact]
    public void IfTrueWithTestAndAction()
    {
        bool test = false;
        string? state = null;

        test.IfTrue(x => x, () => state = test.ToString());
        state.IsEmpty().Should().BeTrue();

        test = true;
        test.IfTrue(x => x, () => state = test.ToString());
        state.Should().Be("True");
    }

    [Fact]
    public void IfTrueTestAndAction()
    {
        bool test = false;
        string? state = null;

        test.IfTrue(() => state = test.ToString());
        state.IsEmpty().Should().BeTrue();

        test = true;
        test.IfTrue(() => state = test.ToString());
        state.Should().Be("True");
    }

    [Fact]
    public void IfFalseTestAndAction()
    {
        bool test = true;
        object? state = null;

        test.IfFalse(() => state = test.ToString());
        state.BeNull();

        state = null;
        test = false;
        test.IfFalse(() => state = test.ToString());
        state.Should().Be("False");
    }

    [Fact]
    public void IfNullTestAndAction()
    {
        string? test = null;
        object? state = null;

        test.IfNull(() => state = "pass");
        state.Should().Be("pass");

        state = null;
        test = "value";
        test.IfNull(() => state = "pass");
        state.BeNull();
    }

    [Fact]
    public void IfNotNullTestAndAction()
    {
        string? test = null;
        object? state = null;

        test.IfNotNull(x => state = "set");
        state.BeNull();

        state = null;
        test = "value";
        test.IfNotNull(x => state = x + "set");
        state.Should().Be("valueset");
    }

    [Fact]
    public void IfEmptyTestAndAction()
    {
        string? test = null;
        object? state = null;

        test.IfEmpty(() => state = "pass");
        state.Should().Be("pass");

        state = null;
        test = "value";
        test.IfEmpty(() => state = "pass");
        state.BeNull();
    }

    [Fact]
    public void IfNotEmptyTestAndAction()
    {
        string? test = null;
        object? state = null;

        test.IfNotEmpty(x => state = "set");
        state.BeNull();

        state = null;
        test = "value";
        test.IfNotEmpty(x => state = x + "set");
        state.Should().Be("valueset");
    }
}
