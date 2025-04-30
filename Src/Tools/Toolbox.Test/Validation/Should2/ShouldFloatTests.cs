//using Toolbox.Tools.Should;

//namespace Toolbox.Test.Validation.Should2;

//public class ShouldFloatTests
//{
//    [Fact]
//    public void Be()
//    {
//        float v1 = 10.4f;
//        v1.Should().Be(10.4f);

//        try
//        {
//            float v2 = 15.3f;
//            v2.Should().NotBe(15.3f);
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
//        float? v1 = 10.4f;
//        v1.Should().Be(10.4f);

//        try
//        {
//            float? v2 = 15.3f;
//            v2.Should().NotBe(15.3f);
//        }
//        catch (ArgumentException)
//        {
//            return;
//        }

//        throw new ArgumentException("Exception not thrown");
//    }
//}
