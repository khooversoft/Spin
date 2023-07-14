using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Test.Types;
using Toolbox.Tools.Validation;

namespace Toolbox.Test.Validation;

public class ValidationLinkTests
{
    public record SubClass(string Name, int Value);

    private record PrimaryClass
    {
        public string Name { get; set; } = null!;
        public SubClass SubClass { get; set; } = null!;
    }

    [Fact]
    public void InvalidValues()
    {
        IValidator<SubClass> subClassValidator = new Validator<SubClass>()
            .RuleFor(x => x.Name).NotEmpty()
            .RuleFor(x => x.Value).Must(x => x > 5, _ => "bad value")
            .Build();

        IValidator<PrimaryClass> validator = new Validator<PrimaryClass>()
            .RuleFor(x => x.Name).ValidObjectId()
            .RuleFor(x => x.SubClass).Validate(subClassValidator)
            .Build();

        var model = new PrimaryClass
        {
            Name = "name",
            SubClass = null!
        };

        ValidatorResult result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Count().Should().Be(2);

        model = new PrimaryClass
        {
            Name = "schema/name",
            SubClass = new SubClass("name1", 5),
        };

        result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Count().Should().Be(1);
    }

    [Fact]
    public void ValidValues()
    {
        IValidator<SubClass> subClassValidator = new Validator<SubClass>()
            .RuleFor(x => x.Name).NotEmpty()
            .RuleFor(x => x.Value).Must(x => x > 5, _ => "bad value")
            .Build();

        IValidator<PrimaryClass> validator = new Validator<PrimaryClass>()
            .RuleFor(x => x.Name).ValidObjectId()
            .RuleFor(x => x.SubClass).Validate(subClassValidator)
            .Build();

        var model = new PrimaryClass
        {
            Name = "schema/name",
            SubClass = new SubClass("name2", 101),
        };

        ValidatorResult result = validator.Validate(model);
        result.IsValid.Should().BeTrue();
        result.Errors.Count().Should().Be(0);
    }
}
