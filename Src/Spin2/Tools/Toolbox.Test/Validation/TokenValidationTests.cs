using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tools.Validation.Token;
using Toolbox.Types.Maybe;

namespace Toolbox.Test.Validation;

public class TokenValidationTests
{
    private static TokenValidator _validator = new TokenValidator()
        .Add(new TokenValidatorTest(TokenValidatorType.First, x => char.IsLetterOrDigit(x)))
        .Add(new TokenValidatorTest(TokenValidatorType.First, x => x == '.'))
        .Add(new TokenValidatorLetterNumber(TokenValidatorType.Middle))
        .Add(new TokenValidatorTest(TokenValidatorType.Middle, x => char.IsLetterOrDigit(x)))
        .Add(new TokenValidatorTest(TokenValidatorType.Last, x => char.IsLetterOrDigit(x)));

    [Fact]
    public void TestEmptyOrNullToken()
    {
        _validator.Validate(null).Should().BeFalse();
        _validator.Validate("").Should().BeFalse();

        new TokenValidator().Action(x =>
        {
            x.Validate(null).Should().BeTrue();
            x.Validate("").Should().BeTrue();
        });
    }

    [Fact]
    public void TestSimplePatterns()
    {
        _validator.Validate("a").Should().BeTrue();
        _validator.Validate("1").Should().BeTrue();
        _validator.Validate(".").Should().BeTrue();

        _validator.Validate("a1").Should().BeTrue();
        _validator.Validate("1a").Should().BeTrue();
        _validator.Validate(".a").Should().BeTrue();
        _validator.Validate("b.").Should().BeFalse();
        _validator.Validate("1.").Should().BeFalse();
    }
}
