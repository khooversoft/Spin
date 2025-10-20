using Toolbox.Tools;

namespace Toolbox.Test.Validation.ScalarValidationTests;

public class VerifyLongTests
{
    [Fact]
    public void Be()
    {
        long v1 = 0;
        v1.Be(0);

        Verify.Throw<ArgumentException>(() =>
        {
            long v2 = 5;
            v2.NotBe(5);
        });
    }

    [Fact]
    public void BeNullable()
    {
        long? v1 = 0;
        v1.Be(0);

        Verify.Throw<ArgumentException>(() =>
        {
            long? v2 = 5;
            v2.NotBe(5);
        });
    }

    [Fact]
    public void Be_Fails_And_NotBe_Succeeds()
    {
        Verify.Throw<ArgumentException>(() =>
        {
            long v = 1;
            v.Be(2);
        });

        long v2 = 1;
        v2.NotBe(2);
    }

    [Fact]
    public void Be_Fails_And_NotBe_Succeeds_Nullable()
    {
        Verify.Throw<ArgumentException>(() =>
        {
            long? v = 1;
            v.Be(2);
        });

        long? v2 = 1;
        v2.NotBe(2);
    }

    [Fact]
    public void NotNull_And_BeNull_ForNullable()
    {
        long? v1 = 1;
        v1.NotNull();

        Verify.Throw<ArgumentException>(() =>
        {
            long? v2 = 1;
            v2.BeNull();
        });

        long? v3 = null;
        v3.BeNull();

        Verify.Throw<ArgumentException>(() =>
        {
            long? v4 = null;
            v4.NotNull();
        });
    }
}
