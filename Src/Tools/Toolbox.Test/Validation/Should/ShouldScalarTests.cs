using Toolbox.Tools.Should;

namespace Toolbox.Test.Validation.Should;

public class ShouldScalarTests
{

    [Fact]
    public void BeLongNumberr()
    {
        long v1 = 10;
        v1.Should().Be(10);

        try
        {
            long v2 = 15;
            v2.Should().NotBe(15);
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

}
