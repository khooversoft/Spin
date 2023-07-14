using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Tools.Validation;

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

        ValidatorResult result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Count().Should().Be(1);
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

        ValidatorResult result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Count().Should().Be(1);
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

        ValidatorResult result = validator.Validate(model);
        result.IsValid.Should().BeTrue();
        result.Errors.Count().Should().Be(0);
    }
}
