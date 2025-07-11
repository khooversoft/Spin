//using Toolbox.Tools;

//namespace Toolbox.Test.Validation.Should;

//public class ShouldStringTests
//{
//    [Fact]
//    public void Be()
//    {
//        string v1 = "hello";
//        v1.Be("hello");

//        try
//        {
//            string v2 = "hello";
//            v2.NotBe("hello");
//        }
//        catch (ArgumentException)
//        {
//            return;
//        }

//        throw new ArgumentException("Exception not thrown");
//    }

//    [Fact]
//    public void BeNullable()
//    {
//        string? v1 = "hello";
//        v1.Be("hello");

//        try
//        {
//            string? v2 = "hello";
//            v2.NotBe("hello");
//        }
//        catch (ArgumentException)
//        {
//            return;
//        }

//        throw new ArgumentException("Exception not thrown");
//    }


//    [Fact]
//    public void BeNullableWithNull()
//    {
//        string? v1 = null;
//        v1.Be(null);

//        try
//        {
//            string? v2 = null;
//            v2.NotBe(null);
//        }
//        catch (ArgumentException)
//        {
//            return;
//        }

//        throw new ArgumentException("Exception not thrown");
//    }

//    [Fact]
//    public void StringBeNull()
//    {
//        string v1 = null!;
//        v1.BeNull();

//        try
//        {
//            string v2 = "hello";
//            v2.BeNull();
//        }
//        catch (ArgumentException)
//        {
//            return;
//        }

//        throw new ArgumentException("Exception not thrown");
//    }

//    [Fact]
//    public void StringNotBeNull()
//    {
//        string v1 = "hello"!;
//        v1.NotNull();

//        try
//        {
//            string v2 = null!;
//            v2.NotNull();
//        }
//        catch (ArgumentException)
//        {
//            return;
//        }

//        throw new ArgumentException("Exception not thrown");
//    }

//    [Fact]
//    public void StringBeEmpty()
//    {
//        string v1 = null!;
//        v1.BeEmpty();

//        try
//        {
//            string v2 = "hello";
//            v2.BeEmpty();
//        }
//        catch (ArgumentException)
//        {
//            return;
//        }

//        throw new ArgumentException("Exception not thrown");
//    }

//    [Fact]
//    public void StringBeNullNullable()
//    {
//        string? v1 = null!;
//        v1.BeNull();

//        try
//        {
//            string? v2 = "hello";
//            v2.BeNull();
//        }
//        catch (ArgumentException)
//        {
//            return;
//        }

//        throw new ArgumentException("Exception not thrown");
//    }

//    [Fact]
//    public void StringNotBeNullNullable()
//    {
//        string? v1 = "dkd";
//        v1.NotNull();

//        try
//        {
//            string? v2 = null!;
//            v2.NotNull();
//        }
//        catch (ArgumentException)
//        {
//            return;
//        }

//        throw new ArgumentException("Exception not thrown");
//    }

//    [Fact]
//    public void StringStartsWith()
//    {
//        string v1 = "This is a test";
//        v1.StartsWith("This").BeTrue();

//        try
//        {
//            string v2 = "This is a test";
//            v2.StartsWith("next").BeTrue();
//        }
//        catch (ArgumentException)
//        {
//            return;
//        }

//        throw new ArgumentException("Exception not thrown");
//    }

//    [Fact]
//    public void StringStartsWithNullable()
//    {
//        string? v1 = "This is a test";
//        v1.StartsWith("This").BeTrue();

//        try
//        {
//            string? v2 = "This is a test";
//            v2.StartsWith("next").BeTrue();
//        }
//        catch (ArgumentException)
//        {
//            return;
//        }

//        throw new ArgumentException("Exception not thrown");
//    }
//}
