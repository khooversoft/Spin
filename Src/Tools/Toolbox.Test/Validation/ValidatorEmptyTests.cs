using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Test.Validation;

public class ValidatorEmptyTests
{
    private record EmptyTest
    {
        public string Value { get; init; } = null!;
    }

    [Fact]
    public void NotNullFail()
    {
        IValidator<EmptyTest> validator = new Validator<EmptyTest>()
            .RuleFor(x => x.Value).NotEmpty()
            .Build();

        var model = new EmptyTest
        {
            Value = null!,
        };

        Option<IValidatorResult> result = validator.Validate(model);
        result.IsError().Should().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Should().Be(1);
    }


    [Fact]
    public void NotEmptyFail()
    {
        IValidator<EmptyTest> validator = new Validator<EmptyTest>()
            .RuleFor(x => x.Value).NotEmpty()
            .Build();

        var model = new EmptyTest
        {
            Value = "",
        };

        var result = validator.Validate(model);
        result.IsError().Should().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Should().Be(1);
    }

    [Fact]
    public void NotEmptyPass()
    {
        IValidator<EmptyTest> validator = new Validator<EmptyTest>()
            .RuleFor(x => x.Value).NotEmpty()
            .Build();

        var model = new EmptyTest
        {
            Value = "test",
        };

        var result = validator.Validate(model);
        result.IsOk().Should().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Should().Be(0);
    }
}
