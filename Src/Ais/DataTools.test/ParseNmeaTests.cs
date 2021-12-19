using DataTools.Application;
using DataTools.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using Toolbox.Tools;
using Xunit;

namespace DataTools.test
{
    public class ParseNmeaTests
    {
        //[Fact]
        //public void ParseShortSample1()
        //{
        //    string message = @"\s:rORBCOMM029,c:1777777777,q:u*hh\!AIVDM,1,1,,,13a;9F3002u8U8=k9oL4uUkL0000,0*12";

        //    NmeaRecord record = new NmeaParser(new AppOption(), new Counters(new NullLogger<Counters>()), new NullLogger<NmeaParser>()).Parse(message)!;

        //    record.GroupId.Should().BeNull();
        //    record.SourceType.Should().Be("rORBCOMM029");
        //    record.Timestamp.Should().Be("1777777777");
        //    record.Quality.Should().Be("u");
        //    record.Chksum.Should().Be("hh");
        //    record.AisMessage.Should().Be("!AIVDM,1,1,,,13a;9F3002u8U8=k9oL4uUkL0000,0*12");
        //    record.AisMessageJson.Should().Be("{\"messageType\":\"positionReportClassA\",\"repeat\":0,\"mmsi\":244500824}");
        //}

        //[Fact]
        //public void ParseShortSample2()
        //{
        //    string message = @"\s:rORBCOMM007,q:u,c:1548979200*35\!AIVDM,1,1,,,405flgQv<PP0024E;QeSC:700L02,0*3F";

        //    NmeaRecord record = new NmeaParser(new AppOption(), new Counters(new NullLogger<Counters>()), new NullLogger<NmeaParser>()).Parse(message)!;

        //    record.GroupId.Should().BeNull();
        //    record.SourceType.Should().Be("rORBCOMM007");
        //    record.Timestamp.Should().Be("1548979200");
        //    record.Quality.Should().Be("u");
        //    record.Chksum.Should().Be("35");
        //    record.AisMessage.Should().Be("!AIVDM,1,1,,,405flgQv<PP0024E;QeSC:700L02,0*3F");
        //    record.AisMessageJson.Should().Be("{\"messageType\":\"baseStationReport\",\"repeat\":0,\"mmsi\":6010046}");
        //}

        //[Fact]
        //public void ParseShortSample3()
        //{
        //    string message = @"\s:rORBCOMM007,q:u,c:1548979245*34\!AIVDM,1,1,,,405flaiv<PP0d1UHJCdLA8?02L0;,0*44";

        //    NmeaRecord record = new NmeaParser(new AppOption(), new Counters(new NullLogger<Counters>()), new NullLogger<NmeaParser>()).Parse(message)!;

        //    record.GroupId.Should().BeNull();
        //    record.SourceType.Should().Be("rORBCOMM007");
        //    record.Timestamp.Should().Be("1548979245");
        //    record.Quality.Should().Be("u");
        //    record.Chksum.Should().Be("34");
        //    record.AisMessage.Should().Be("!AIVDM,1,1,,,405flaiv<PP0d1UHJCdLA8?02L0;,0*44");
        //    record.AisMessageJson.Should().Be("{\"messageType\":\"baseStationReport\",\"repeat\":0,\"mmsi\":6010023}");
        //}

        //[Fact]
        //public void LongShortSample4()
        //{
        //    string message = @"\g:1-2-7275,s:rORBCOMM009,c:1549083786*5A\!AIVDM,2,1,5,B,<A7w5@0>4gqPwT4CweCw:QH2;h1>5wr:0hDO0@01::acJ7sEEEEv144cf800,0*46";

        //    NmeaRecord? record = new NmeaParser(new AppOption(), new Counters(new NullLogger<Counters>()), new NullLogger<NmeaParser>()).Parse(message);
        //    record.Should().BeNull();
        //}

        //[Fact]
        //public void LongShortSample5()
        //{
        //    string message = @"\g:1-2-7275,s:rORBCOMM009,c:1549083786*5A\!AIVDM,2,1,5,B,<A7w5@0>4gqPwT4CweCw:QH2;h1>5wr:0hDO0@01::acJ7sEEEEv144cf800,0*46";

        //    NmeaRecord? record = new NmeaParser(new AppOption(), new Counters(new NullLogger<Counters>()), new NullLogger<NmeaParser>()).Parse(message);
        //    record.Should().BeNull();
        //}
    }
}
