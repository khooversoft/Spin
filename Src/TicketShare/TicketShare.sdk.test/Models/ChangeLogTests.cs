//using Toolbox.Extensions;
//using Toolbox.Tools.Should;
//using Toolbox.Types;

//namespace TicketShare.sdk.test.Models;

//public class ChangeLogTests
//{
//    [Fact]
//    public void Serialization1()
//    {
//        var p1 = new ChangeLog
//        {
//            ChangedByPrincipalId = "key1",
//            SeatId = "desc1",
//            PropertyName = "propertyName1",
//            OldValue = "oldValue1",
//            NewValue = "newValue",
//        };

//        p1.Validate().IsOk().Should().BeTrue();
//        string j1 = p1.ToJson();
//        var p2 = j1.ToObject<ChangeLog>();
//        (p1 == p2).Should().BeTrue();
//    }

//    [Fact]
//    public void Compare1()
//    {
//        DateTime dt = DateTime.Now;
//        string id = Guid.NewGuid().ToString();

//        var p1 = new ChangeLog
//        {
//            Id = id,
//            Date = dt,
//            ChangedByPrincipalId = "key1",
//            SeatId = "desc1",
//            PropertyName = "propertyName1",
//            OldValue = "oldValue1",
//            NewValue = "newValue",
//        };

//        var p2 = new ChangeLog
//        {
//            Id = id,
//            Date = dt,
//            ChangedByPrincipalId = "key1",
//            SeatId = "desc1",
//            PropertyName = "propertyName1",
//            OldValue = "oldValue1",
//            NewValue = "newValue",
//        };

//        (p1 == p2).Should().BeTrue();
//    }

//    [Fact]
//    public void Compare2()
//    {
//        DateTime dt = DateTime.Now;
//        string id = Guid.NewGuid().ToString();

//        var p1 = new ChangeLog
//        {
//            Id = id,
//            Date = dt,
//            ChangedByPrincipalId = "key1",
//            SeatId = "desc1",
//        };

//        var p2 = new ChangeLog
//        {
//            Id = id,
//            Date = dt,
//            ChangedByPrincipalId = "key1",
//            SeatId = "desc1",
//        };

//        (p1 == p2).Should().BeTrue();
//    }


//    [Fact]
//    public void Compare3()
//    {
//        DateTime dt = DateTime.Now;
//        string id = Guid.NewGuid().ToString();

//        var p1 = new ChangeLog
//        {
//            Id = id,
//            Date = dt,
//            ChangedByPrincipalId = "key1",
//            SeatId = "desc1",
//            PropertyName = "propertyName1",
//        };

//        var p2 = new ChangeLog
//        {
//            Id = id,
//            Date = dt,
//            ChangedByPrincipalId = "key1",
//            SeatId = "desc1",
//            PropertyName = "propertyName1",
//        };

//        (p1 == p2).Should().BeTrue();
//    }

//    [Fact]
//    public void NegCompare()
//    {
//        DateTime dt = DateTime.Now;
//        string id = Guid.NewGuid().ToString();

//        var p1 = new ChangeLog
//        {
//            Id = id,
//            Date = dt,
//            ChangedByPrincipalId = "key1",
//            SeatId = "desc1",
//            PropertyName = "propertyName1",
//            OldValue = "oldValue1",
//            NewValue = "newValue",
//        };

//        var p2 = new ChangeLog
//        {
//            Id = id,
//            Date = dt,
//            ChangedByPrincipalId = "key1",
//            SeatId = "desc2",
//            PropertyName = "propertyName1",
//            OldValue = "oldValue1",
//            NewValue = "newValue",
//        };

//        (p1 == p2).Should().BeFalse();
//    }

//    [Fact]
//    public void NegCompare2()
//    {
//        DateTime dt = DateTime.Now;
//        string id = Guid.NewGuid().ToString();

//        var p1 = new ChangeLog
//        {
//            Id = id,
//            Date = dt,
//            ChangedByPrincipalId = "key1",
//            SeatId = "desc1",
//            PropertyName = "propertyName1",
//            OldValue = "oldValue1",
//            NewValue = "newValue",
//        };

//        var p2 = new ChangeLog
//        {
//            Id = id,
//            Date = dt,
//            ChangedByPrincipalId = "key1",
//            SeatId = "desc1",
//            PropertyName = "propertyName1",
//            NewValue = "newValue",
//        };

//        (p1 == p2).Should().BeFalse();
//    }
//}
