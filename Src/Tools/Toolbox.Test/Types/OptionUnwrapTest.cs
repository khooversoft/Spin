using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class OptionUnwrapTest
{
    [Fact]
    public void OptionWrapTest()
    {
        var option = new Option(StatusCode.OK, "no error");
        Option<Option> wrapped = new Option<Option>(option, StatusCode.OK);

        option.StatusCode.Should().Be(StatusCode.OK);
        option.Error.Should().Be("no error");
        wrapped.StatusCode.Should().Be(StatusCode.OK);
        wrapped.Error.BeNull();
        wrapped.Value.StatusCode.Should().Be(StatusCode.OK);
        wrapped.Value.Error.Should().Be("no error");

        Option unwrapped = wrapped.Return();
        unwrapped.StatusCode.Should().Be(StatusCode.OK);
        unwrapped.Error.Should().Be("no error");
    }

    [Fact]
    public void OptionWithValueWrapTest()
    {
        var option = new Option<string>("value", StatusCode.OK, "no error");
        Option<Option<string>> wrapped = new Option<Option<string>>(option, StatusCode.OK);

        option.Value.Should().Be("value");
        option.HasValue.Should().BeTrue();
        option.StatusCode.Should().Be(StatusCode.OK);
        option.Error.Should().Be("no error");
        wrapped.StatusCode.Should().Be(StatusCode.OK);
        wrapped.Error.BeNull();
        wrapped.Value.StatusCode.Should().Be(StatusCode.OK);
        wrapped.Value.Error.Should().Be("no error");

        Option<string> unwrapped = wrapped.Return();
        unwrapped.HasValue.Should().BeTrue();
        unwrapped.Value.Should().Be("value");
        unwrapped.StatusCode.Should().Be(StatusCode.OK);
        unwrapped.Error.Should().Be("no error");
    }
}
