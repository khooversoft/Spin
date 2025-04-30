using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class OptionUnwrapTest
{
    [Fact]
    public void OptionWrapTest()
    {
        var option = new Option(StatusCode.OK, "no error");
        Option<Option> wrapped = new Option<Option>(option, StatusCode.OK);

        option.StatusCode.Be(StatusCode.OK);
        option.Error.Be("no error");
        wrapped.StatusCode.Be(StatusCode.OK);
        wrapped.Error.BeNull();
        wrapped.Value.StatusCode.Be(StatusCode.OK);
        wrapped.Value.Error.Be("no error");

        Option unwrapped = wrapped.Return();
        unwrapped.StatusCode.Be(StatusCode.OK);
        unwrapped.Error.Be("no error");
    }

    [Fact]
    public void OptionWithValueWrapTest()
    {
        var option = new Option<string>("value", StatusCode.OK, "no error");
        Option<Option<string>> wrapped = new Option<Option<string>>(option, StatusCode.OK);

        option.Value.Be("value");
        option.HasValue.BeTrue();
        option.StatusCode.Be(StatusCode.OK);
        option.Error.Be("no error");
        wrapped.StatusCode.Be(StatusCode.OK);
        wrapped.Error.BeNull();
        wrapped.Value.StatusCode.Be(StatusCode.OK);
        wrapped.Value.Error.Be("no error");

        Option<string> unwrapped = wrapped.Return();
        unwrapped.HasValue.BeTrue();
        unwrapped.Value.Be("value");
        unwrapped.StatusCode.Be(StatusCode.OK);
        unwrapped.Error.Be("no error");
    }
}
