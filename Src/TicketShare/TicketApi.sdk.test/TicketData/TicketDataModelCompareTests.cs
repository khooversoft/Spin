//using TicketApi.sdk.Model;
//using Toolbox.Tools;

//namespace TicketApi.sdk.test.TicketData;

//public class TicketDataModelCompareTests
//{
//    [Fact]
//    public void TestImageModel()
//    {
//        var m1 = new ImageModel { Ratio = "r", Url = "u", Height = 1, Width = 3 };
//        var m2 = new ImageModel { Ratio = "r", Url = "u", Height = 1, Width = 3 };
//        (m1 == m2).BeTrue();
//    }

//    [Fact]
//    public void TestAttractionModel()
//    {
//        var m1 = new AttractionModel { Id = "a1", Name = "name1a", Url = "url", Locale = "us-en", Images = [new ImageModel { Ratio = "r", Url = "u", Height = 1, Width = 3 }] };
//        var m2 = new AttractionModel { Id = "a1", Name = "name1a", Url = "url", Locale = "us-en", Images = [new ImageModel { Ratio = "r", Url = "u", Height = 1, Width = 3 }] };

//        (m1 == m2).BeTrue();
//    }

//    [Fact]
//    public void TestEventModel()
//    {
//        var m1 = new EventRecord { Id = "1b", Name = "name1b", Timezone = "tz1" };
//        var m2 = new EventRecord { Id = "1b", Name = "name1b", Timezone = "tz1" };

//        (m1 == m2).BeTrue();
//    }

//    [Fact]
//    public void TestVenueModel()
//    {
//        var m1 = new VenueRecord { Id = "1c", Name = "name1c", City = "city1c" };
//        var m2 = new VenueRecord { Id = "1c", Name = "name1c", City = "city1c" };

//        (m1 == m2).BeTrue();
//    }

//    //[Fact]
//    //public void DataCompareTest1()
//    //{
//    //    DateTime dt = DateTime.UtcNow;

//    //    var m1 = new TicketDataRecord
//    //    {
//    //        LastUpdated = dt,
//    //        Attractions = [
//    //            new AttractionRecord { Id = "a1", Name = "name1a", Url = "url", Locale = "us-en" },
//    //            new AttractionRecord { Id = "a2", Name = "name2a" },
//    //        ],
//    //        Events = [
//    //            new EventRecord { Id = "1b", Name = "name1b", Timezone = "tz1" },
//    //            new EventRecord { Id = "2b", Name = "name2b", Timezone = "tz2" },
//    //        ],
//    //        Venues = [
//    //            new VenueRecord { Id = "1c", Name = "name1c", City = "city1c" },
//    //            new VenueRecord { Id = "2c", Name = "name2c", City = "city2c" },
//    //        ],
//    //    };

//    //    var m2 = new TicketDataRecord
//    //    {
//    //        LastUpdated = dt,
//    //        Attractions = [
//    //            new AttractionRecord { Id = "a1", Name = "name1a", Url = "url", Locale = "us-en" },
//    //            new AttractionRecord { Id = "a2", Name = "name2a" },
//    //        ],
//    //        Events = [
//    //            new EventRecord { Id = "1b", Name = "name1b", Timezone = "tz1" },
//    //            new EventRecord { Id = "2b", Name = "name2b", Timezone = "tz2" },
//    //        ],
//    //        Venues = [
//    //            new VenueRecord { Id = "1c", Name = "name1c", City = "city1c" },
//    //            new VenueRecord { Id = "2c", Name = "name2c", City = "city2c" },
//    //        ],
//    //    };

//    //    (m1 == m2).BeTrue();
//    //}
//}
