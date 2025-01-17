using Toolbox.Tools.Should;

namespace Toolbox.Test.Validation.Should;

public class ShouldObjectTests
{
    [Fact]
    public void Be()
    {
        object v1 = "hello";
        v1.Should().Be("hello");

        try
        {
            object v2 = "hello";
            v2.Should().NotBe("hello");
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

    [Fact]
    public void BeNull()
    {
        object? v1 = null;
        v1.Should().Be(null);

        try
        {
            object v2 = "hello";
            v2.Should().NotBe("hello");
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

    [Fact]
    public void ObjectBeNull()
    {
        object v1 = null!;
        v1.Should().BeNull();

        try
        {
            object v2 = "hello";
            v2.Should().BeNull();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

    [Fact]
    public void ObjectNotBeNull()
    {
        object v1 = "dkd";
        v1.Should().NotBeNull();

        try
        {
            object v2 = null!;
            v2.Should().NotBeNull();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

    [Fact]
    public void ObjectBeNullNullable()
    {
        object? v1 = null!;
        v1.Should().BeNull();

        try
        {
            object? v2 = "hello";
            v2.Should().BeNull();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }

    [Fact]
    public void ObjectNotBeNullNullable()
    {
        object? v1 = "dkd";
        v1.Should().NotBeNull();

        try
        {
            object? v2 = null!;
            v2.Should().NotBeNull();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new ArgumentException("Exception not thrown");
    }
}
