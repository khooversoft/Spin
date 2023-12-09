using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class OptionConstructorTests
{
    [Fact]
    public void DefaultConstructorOptionNoParameter()
    {
        var option = new Option();
        option.StatusCode.Should().Be(default);
        option.Error.Should().BeNull();

        var option2 = new Option();
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void DefaultConstructorReferenceTypeNoParameter()
    {
        var option = new Option<string>();
        option.StatusCode.Should().Be(default);
        option.Error.Should().BeNull();
        option.HasValue.Should().BeFalse();
        option.Value.Should().Be(default);
        option.Return(false).Should().Be(default);

        var option2 = new Option<string>();
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void DefaultConstructorValueTypeNoParameter()
    {
        var option = new Option<int>();
        option.StatusCode.Should().Be(default);
        option.Error.Should().BeNull();
        option.HasValue.Should().BeFalse();
        option.Value.Should().Be(default);
        option.Return(false).Should().Be(default);

        var option2 = new Option<int>();
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionWithStatusCodeConstructor()
    {
        var option = new Option(StatusCode.BadRequest);
        option.StatusCode.Should().Be(StatusCode.BadRequest);
        option.Error.Should().BeNull();

        var option2 = new Option(StatusCode.BadRequest);
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionWithStatusCodeAndErrorConstructor()
    {
        var option = new Option(StatusCode.OK, "error");
        option.StatusCode.Should().Be(StatusCode.OK);
        option.Error.Should().Be("error");

        var option2 = new Option(StatusCode.OK, "error");
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionValueTypeConstructorPattern1()
    {
        var option = new Option<int>(false, default);
        option.HasValue.Should().BeFalse();
        option.StatusCode.Should().Be(StatusCode.NoContent);
        option.Error.Should().BeNull();
        option.Value.Should().Be(default);
        option.Return(false).Should().Be(default);

        var option2 = new Option<int>(false, default);
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionValueTypeConstructorPattern2()
    {
        var option = new Option<int>(true, 5);
        option.HasValue.Should().BeTrue();
        option.StatusCode.Should().Be(StatusCode.OK);
        option.Error.Should().BeNull();
        option.Value.Should().Be(5);
        option.Return().Should().Be(5);

        var option2 = new Option<int>(true, 5);
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionValueTypeConstructorPattern3()
    {
        var option = new Option<int>(true, 10, StatusCode.Forbidden);
        option.HasValue.Should().BeTrue();
        option.StatusCode.Should().Be(StatusCode.Forbidden);
        option.Error.Should().BeNull();
        option.Value.Should().Be(10);
        option.Return().Should().Be(10);

        var option2 = new Option<int>(true, 10, StatusCode.Forbidden);
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionValueTypeConstructorPattern4()
    {
        var option = new Option<int>(StatusCode.NotFound);
        option.HasValue.Should().BeFalse();
        option.StatusCode.Should().Be(StatusCode.NotFound);
        option.Error.Should().BeNull();
        option.Value.Should().Be(default);
        option.Return(false).Should().Be(default);

        var option2 = new Option<int>(StatusCode.NotFound);
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionValueTypeConstructorPattern5()
    {
        var option = new Option<int>(StatusCode.NotFound, "not found");
        option.HasValue.Should().BeFalse();
        option.StatusCode.Should().Be(StatusCode.NotFound);
        option.Error.Should().Be("not found");
        option.Value.Should().Be(default);
        option.Return(false).Should().Be(default);

        var option2 = new Option<int>(StatusCode.NotFound, "not found");
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionValueTypeConstructorPattern6()
    {
        var option = new Option<int>(15, StatusCode.InternalServerError);
        option.HasValue.Should().BeTrue();
        option.StatusCode.Should().Be(StatusCode.InternalServerError);
        option.Error.Should().BeNull();
        option.Value.Should().Be(15);
        option.Return().Should().Be(15);

        var option2 = new Option<int>(15, StatusCode.InternalServerError);
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionValueTypeConstructorPattern7()
    {
        var option = new Option<int>(default, StatusCode.NotFound);
        option.HasValue.Should().BeTrue();
        option.StatusCode.Should().Be(StatusCode.NotFound);
        option.Error.Should().BeNull();
        option.Value.Should().Be(default);
        option.Return().Should().Be(default);

        var option2 = new Option<int>(default, StatusCode.NotFound);
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionValueTypeConstructorPattern8()
    {
        var option = new Option<int>(default, StatusCode.NotFound, "not found really");
        option.HasValue.Should().BeTrue();
        option.StatusCode.Should().Be(StatusCode.NotFound);
        option.Error.Should().Be("not found really");
        option.Value.Should().Be(default);
        option.Return().Should().Be(default);

        var option2 = new Option<int>(default, StatusCode.NotFound, "not found really");
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionReferenceTypeConstructor()
    {
        var option = new Option<string?>(false, default);
        option.HasValue.Should().BeFalse();
        option.StatusCode.Should().Be(StatusCode.NoContent);
        option.Error.Should().BeNull();
        option.Value.Should().Be(default);
        option.Return(false).Should().Be(default);

        var option2 = new Option<string?>(false, default);
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionReferenceTypeConstructorPattern1()
    {
        var option = new Option<string?>(false, null);
        option.HasValue.Should().BeFalse();
        option.StatusCode.Should().Be(StatusCode.NoContent);
        option.Error.Should().BeNull();
        option.Value.Should().BeNull();
        option.Return(false).Should().BeNull();

        var option2 = new Option<string?>(false, null);
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionReferenceTypeConstructorPattern2()
    {
        var option = new Option<string>(false, default!);
        option.HasValue.Should().BeFalse();
        option.StatusCode.Should().Be(StatusCode.NoContent);
        option.Error.Should().BeNull();
        option.Value.Should().Be(default);
        option.Return(false).Should().BeNull(default);

        var option2 = new Option<string>(false, default!);
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionReferenceTypeConstructorPattern3()
    {
        var option = new Option<string>(true, "value");
        option.HasValue.Should().BeTrue();
        option.StatusCode.Should().Be(StatusCode.OK);
        option.Error.Should().BeNull();
        option.Value.Should().Be("value");
        option.Return().Should().Be("value");

        var option2 = new Option<string>(true, "value");
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionReferenceTypeConstructorPattern4()
    {
        var option = new Option<string?>(true, "value");
        option.HasValue.Should().BeTrue();
        option.StatusCode.Should().Be(StatusCode.OK);
        option.Error.Should().BeNull();
        option.Value.Should().Be("value");
        option.Return().Should().Be("value");

        var option2 = new Option<string?>(true, "value");
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionReferenceTypeConstructorPattern5()
    {
        var option = new Option<string>(true, "10", StatusCode.Forbidden);
        option.HasValue.Should().BeTrue();
        option.StatusCode.Should().Be(StatusCode.Forbidden);
        option.Error.Should().BeNull();
        option.Value.Should().Be("10");
        option.Return().Should().Be("10");

        var option2 = new Option<string>(true, "10", StatusCode.Forbidden);
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionReferenceTypeConstructorPattern6()
    {
        var option = new Option<string>(StatusCode.NotFound);
        option.HasValue.Should().BeFalse();
        option.StatusCode.Should().Be(StatusCode.NotFound);
        option.Error.Should().BeNull();
        option.Value.Should().Be(default);
        option.Return(false).Should().BeNull(default);

        var option2 = new Option<string>(StatusCode.NotFound);
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionReferenceTypeConstructorPattern7()
    {
        var option = new Option<string>(StatusCode.NotFound, "not found");
        option.HasValue.Should().BeFalse();
        option.StatusCode.Should().Be(StatusCode.NotFound);
        option.Error.Should().Be("not found");
        option.Value.Should().Be(default);
        option.Return(false).Should().BeNull(default);

        var option2 = new Option<string>(StatusCode.NotFound, "not found");
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionReferenceTypeConstructorPattern8()
    {
        var option = new Option<string>("15", StatusCode.InternalServerError);
        option.HasValue.Should().BeTrue();
        option.StatusCode.Should().Be(StatusCode.InternalServerError);
        option.Error.Should().BeNull();
        option.Value.Should().Be("15");
        option.Return().Should().Be("15");

        var option2 = new Option<string>("15", StatusCode.InternalServerError);
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionReferenceTypeConstructorPattern9()
    {
        var option = new Option<string?>(default, StatusCode.NotFound);
        option.HasValue.Should().BeFalse();
        option.StatusCode.Should().Be(StatusCode.NotFound);
        option.Error.Should().BeNull();
        option.Value.Should().Be(default);
        option.Return(false).Should().BeNull(default);

        var option2 = new Option<string?>(default, StatusCode.NotFound);
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionReferenceTypeConstructorPattern10()
    {
        var option = new Option<string>(default!, StatusCode.NotFound, "not found really");
        option.HasValue.Should().BeFalse();
        option.StatusCode.Should().Be(StatusCode.NotFound);
        option.Error.Should().Be("not found really");
        option.Value.Should().Be(default);
        option.Return(false).Should().BeNull(default);

        var option2 = new Option<string>(default!, StatusCode.NotFound, "not found really");
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }

    [Fact]
    public void OptionReferenceTypeConstructorPattern11()
    {
        var option = new Option<string>("value", StatusCode.NotFound, "not found really");
        option.HasValue.Should().BeTrue();
        option.StatusCode.Should().Be(StatusCode.NotFound);
        option.Error.Should().Be("not found really");
        option.Value.Should().Be("value");
        option.Return().Should().Be("value");

        var option2 = new Option<string>("value", StatusCode.NotFound, "not found really");
        (option == option2).Should().BeTrue();
        (option != option2).Should().BeFalse();
    }
}
