using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Tools.Validation;

namespace Toolbox.Test.Validation;

public class ValidationObjectIdTests
{
    private record ObjectIdTest
    {
        public string Name { get; set; } = null!;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("~!(")]
    [InlineData("&")]
    [InlineData("schema")]

    public void Invalid(string objectId)
    {
        IValidator<ObjectIdTest> validator = new Validator<ObjectIdTest>()
            .RuleFor(x => x.Name).ValidObjectId()
            .Build();

        var model = new ObjectIdTest
        {
            Name = objectId,
        };

        ValidatorResult result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Count().Should().Be(1);
    }

    [Theory]
    [InlineData("name")]
    [InlineData("abcedefhijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890._$@_*/abcedefhijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890._$@_*")]
    [InlineData("schema/tenant")]
    [InlineData("schema/tenant/path2")]
    public void ObjectIdIsValid(string objectId)
    {
        IValidator<ObjectIdTest> validator = new Validator<ObjectIdTest>()
            .RuleFor(x => x.Name).ValidObjectId()
            .Build();

        var model = new ObjectIdTest
        {
            Name = objectId
        };
    }
}
