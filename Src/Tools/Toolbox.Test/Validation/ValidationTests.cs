using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Test.Validation;

public record TestOption
{
    public string DomainName { get; init; } = null!;
    public string AccountName { get; init; } = null!;
    public int Value { get; init; }
    public IReadOnlyList<string> ScalarValues { get; init; } = Array.Empty<string>();
    public IReadOnlyList<SubOption> SubOptions { get; init; } = Array.Empty<SubOption>();
    public string ObjectId { get; init; } = null!;
    public DateTime DateTime { get; init; }
}

public record SubOption
{
    public string Name { get; init; } = null!;
    public int Value { get; init; }
}

public class ValidationTests
{
    [Fact]
    public void TestValidationShouldPass()
    {
        var subValidation = new Validator<SubOption>();
        subValidation.RuleFor(x => x.Name).NotEmpty().Must(x => x.EndsWith("name"), x => $"{x} does not end with 'name'");
        subValidation.RuleFor(x => x.Value).Must(x => x >= 0, x => $"{x} must >= 0");

        var validations = new Validator<TestOption>();

        validations.RuleFor(x => x.DomainName).NotEmpty().ValidName();
        validations.RuleFor(x => x.AccountName).NotEmpty();
        validations.RuleFor(x => x.Value).Must(x => x == 1, _ => "must be one");
        validations.RuleFor(x => x.ScalarValues).NotNull();
        validations.RuleForEach(x => x.SubOptions).Validate(subValidation);
        validations.RuleForEach(x => x.ScalarValues).NotEmpty();
        validations.RuleForObject(x => x).Must(x => x.DomainName == "domain", _ => "'domain' is required for DomainName");
        validations.RuleFor(x => x.ObjectId).NotEmpty();

        var option = new TestOption
        {
            DomainName = "domain",
            AccountName = "accountName",
            Value = 1,
            ScalarValues = Enumerable.Range(0, 5).Select(x => $"Item {x}").ToArray(),
            SubOptions = Enumerable.Range(0, 3).Select(x => new SubOption { Name = $"{x} name", Value = x }).ToArray(),
            ObjectId = "schema/tenant/path",
            DateTime = DateTime.Now,
        };

        var result = validations.Validate(option);
        result.NotNull();
        result.IsOk().Should().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Should().Be(0);
    }

    [Fact]
    public void TestValidationFluentShouldPass()
    {
        var subValidation = new Validator<SubOption>();
        subValidation.RuleFor(x => x.Name).NotEmpty().Must(x => x.EndsWith("name"), x => $"{x} does not end with 'name'");
        subValidation.RuleFor(x => x.Value).Must(x => x >= 0, x => $"{x} must >= 0");

        var validations = new Validator<TestOption>()
            .RuleFor(x => x.DomainName).NotEmpty().ValidName()
            .RuleFor(x => x.AccountName).NotEmpty()
            .RuleFor(x => x.Value).Must(x => x == 1, _ => "must be one")
            .RuleFor(x => x.ScalarValues).NotNull()
            .RuleForObject(x => x).Must(x => x.DomainName == "domain", _ => "'domain' is required for DomainName")
            .RuleForEach(x => x.SubOptions).Validate(subValidation)
            .RuleForEach(x => x.ScalarValues).NotEmpty()
            .RuleFor(x => x.ObjectId).NotNull().NotEmpty()
            .Build();

        var option = new TestOption
        {
            DomainName = "domain",
            AccountName = "accountName",
            Value = 1,
            ScalarValues = Enumerable.Range(0, 5).Select(x => $"Item {x}").ToArray(),
            SubOptions = Enumerable.Range(0, 3).Select(x => new SubOption { Name = $"{x} name", Value = x }).ToArray(),
            ObjectId = "schema/tenant/path",
            DateTime = DateTime.Now,
        };

        var result = validations.Validate(option);
        result.NotNull();
        result.IsOk().Should().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Should().Be(0);
    }

    [Fact]
    public void TestValidationShouldFail()
    {
        var subValidation = new Validator<SubOption>();
        subValidation.RuleFor(x => x.Name).NotEmpty().Must(x => x.EndsWith("name"), x => $"{x} does not end with 'name'");
        subValidation.RuleFor(x => x.Value).Must(x => x == 2, x => $"{x} must >= 0");

        var validations = new Validator<TestOption>();

        validations.RuleFor(x => x.DomainName).NotEmpty();
        validations.RuleFor(x => x.AccountName).NotEmpty();
        validations.RuleFor(x => x.Value).Must(x => x == 1, _ => "must be one");
        validations.RuleFor(x => x.ScalarValues).NotNull();
        validations.RuleForEach(x => x.ScalarValues).NotEmpty();
        validations.RuleForEach(x => x.SubOptions).Validate(subValidation);

        var option = new TestOption
        {
            DomainName = "domain",
            AccountName = "accountName",
            Value = 1,
            ScalarValues = Enumerable.Range(0, 5).Select(x => $"Item {x}").ToArray(),
            SubOptions = Enumerable.Range(0, 3).Select(x => new SubOption { Name = $"{x} name", Value = x }).ToArray(),
        };

        var result = validations.Validate(option);
        result.NotNull();
        result.IsError().Should().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Should().Be(2);
    }

    [Fact]
    public void TestValidationNotEmpty()
    {
        var validations = new Validator<TestOption>()
            .RuleFor(x => x.DomainName).NotEmpty()
            .RuleFor(x => x.AccountName).NotEmpty()
            .RuleFor(x => x.Value).Must(x => x == 1, _ => "must be one")
            .Build();

        var option = new TestOption
        {
            DomainName = "domain",
        };

        var result = validations.Validate(option);
        result.NotNull();
        result.IsError().Should().BeTrue();
        result.Return().Cast<ValidatorResult>().Errors.Count().Should().Be(2);
    }

    [Fact]
    public void TestValidationMust()
    {
        var validations = new Validator<TestOption>()
            .RuleFor(x => x.DomainName).NotEmpty().Must(x => x == "domain" ? StatusCode.OK : (StatusCode.Conflict, "domain is required"))
            .RuleFor(x => x.AccountName).NotEmpty().Must(x => x == "accountName", _ => "accountName is required")
            .Build();

        new TestOption
        {
            DomainName = "domain",
            AccountName = "accountName",
        }.Action(x =>
        {
            var result = validations.Validate(x);
            result.NotNull();
            result.IsOk().Should().BeTrue(result.ToString());
            result.Return().Cast<ValidatorResult>().Errors.Count().Should().Be(0);
        });

        new TestOption
        {
            DomainName = "domain-not",
            AccountName = "accountName",
        }.Action(x =>
        {
            var result = validations.Validate(x);
            result.NotNull();
            result.IsError().Should().BeTrue();
            result.Return().Cast<ValidatorResult>().Errors.Count().Should().Be(1);
            result.Return().Cast<ValidatorResult>().Errors[0].Cast<ValidatorError>().Message.Should().Be("domain is required");
        });

        new TestOption
        {
            DomainName = "domain",
            AccountName = "accountName-not",
        }.Action(x =>
        {
            var result = validations.Validate(x);
            result.NotNull();
            result.IsError().Should().BeTrue();
            result.Return().Cast<ValidatorResult>().Errors.Count().Should().Be(1);
            result.Return().Cast<ValidatorResult>().Errors[0].Cast<ValidatorError>().Message.Should().Be("accountName is required");
        });

        new TestOption
        {
            DomainName = "domain-not",
            AccountName = "accountName-not",
        }.Action(x =>
        {
            var result = validations.Validate(x);
            result.NotNull();
            result.IsError().Should().BeTrue();
            result.Return().Cast<ValidatorResult>().Errors.Count().Should().Be(2);
            result.Return().Cast<ValidatorResult>().Errors[0].Cast<ValidatorError>().Message.Should().Be("domain is required");
            result.Return().Cast<ValidatorResult>().Errors[1].Cast<ValidatorError>().Message.Should().Be("accountName is required");
        });
    }
}
