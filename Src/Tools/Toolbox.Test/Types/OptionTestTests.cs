using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class OptionTestTests
{
    [Fact]
    public void SandardFlow()
    {
        var optionTest = new OptionTest();

        optionTest.Test(() => StatusCode.OK);
        optionTest.IsOk().BeTrue();
        optionTest.Option.StatusCode.Be(StatusCode.OK);

        optionTest.Test(() => StatusCode.OK);
        optionTest.IsOk().BeTrue();
        optionTest.Option.StatusCode.Be(StatusCode.OK);

        optionTest.Test(() => StatusCode.BadRequest);
        optionTest.IsError().BeTrue();
        optionTest.Option.StatusCode.Be(StatusCode.BadRequest);

        optionTest.Test(() => StatusCode.OK);
        optionTest.IsError().BeTrue();
        optionTest.Option.StatusCode.Be(StatusCode.BadRequest);
    }

    [Fact]
    public void FluentFlowPositive()
    {
        var test = new OptionTest()
            .Test(() => StatusCode.OK)
            .Test(() => StatusCode.OK);

        test.IsOk().BeTrue();
        test.IsError().BeFalse();
    }

    [Fact]
    public void FluentFlowFaile()
    {
        var test = new OptionTest()
            .Test(() => StatusCode.OK)
            .Test(() => StatusCode.BadRequest)
            .Test(() => StatusCode.OK);

        test.IsOk().BeFalse();
        test.IsError().BeTrue();
    }
}
