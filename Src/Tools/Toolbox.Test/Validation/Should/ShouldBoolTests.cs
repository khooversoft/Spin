using Toolbox.Tools.Should;

namespace Toolbox.Test.Validation.Should;

public class ShouldBoolTests
{
    [Fact]
    public void BeTrue()
    {
        bool v1 = true;
        v1.Should().BeTrue();

        try
        {
            bool v2 = false;
            v2.Should().BeTrue();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

    [Fact]
    public void BeFalse()
    {
        bool v1 = false;
        v1.Should().BeFalse();

        try
        {
            bool v2 = true;
            v2.Should().BeFalse();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

    [Fact]
    public void BeTrueNullable()
    {
        bool? v1 = true;
        v1.Should().BeTrue();

        try
        {
            bool? v2 = false;
            v2.Should().BeTrue();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

    [Fact]
    public void BeFalseNullable()
    {
        bool? v1 = false;
        v1.Should().BeFalse();

        try
        {
            bool? v2 = true;
            v2.Should().BeFalse();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

    [Fact]
    public void Be()
    {
        bool v1 = true;
        v1.Should().Be(true);

        try
        {
            bool v2 = false;
            v2.Should().Be(true);
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
        bool? v1 = true;
        v1.Should().Be(true);

        try
        {
            bool? v2 = false;
            v2.Should().Be(true);
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

    [Fact]
    public void NotBe()
    {
        bool v1 = false;
        v1.Should().BeFalse();

        try
        {
            bool v2 = true;
            v2.Should().BeFalse();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

    [Fact]
    public void NotBeNullable()
    {
        bool? v1 = false;
        v1.Should().BeFalse();

        try
        {
            bool? v2 = true;
            v2.Should().BeFalse();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

}
