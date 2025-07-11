using Toolbox.Extensions;
using Toolbox.Tools;
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

        value.Be(35);

        var value1 = 10.ToOption()
            .Bind(x => x + 10)
            .Bind(x => x + 20) switch
        {
            var v when v == Option<int>.None => -1,
            var v => v.Return(),
        };

        value1.Be(40);
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

        value.Be(-1);

        var value1 = 5.ToOption()
            .Bind(x => x + 10)
            .Bind(x => Option<int>.None)
            .Bind(x => x + 20) switch
        {
            var v when v == Option<int>.None => -1,
            var v => v.Return(),
        };

        value.Be(-1);
    }

    [Fact]
    public void BindWithErrors()
    {
        Option<string> option1 = new Option<string>("hello").Bind(x => "that works");
        option1.IsOk().BeTrue();
        option1.Return().Be("that works");

        Option<string> option2 = new Option<string>(StatusCode.BadRequest).Bind(x => "that works");
        option2.IsError().BeTrue();
        option2.StatusCode.Be(StatusCode.BadRequest);
        option2.Value.BeEmpty();

        Option<string> option3 = "hello".ToOption()
            .Bind(x => x + ":suffix")
            .Bind(x => x + ":suffix2")
            .Bind(x => new Option<string>(StatusCode.Forbidden));

        option3.IsOk().BeFalse();
        option3.StatusCode.Be(StatusCode.Forbidden);
        option3.Value.BeEmpty();

        Option<string> option4 = new Option<string>(StatusCode.OK).Bind(x => "that works");
        option4.IsError().BeFalse();
        option4.StatusCode.Be(StatusCode.OK);
        option4.Value.BeEmpty();
    }

    [Fact]
    public async Task BindAsync()
    {
        int value = await 5.ToOption()
            .Bind(x => x + 10)
            .BindAsync<int, int>(async x => await add20(x))
            .BindAsync<int, int>(async x => await add20(x))
            .Return();

        value.Be(55);

        Task<int> add20(int value) => (value + 20).ToTaskResult();
    }
}
