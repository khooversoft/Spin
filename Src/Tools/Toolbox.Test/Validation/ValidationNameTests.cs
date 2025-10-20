using Toolbox.Extensions;
using Toolbox.Tools;
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
        result.IsError().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("abcedefh")]
    public void OptionOk(string? name)
    {
        IValidator<NameTestOption> validator = new Validator<NameTestOption>()
            .RuleFor(x => x.Name).ValidNameOption()
            .Build();

        var model = new NameTestOption
        {
            Name = name,
        };

        var result = validator.Validate(model);
        result.BeOk();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("~!(")]
    [InlineData("&")]
    [InlineData("a b")]
    [InlineData("a/b")]
    public void InvalidCharacters(string? value)
    {
        IValidator<NameTestOption> validator = new Validator<NameTestOption>()
            .RuleFor(x => x.Name).ValidNameOption()
            .Build();

        var model = new NameTestOption
        {
            Name = value,
        };

        var result = validator.Validate(model);
        result.BeError();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(1);
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
        result.BeOk();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(0);
    }

    [Theory]
    [InlineData("name")]
    [InlineData("abc-XYZ-123")]
    public void Option_ValidName_IsValid(string subject)
    {
        IValidator<NameTestOption> validator = new Validator<NameTestOption>()
            .RuleFor(x => x.Name).ValidNameOption()
            .Build();

        var model = new NameTestOption
        {
            Name = subject,
        };

        var result = validator.Validate(model);
        result.BeOk();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(0);
    }
}
