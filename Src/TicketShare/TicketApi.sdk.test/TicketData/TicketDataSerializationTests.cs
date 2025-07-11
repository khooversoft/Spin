//using TicketApi.sdk.Model;
//using Toolbox.Extensions;
//using Toolbox.Tools;

//namespace TicketApi.sdk.test.TicketData;

//public class TicketDataSerializationTests
//{
//    [Fact]
//    public void SerializationEmptyTest()
//    {
//        var m1 = new ClassificationRecord();
//        var json = m1.ToJson();
//        var m2 = json.ToObject<ClassificationRecord>();
//        (m1 == m2).BeTrue();
//    }

//    [Fact]
//    public void AttractionSerializationTest()
//    {
//        var m1 = new AttractionModel { Id = "a1", Name = "name1a", Url = "url", Locale = "us-en", Images = [new ImageModel { Ratio = "r", Url = "u", Height = 1, Width = 3 }] };

//        var json = m1.ToJson();
//        var m2 = json.ToObject<AttractionModel>();
//        (m1 == m2).BeTrue();
//    }

//    [Fact]
//    public void EventSerializationTest()
//    {
//        var dt = DateTime.Now;
//        var m1 = new EventRecord { Id = "1b", Name = "name1b", Timezone = "tz1", LocalDateTime = dt };
//        var json = m1.ToJson();
//        var m2 = json.ToObject<EventRecord>();
//        (m1 == m2).BeTrue();
//    }

//    [Fact]
//    public void VenueSerializationTest()
//    {
//        var m1 = new VenueRecord { Id = "1c", Name = "name1c", City = "city1c" };
//        var json = m1.ToJson();
//        var m2 = json.ToObject<VenueRecord>();
//        (m1 == m2).BeTrue();
//    }

//    //[Fact]
//    //public void SerializationDataTest()
//    //{
//    //    var m1 = new TicketDataRecord
//    //    {
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

//    //    var json = m1.ToJson();
//    //    var m2 = json.ToObject<TicketDataRecord>();
//    //    (m1 == m2).BeTrue();
//    //}
//}
