using Toolbox.Tools;

namespace Toolbox.Test.Tools;

public class StandardValidationTests
{
    [Theory]
    [InlineData("a")]
    [InlineData("a1")]
    [InlineData("a1-")]
    [InlineData("a1-/")]
    [InlineData("a1-/:")]
    [InlineData("a1-/:@")]
    [InlineData("a1-/:@.")]
    public void NameValidationTest(string value)
    {
        bool result = StandardValidation.IsName(value);
        result.BeTrue(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1")]
    [InlineData("a!")]
    [InlineData("a#")]
    [InlineData("a$")]
    [InlineData("a%")]
    [InlineData("a&")]
    [InlineData("a*")]
    [InlineData("a(")]
    [InlineData("a)")]
    [InlineData("a[")]
    [InlineData("a]")]
    [InlineData("a;")]
    [InlineData("a'")]
    [InlineData("a\"")]
    [InlineData("a<")]
    [InlineData("a>")]
    [InlineData("a?")]
    [InlineData("a,")]
    [InlineData("a`")]
    [InlineData("a~")]
    [InlineData("a\\")]
    [InlineData("a|")]
    public void NameValidationFailTest(string? value)
    {
        bool result = StandardValidation.IsName(value!);
        result.BeFalse(value);
    }

    [Theory]
    [InlineData("1234567890abcedfghijklmnopqurstuvxyzABCDEFGHIJKLMNOPQRSTUVWXYZ !@#$%^&*()-_=+[{]}\\|:;\"'<,>.?/~`")]
    public void DescriptionValidationTest(string value)
    {
        bool result = StandardValidation.IsDescription(value);
        result.BeTrue(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("\r")]
    [InlineData("\n")]
    [InlineData("\t")]
    public void DescriptionValidationFailTest(string? value)
    {
        bool result = StandardValidation.IsDescription(value!);
        result.BeFalse(value);
    }
}
