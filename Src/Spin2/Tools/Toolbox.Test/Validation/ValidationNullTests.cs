using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Tools.Validation;
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
        result.IsError().Should().BeTrue();
        result.Return().As<ValidatorResult>().Errors.Count().Should().Be(1);
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
        result.IsOk().Should().BeTrue();
        result.Return().As<ValidatorResult>().Errors.Count().Should().Be(0);
    }
}
