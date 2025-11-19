using Toolbox.Tools;

namespace Toolbox.Test.Validation.ScalarValidationTests;

public class VerifyIntTests
{
    [Fact]
    public void Be()
    {
        int v1 = 0;
        v1.Be(0);

        Verify.Throws<ArgumentException>(() =>
        {
            int v2 = 5;
            v2.NotBe(5);
        });
    }

    [Fact]
    public void BeNullable()
    {
        int? v1 = 0;
        v1.Be(0);

        Verify.Throws<ArgumentException>(() =>
        {
            int? v2 = 5;
            v2.NotBe(5);
        });
    }

    [Fact]
    public void BeNull()
    {
        int? v1 = null;
        v1.BeNull();

        Verify.Throws<ArgumentException>(() =>
        {
            int? v2 = null;
            v2.NotNull();
        });
    }

    [Fact]
    public void Be_Fails_And_NotBe_Succeeds()
    {
        Verify.Throws<ArgumentException>(() =>
        {
            int v = 1;
            v.Be(2);
        });

        int v2 = 1;
        v2.NotBe(2);
    }

    [Fact]
    public void Be_Fails_And_NotBe_Succeeds_Nullable()
    {
        Verify.Throws<ArgumentException>(() =>
        {
            int? v = 1;
            v.Be(2);
        });

        int? v2 = 1;
        v2.NotBe(2);
    }

    [Fact]
    public void NotNull_Succeeds()
    {
        int? v = 0;
        v.NotNull();
    }

    [Fact]
    public void BeNull_Fails_WhenHasValue()
    {
        Verify.Throws<ArgumentException>(() =>
        {
            int? v = 1;
            v.BeNull();
        });
    }
}
