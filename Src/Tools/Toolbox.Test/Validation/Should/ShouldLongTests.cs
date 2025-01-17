using Toolbox.Tools.Should;

namespace Toolbox.Test.Validation.Should;

public class ShouldLongTests
{
    [Fact]
    public void Be()
    {
        long v1 = 0;
        v1.Should().Be(0);

        try
        {
            long v2 = 5;
            v2.Should().NotBe(5);
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

    [Fact]
    public void BeNullable()
    {
        long? v1 = 0;
        v1.Should().Be(0);

        try
        {
            long? v2 = 5;
            v2.Should().NotBe(5);
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }
}
