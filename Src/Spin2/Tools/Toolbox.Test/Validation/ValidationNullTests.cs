using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Tools.Validation;

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

        ValidatorResult result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Count().Should().Be(1);
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

        ValidatorResult result = validator.Validate(model);
        result.IsValid.Should().BeTrue();
        result.Errors.Count().Should().Be(0);
    }
}
