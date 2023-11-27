using FluentAssertions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Test.Validation;

public class ValidationNameTests
{
    private record NameTest
    {
        public string Name { get; set; } = null!;
    }

    private record NameTestOption
    {
        public string? Name { get; set; }
    }

    [Fact]

    public void NullMistake()
    {
        IValidator<NameTest> validator = new Validator<NameTest>()
            .RuleFor(x => x.Name).ValidName()
            .Build();

        var model = new NameTest
        {
            Name = null!,
        };

        var result = validator.Validate(model);
        result.IsError().Should().BeTrue();
        result.Return().As<ValidatorResult>().Errors.Count().Should().Be(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("~!(")]
    [InlineData("&")]

    public void NullAndInvalidCharacters(string? value)
    {
        IValidator<NameTestOption> validator = new Validator<NameTestOption>()
            .RuleFor(x => x.Name).ValidNameOption()
            .Build();

        var model = new NameTestOption
        {
            Name = value,
        };

        var result = validator.Validate(model);
        result.IsError().Should().BeTrue();
        result.Return().As<ValidatorResult>().Errors.Count().Should().Be(1);
    }

    [Theory]
    [InlineData("name")]
    [InlineData("abcedefhijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890-$E")]
    public void NameIsValid(string subject)
    {
        IValidator<NameTest> validator = new Validator<NameTest>()
            .RuleFor(x => x.Name).ValidName()
            .Build();

        var model = new NameTest
        {
            Name = subject
        };

        var result = validator.Validate(model);
        result.IsOk().Should().BeTrue();
        result.Return().As<ValidatorResult>().Errors.Count().Should().Be(0);
    }
}
