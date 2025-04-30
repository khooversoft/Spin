using Toolbox.Tools;

namespace Toolbox.Test.Validation.Should2;

public class ShouldBoolTests
{
    [Fact]
    public void BeTrue()
    {
        bool v1 = true;
        v1.Be(true);
        v1.BeTrue();

        Verify.Throw<ArgumentException>(() =>
        {
            bool v2 = false;
            v2.Be(true);
        });

        Verify.Throw<ArgumentException>(() =>
        {
            bool v2 = false;
            v2.BeTrue();
        });
    }

    [Fact]
    public void BeFalse()
    {
        bool v1 = false;
        v1.Be(false);
        v1.BeFalse();

        Verify.Throw<ArgumentException>(() =>
        {
            bool v2 = true;
            v2.Be(false);
        });

        Verify.Throw<ArgumentException>(() =>
        {
            bool v2 = true;
            v2.BeFalse();
        });
    }

    [Fact]
    public void BeTrueNullable()
    {
        bool? v1 = true;
        v1.Be(true);
        v1.BeTrue();

        Verify.Throw<ArgumentException>(() =>
        {
            bool? v2 = false;
            v2.Be(true);
        });

        Verify.Throw<ArgumentException>(() =>
        {
            bool? v2 = false;
            v2.BeTrue();
        });
    }

    [Fact]
    public void BeFalseNullable()
    {
        bool? v1 = false;
        v1.Be(false);
        v1.BeFalse();

        Verify.Throw<ArgumentException>(() =>
        {
            bool? v2 = true;
            v2.Be(false);
        });
        
        Verify.Throw<ArgumentException>(() =>
        {
            bool? v2 = true;
            v2.BeFalse();
        });
    }
}
