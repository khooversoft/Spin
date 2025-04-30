//namespace Toolbox.Test.Validation.Should;

//public class ShouldActionTests
//{
//    [Fact]
//    public void NotThrow()
//    {
//        Action v1 = () => { };
//        v1.NotThrow();

//        try
//        {
//            Action v2 = () => throw new ArgumentException();
//            v2.NotThrow();
//        }
//        catch (ArgumentException)
//        {
//            return;
//        }

//        throw new ArgumentException("Exception not thrown");
//    }

//    [Fact]
//    public void Throw()
//    {
//        Action v1 = () => throw new ArgumentException();
//        v1.Throw();

//        try
//        {
//            Action v2 = () => { };
//            v2.Throw();
//        }
//        catch (ArgumentException)
//        {
//            return;
//        }

//        throw new ArgumentException("Exception not thrown");
//    }

//    [Fact]
//    public void ThrowSpecific()
//    {
//        Action v1 = () => throw new ArgumentException();
//        v1.Throw<ArgumentException>();

//        try
//        {
//            Action v2 = () => throw new ArgumentException();
//            v2.Throw<InvalidCastException>();
//        }
//        catch (ArgumentException)
//        {
//            return;
//        }

//        throw new ArgumentException("Exception not thrown");
//    }

//}
