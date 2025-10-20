using Toolbox.Tools;

namespace Toolbox.Test.Validation.ScalarValidationTests;

public class VerifyStringTests
{
    [Fact]
    public void Be()
    {
        string v1 = "hello";
        v1.Be("hello");
        v1.NotNull();
        v1.NotEmpty();

        Verify.Throw<ArgumentException>(() =>
        {
            string v2 = "hello";
            v2.NotBe("hello");
        });
    }

    [Fact]
    public void BeWithNull()
    {
        string? v1 = null;
        v1.Be(null);
        v1.BeNull();

        Verify.Throw<ArgumentException>(() =>
        {
            string? v2 = null;
            v2.NotBe(null);
        });

        Verify.Throw<ArgumentException>(() =>
        {
            string? v2 = null;
            v2.NotNull();
        });

        Verify.Throw<ArgumentException>(() =>
        {
            string? v2 = null;
            v2.NotEmpty();
        });
    }

    [Fact]
    public void BeNullable()
    {
        string? v1 = "hello";
        v1.Be("hello");
        v1.NotEmpty();

        Verify.Throw<ArgumentException>(() =>
        {
            string? v2 = "hello";
            v2.NotBe("hello");
        });
    }

    [Fact]
    public void NotBe_Succeeds_WhenDifferent()
    {
        string v1 = "hello";
        v1.NotBe("world");
    }

    [Fact]
    public void BeNull_Fails_WhenNotNull()
    {
        Verify.Throw<ArgumentException>(() =>
        {
            string? v = "value";
            v.BeNull();
        });
    }

    [Fact]
    public void NotEmpty_Fails_For_EmptyString()
    {
        Verify.Throw<ArgumentException>(() =>
        {
            string v = "";
            v.NotEmpty();
        });
    }
}
