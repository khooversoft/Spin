using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class OptionBindTests
{

    [Fact]
    public void ValidValues_WhenMatch()
    {
        var value = 5.ToOption()
            .Bind(x => x + 10)
            .Bind(x => x + 20)
            .Return();

        value.Should().Be(35);

        var value1 = 10.ToOption()
            .Bind(x => x + 10)
            .Bind(x => x + 20) switch
        {
            var v when v == Option<int>.None => -1,
            var v => v.Return(),
        };

        value1.Should().Be(40);
    }

    [Fact]
    public void NotValidValues_WhenMatch()
    {
        var value = 5.ToOption()
            .Bind(x => x + 10)
            .Bind(x => Option<int>.None)
            .Bind(x => x + 20) switch
        {
            var v when !v.HasValue => -1,
            var v => v.Return(),
        };

        value.Should().Be(-1);

        var value1 = 5.ToOption()
            .Bind(x => x + 10)
            .Bind(x => Option<int>.None)
            .Bind(x => x + 20) switch
        {
            var v when v == Option<int>.None => -1,
            var v => v.Return(),
        };

        value.Should().Be(-1);
    }
}
