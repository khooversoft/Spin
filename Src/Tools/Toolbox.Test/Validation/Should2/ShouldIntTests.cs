using Toolbox.Tools;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Validation.Should2;

public class ShouldIntTests
{
    [Fact]
    public void Be()
    {
        int v1 = 0;
        v1.Should().Be(0);

        Verify.Throw<ArgumentException>(() =>
        {
            int v2 = 5;
            v2.Should().NotBe(5);
        });
    }

    [Fact]
    public void BeNullable()
    {
        int? v1 = 0;
        v1.Should().Be(0);

        Verify.Throw<ArgumentException>(() =>
        {
            int? v2 = 5;
            v2.Should().NotBe(5);
        });
    }

    [Fact]
    public void BeNull()
    {
        int? v1 = null;
        v1.BeNull();

        Verify.Throw<ArgumentException>(() =>
        {
            int? v2 = null;
            v2.NotNull();
        });
    }
}
