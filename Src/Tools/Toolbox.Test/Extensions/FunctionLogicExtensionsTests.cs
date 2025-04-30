using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Extensions;

public class FunctionLogicExtensionsTests
{
    [Fact]
    public void IfTrueWithValue()
    {
        bool test = false;
        string? state = null;

        test.IfTrue(x => x, x => state = x.ToString());
        state.IsEmpty().BeTrue();

        test = true;
        state = null;
        test.IfTrue(x => x, x => state = x.ToString());
        state.Be("True");
    }

    [Fact]
    public void IfTrueAction()
    {
        bool test = false;
        string? state = null;

        test.IfTrue(() => state = test.ToString());
        state.IsEmpty().BeTrue();

        test = true;
        state = null;
        test.IfTrue(() => state = test.ToString());
        state.Be("True");
    }

    [Fact]
    public void IfElseWithValue()
    {
        string? state = null;
        bool test = false;

        test.IfElse(x => x, x => state = x.ToString(), x => state = "*");
        state.Be("*");

        test = true;
        state = null;
        test.IfElse(x => x, x => state = x.ToString(), x => state = "*");
        state.Be("True");
    }

    [Fact]
    public void IfElseAction()
    {
        string? state = null;

        bool test = false;
        test.IfElse(() => state = "true", () => state = "*");
        state.Be("*");

        test = true;
        state = null;
        test.IfElse(() => state = "true", () => state = "*");
        state.Be("true");
    }
}
