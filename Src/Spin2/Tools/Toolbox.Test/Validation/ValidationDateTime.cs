using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Tools.Validation;

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

        ValidatorResult result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Count().Should().Be(1);
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

        ValidatorResult result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Count().Should().Be(1);

        model = new TimeDateTest
        {
            Value = new DateTime(2199, 12, 31).AddDays(1),
        };

        result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Count().Should().Be(1);
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

        ValidatorResult result = validator.Validate(model);
        result.IsValid.Should().BeTrue();
        result.Errors.Count().Should().Be(0);

        model = new TimeDateTest
        {
            Value = new DateTime(2199, 12, 31),
        };

        result = validator.Validate(model);
        result.IsValid.Should().BeTrue();
        result.Errors.Count().Should().Be(0);
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

        ValidatorResult result = validator.Validate(model);
        result.IsValid.Should().BeTrue();
        result.Errors.Count().Should().Be(0);
    }
}
