//using Toolbox.Tools;

//namespace Toolbox.Test.Validation.Should;

//public class ShouldIntTests
//{
//    [Fact]
//    public void Be()
//    {
//        int v1 = 0;
//        v1.Be(0);

//        try
//        {
//            int v2 = 5;
//            v2.NotBe(5);
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
//        int? v1 = 0;
//        v1.Be(0);

//        try
//        {
//            int? v2 = 5;
//            v2.NotBe(5);
//        }
//        catch (ArgumentException)
//        {
//            return;
//        }

//        throw new ArgumentException("Exception not thrown");
//    }

//    [Fact]
//    public void BeNull()
//    {
//        int? v1 = null;
//        v1.BeNull();

//        try
//        {
//            int? v2 = null;
//            v2.NotNull();
//        }
//        catch (ArgumentException)
//        {
//            return;
//        }

//        throw new ArgumentException("Exception not thrown");
//    }
//}
