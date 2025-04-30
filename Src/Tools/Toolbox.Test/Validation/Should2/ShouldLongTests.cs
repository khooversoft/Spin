using Toolbox.Tools;

namespace Toolbox.Test.Validation.Should2;

public class ShouldLongTests
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
}
