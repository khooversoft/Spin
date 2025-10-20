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

    public record TimeDateOptionTest
    {
        public DateTime? Value { get; init; }
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

    [Fact]
    public void Option_NullValue_IsValid()
    {
        IValidator<TimeDateOptionTest> validator = new Validator<TimeDateOptionTest>()
            .RuleFor(x => x.Value).ValidDateTimeOption()
            .Build();

        var model = new TimeDateOptionTest
        {
            Value = null,
        };

        var result = validator.Validate(model);
        result.IsOk().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(0);
    }

    [Fact]
    public void Option_InvalidValue_IsError()
    {
        IValidator<TimeDateOptionTest> validator = new Validator<TimeDateOptionTest>()
            .RuleFor(x => x.Value).ValidDateTimeOption()
            .Build();

        var model = new TimeDateOptionTest
        {
            Value = new DateTime(1899, 12, 31),
        };

        var result = validator.Validate(model);
        result.IsError().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(1);
    }

    [Fact]
    public void CustomMessage_PropagatesToError()
    {
        const string customMessage = "My custom date validation message";

        IValidator<TimeDateTest> validator = new Validator<TimeDateTest>()
            .RuleFor(x => x.Value).ValidDateTime(customMessage)
            .Build();

        var model = new TimeDateTest { Value = DateTime.MinValue };

        var result = validator.Validate(model);
        result.IsBadRequest().BeTrue();

        var vr = result.Return().Cast<ValidatorResult>();
        var errors = vr.GetErrors();
        errors.Count.Be(1);
        errors[0].Message.Be(customMessage);
    }

    [Fact]
    public void ErrorShape_ContainsTypeAndProperty()
    {
        IValidator<TimeDateTest> validator = new Validator<TimeDateTest>()
            .RuleFor(x => x.Value).ValidDateTime()
            .Build();

        var model = new TimeDateTest { Value = DateTime.MaxValue };

        var result = validator.Validate(model);
        result.IsBadRequest().BeTrue();

        var vr = result.Return().Cast<ValidatorResult>();
        var errors = vr.GetErrors();
        errors.Count.Be(1);

        var e = errors[0];
        e.TypeName.Be("TimeDateTest");
        e.Name.Be("Value");
        e.Message.NotEmpty();
    }

    [Fact]
    public void Option_ValidValue_IsOk()
    {
        IValidator<TimeDateOptionTest> validator = new Validator<TimeDateOptionTest>()
            .RuleFor(x => x.Value).ValidDateTimeOption()
            .Build();

        var model = new TimeDateOptionTest
        {
            Value = new DateTime(2000, 1, 1),
        };

        var result = validator.Validate(model);
        result.IsOk().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(0);
    }
}
