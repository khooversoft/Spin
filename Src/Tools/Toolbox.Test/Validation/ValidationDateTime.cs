using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Validation;

public class ValidationDateTime
{
    public record TimeDateTest
    {
        public DateTime Value { get; init; }
    }

    [Fact]
    public void DefaultFail()
    {
        IValidator<TimeDateTest> validator = new Validator<TimeDateTest>()
            .RuleFor(x => x.Value).ValidDateTime()
            .Build();

        var model = new TimeDateTest
        {
            Value = default!,
        };

        var result = validator.Validate(model);
        result.IsError().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(1);
    }


    [Fact]
    public void ExceedLowerAndUpperLimit()
    {
        IValidator<TimeDateTest> validator = new Validator<TimeDateTest>()
            .RuleFor(x => x.Value).ValidDateTime()
            .Build();

        var model = new TimeDateTest
        {
            Value = new DateTime(1900, 1, 1).AddDays(-1),
        };

        var result = validator.Validate(model);
        result.IsError().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(1);

        model = new TimeDateTest
        {
            Value = new DateTime(2199, 12, 31).AddDays(1),
        };

        result = validator.Validate(model);
        result.IsError().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(1);
    }

    [Fact]
    public void LowerAndUpperLimit()
    {
        IValidator<TimeDateTest> validator = new Validator<TimeDateTest>()
            .RuleFor(x => x.Value).ValidDateTime()
            .Build();

        var model = new TimeDateTest
        {
            Value = new DateTime(1900, 1, 1),
        };

        var result = validator.Validate(model);
        result.IsOk().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(0);

        model = new TimeDateTest
        {
            Value = new DateTime(2199, 12, 31),
        };

        result = validator.Validate(model);
        result.IsOk().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(0);
    }

    [Fact]
    public void NormalDate()
    {
        IValidator<TimeDateTest> validator = new Validator<TimeDateTest>()
            .RuleFor(x => x.Value).ValidDateTime()
            .Build();

        var model = new TimeDateTest
        {
            Value = DateTime.Now,
        };

        var result = validator.Validate(model);
        result.IsOk().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(0);
    }
}
