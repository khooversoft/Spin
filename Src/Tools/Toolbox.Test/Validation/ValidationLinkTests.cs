using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

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
            .RuleFor(x => x.Name).ValidResourceId(ResourceType.DomainOwned)
            .RuleFor(x => x.SubClass).Validate(subClassValidator)
            .Build();

        var model = new PrimaryClass
        {
            Name = "name",
            SubClass = null!
        };

        var result = validator.Validate(model);
        result.IsOk().BeFalse();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(2);

        model = new PrimaryClass
        {
            Name = "schema:tenant.com/name",
            SubClass = new SubClass("name1", 5),
        };

        result = validator.Validate(model);
        result.IsOk().BeFalse();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(1);
    }

    [Fact]
    public void ValidValues()
    {
        IValidator<SubClass> subClassValidator = new Validator<SubClass>()
            .RuleFor(x => x.Name).NotEmpty()
            .RuleFor(x => x.Value).Must(x => x > 5, _ => "bad value")
            .Build();

        IValidator<PrimaryClass> validator = new Validator<PrimaryClass>()
            .RuleFor(x => x.Name).ValidResourceId(ResourceType.DomainOwned)
            .RuleFor(x => x.SubClass).Validate(subClassValidator)
            .Build();

        var model = new PrimaryClass
        {
            Name = "schema:tenant.com/name",
            SubClass = new SubClass("name2", 101),
        };

        var result = validator.Validate(model);
        result.IsOk().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count.Be(0);
    }

    [Fact]
    public void ChildValidator_AggregatesMultipleErrors()
    {
        IValidator<SubClass> subClassValidator = new Validator<SubClass>()
            .RuleFor(x => x.Name).NotEmpty()
            .RuleFor(x => x.Value).Must(x => x > 5, _ => "bad value")
            .Build();

        IValidator<PrimaryClass> validator = new Validator<PrimaryClass>()
            .RuleFor(x => x.Name).ValidResourceId(ResourceType.DomainOwned)
            .RuleFor(x => x.SubClass).Validate(subClassValidator)
            .Build();

        var model = new PrimaryClass
        {
            Name = "schema:tenant.com/name",
            SubClass = new SubClass("", 0),
        };

        var result = validator.Validate(model);
        result.IsOk().BeFalse();
        result.Return().Cast<ValidatorResult>().Errors.Count().Be(2);
    }
}
