using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Test.Validation;

public class ValidationNameTests
{
    private record NameTest
    {
        public string Name { get; set; } = null!;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("~!(")]
    [InlineData("&")]

    public void NullAndInvalidCharacters(string value)
    {
        IValidator<NameTest> validator = new Validator<NameTest>()
            .RuleFor(x => x.Name).ValidName()
            .Build();

        var model = new NameTest
        {
            Name = value,
        };

        var result = validator.Validate(model);
        result.IsError().Should().BeTrue();
        result.Return().As<ValidatorResult>().Errors.Count().Should().Be(1);
    }

    [Theory]
    [InlineData("name")]
    [InlineData("abcedefhijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890-$_E")]
    public void NameIsValid(string subject)
    {
        IValidator<NameTest> validator = new Validator<NameTest>()
            .RuleFor(x => x.Name).ValidName()
            .Build();

        var model = new NameTest
        {
            Name = subject
        };

        var result = validator.Validate(model);
        result.IsOk().Should().BeTrue();
        result.Return().As<ValidatorResult>().Errors.Count().Should().Be(0);
    }
}
