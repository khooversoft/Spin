using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Validation;

public class ValidationNullTests
{
    public record ReferenceClass(string name);

    private record NullTest
    {
        public ReferenceClass Value { get; init; } = null!;
    }

    [Fact]

    public void NotNullFail()
    {
        IValidator<NullTest> validator = new Validator<NullTest>()
            .RuleFor(x => x.Value).NotNull()
            .Build();

        var model = new NullTest
        {
            Value = null!,
        };

        var result = validator.Validate(model);
        result.IsError().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(1);
    }

    [Fact]
    public void ValidReferencePass()
    {
        IValidator<NullTest> validator = new Validator<NullTest>()
            .RuleFor(x => x.Value).NotNull()
            .Build();

        var model = new NullTest
        {
            Value = new ReferenceClass("name"),
        };

        var result = validator.Validate(model);
        result.IsOk().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(0);
    }
}
