using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Tools.Validation;

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

        ValidatorResult result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Count().Should().Be(1);
    }

    [Theory]
    [InlineData("name")]
    [InlineData("signKey@test.com")]
    [InlineData("abcedefhijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890._$@_*")]
    public void NameIsValid(string subject)
    {
        IValidator<NameTest> validator = new Validator<NameTest>()
            .RuleFor(x => x.Name).ValidName()
            .Build();

        var model = new NameTest
        {
            Name = subject
        };

        ValidatorResult result = validator.Validate(model);
        result.IsValid.Should().BeTrue();
        result.Errors.Count().Should().Be(0);
    }
}
