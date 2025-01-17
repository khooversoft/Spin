using Toolbox.Tools.Should;

namespace Toolbox.Test.Validation.Should;

public class ShouldActionTests
{
    [Fact]
    public void NotThrow()
    {
        Action v1 = () => { };
        v1.Should().NotThrow();

        try
        {
            Action v2 = () => throw new ArgumentException();
            v2.Should().NotThrow();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

    [Fact]
    public void Throw()
    {
        Action v1 = () => throw new ArgumentException();
        v1.Should().Throw();

        try
        {
            Action v2 = () => { };
            v2.Should().Throw();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

    [Fact]
    public void ThrowSpecific()
    {
        Action v1 = () => throw new ArgumentException();
        v1.Should().Throw<ArgumentException>();

        try
        {
            Action v2 = () => throw new ArgumentException();
            v2.Should().Throw<InvalidCastException>();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

}
