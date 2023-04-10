//using AisParser.Messages;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;

//namespace AisParser.test
//{
//    public class SerializationTests
//    {
//        [Fact]

//        public void Should_serialize_PositionReportClassAMessage()
//        {
//            var faker = new Faker();
//            var aisMessage = new PositionReportClassAMessage
//            {
//                Repeat = faker.Random.UInt(),
//                Mmsi = faker.Random.UInt(),
//                NavigationStatus = faker.PickRandom<NavigationStatus>(),
//                RateOfTurn = faker.Random.Int(),
//                SpeedOverGround = faker.Random.UInt(),
//                PositionAccuracy = faker.PickRandom<PositionAccuracy>(),
//                Longitude = faker.Random.Double(),
//                Latitude = faker.Random.Double(),
//                CourseOverGround = faker.Random.Double(),
//                TrueHeading = faker.Random.UInt(),
//                Timestamp = faker.Random.UInt(),
//                ManeuverIndicator = faker.PickRandom<ManeuverIndicator>(),
//                Spare = faker.Random.UInt(),
//                Raim = faker.PickRandom<Raim>(),
//                RadioStatus = faker.Random.UInt()
//            };

//            var deserializedAisMessage = Serialize(aisMessage);

//            deserializedAisMessage.Should().NotBeNull();
//            deserializedAisMessage.MessageType.Should().Be(AisMessageType.PositionReportClassA);
//            deserializedAisMessage.Repeat.Should().Be(aisMessage.Repeat);
//            deserializedAisMessage.Mmsi.Should().Be(aisMessage.Mmsi);
//            deserializedAisMessage.NavigationStatus.Should().Be(aisMessage.NavigationStatus);
//            deserializedAisMessage.RateOfTurn.Should().Be(aisMessage.RateOfTurn);
//            deserializedAisMessage.SpeedOverGround.Should().Be(aisMessage.SpeedOverGround);
//            deserializedAisMessage.PositionAccuracy.Should().Be(aisMessage.PositionAccuracy);
//            deserializedAisMessage.Longitude.Should().Be(aisMessage.Longitude);
//            deserializedAisMessage.Latitude.Should().Be(aisMessage.Latitude);
//            deserializedAisMessage.CourseOverGround.Should().Be(aisMessage.CourseOverGround);
//            deserializedAisMessage.TrueHeading.Should().Be(aisMessage.TrueHeading);
//            deserializedAisMessage.Timestamp.Should().Be(aisMessage.Timestamp);
//            deserializedAisMessage.ManeuverIndicator.Should().Be(aisMessage.ManeuverIndicator);
//            deserializedAisMessage.Spare.Should().Be(aisMessage.Spare);
//            deserializedAisMessage.Raim.Should().Be(aisMessage.Raim);
//            deserializedAisMessage.RadioStatus.Should().Be(aisMessage.RadioStatus);
//        }

//        [Fact]
//        public void Should_serialize_PositionReportClassAAssignedScheduleMessage()
//        {
//            var faker = new Faker();
//            var aisMessage = new PositionReportClassAAssignedScheduleMessage
//            {
//                Repeat = faker.Random.UInt(),
//                Mmsi = faker.Random.UInt(),
//                NavigationStatus = faker.PickRandom<NavigationStatus>(),
//                RateOfTurn = faker.Random.Int(),
//                SpeedOverGround = faker.Random.UInt(),
//                PositionAccuracy = faker.PickRandom<PositionAccuracy>(),
//                Longitude = faker.Random.Double(),
//                Latitude = faker.Random.Double(),
//                CourseOverGround = faker.Random.Double(),
//                TrueHeading = faker.Random.UInt(),
//                Timestamp = faker.Random.UInt(),
//                ManeuverIndicator = faker.PickRandom<ManeuverIndicator>(),
//                Spare = faker.Random.UInt(),
//                Raim = faker.PickRandom<Raim>(),
//                RadioStatus = faker.Random.UInt()
//            };

//            var deserializedAisMessage = Serialize(aisMessage);

//            deserializedAisMessage.Should().NotBeNull();
//            deserializedAisMessage.MessageType.Should().Be(AisMessageType.PositionReportClassAAssignedSchedule);
//            deserializedAisMessage.Repeat.Should().Be(aisMessage.Repeat);
//            deserializedAisMessage.Mmsi.Should().Be(aisMessage.Mmsi);
//            deserializedAisMessage.NavigationStatus.Should().Be(aisMessage.NavigationStatus);
//            deserializedAisMessage.RateOfTurn.Should().Be(aisMessage.RateOfTurn);
//            deserializedAisMessage.SpeedOverGround.Should().Be(aisMessage.SpeedOverGround);
//            deserializedAisMessage.PositionAccuracy.Should().Be(aisMessage.PositionAccuracy);
//            deserializedAisMessage.Longitude.Should().Be(aisMessage.Longitude);
//            deserializedAisMessage.Latitude.Should().Be(aisMessage.Latitude);
//            deserializedAisMessage.CourseOverGround.Should().Be(aisMessage.CourseOverGround);
//            deserializedAisMessage.TrueHeading.Should().Be(aisMessage.TrueHeading);
//            deserializedAisMessage.Timestamp.Should().Be(aisMessage.Timestamp);
//            deserializedAisMessage.ManeuverIndicator.Should().Be(aisMessage.ManeuverIndicator);
//            deserializedAisMessage.Spare.Should().Be(aisMessage.Spare);
//            deserializedAisMessage.Raim.Should().Be(aisMessage.Raim);
//            deserializedAisMessage.RadioStatus.Should().Be(aisMessage.RadioStatus);
//        }

//        [Fact]
//        public void Should_serialize_PositionReportClassAResponseToInterrogationMessage()
//        {
//            var faker = new Faker();
//            var aisMessage = new PositionReportClassAResponseToInterrogationMessage
//            {
//                Repeat = faker.Random.UInt(),
//                Mmsi = faker.Random.UInt(),
//                NavigationStatus = faker.PickRandom<NavigationStatus>(),
//                RateOfTurn = faker.Random.Int(),
//                SpeedOverGround = faker.Random.UInt(),
//                PositionAccuracy = faker.PickRandom<PositionAccuracy>(),
//                Longitude = faker.Random.Double(),
//                Latitude = faker.Random.Double(),
//                CourseOverGround = faker.Random.Double(),
//                TrueHeading = faker.Random.UInt(),
//                Timestamp = faker.Random.UInt(),
//                ManeuverIndicator = faker.PickRandom<ManeuverIndicator>(),
//                Spare = faker.Random.UInt(),
//                Raim = faker.PickRandom<Raim>(),
//                RadioStatus = faker.Random.UInt()
//            };

//            var deserializedAisMessage = Serialize(aisMessage);

//            deserializedAisMessage.Should().NotBeNull();
//            deserializedAisMessage.MessageType.Should().Be(AisMessageType.PositionReportClassAResponseToInterrogation);
//            deserializedAisMessage.Repeat.Should().Be(aisMessage.Repeat);
//            deserializedAisMessage.Mmsi.Should().Be(aisMessage.Mmsi);
//            deserializedAisMessage.NavigationStatus.Should().Be(aisMessage.NavigationStatus);
//            deserializedAisMessage.RateOfTurn.Should().Be(aisMessage.RateOfTurn);
//            deserializedAisMessage.SpeedOverGround.Should().Be(aisMessage.SpeedOverGround);
//            deserializedAisMessage.PositionAccuracy.Should().Be(aisMessage.PositionAccuracy);
//            deserializedAisMessage.Longitude.Should().Be(aisMessage.Longitude);
//            deserializedAisMessage.Latitude.Should().Be(aisMessage.Latitude);
//            deserializedAisMessage.CourseOverGround.Should().Be(aisMessage.CourseOverGround);
//            deserializedAisMessage.TrueHeading.Should().Be(aisMessage.TrueHeading);
//            deserializedAisMessage.Timestamp.Should().Be(aisMessage.Timestamp);
//            deserializedAisMessage.ManeuverIndicator.Should().Be(aisMessage.ManeuverIndicator);
//            deserializedAisMessage.Spare.Should().Be(aisMessage.Spare);
//            deserializedAisMessage.Raim.Should().Be(aisMessage.Raim);
//            deserializedAisMessage.RadioStatus.Should().Be(aisMessage.RadioStatus);
//        }

//        [Fact]
//        public void Should_serialize_BaseStationReportMessage()
//        {
//            var faker = new Faker();
//            var aisMessage = new BaseStationReportMessage
//            {
//                Repeat = faker.Random.UInt(),
//                Mmsi = faker.Random.UInt(),
//                Year = faker.Random.UInt(),
//                Month = faker.Random.UInt(1, 12),
//                Day = faker.Random.UInt(1, 31),
//                Hour = faker.Random.UInt(0, 23),
//                Minute = faker.Random.UInt(0, 59),
//                Second = faker.Random.UInt(0, 59),
//                PositionAccuracy = faker.PickRandom<PositionAccuracy>(),
//                Longitude = faker.Random.Double(),
//                Latitude = faker.Random.Double(),
//                PositionFixType = faker.PickRandom<PositionFixType>(),
//                Spare = faker.Random.UInt(),
//                Raim = faker.PickRandom<Raim>(),
//                RadioStatus = faker.Random.UInt()
//            };

//            var deserializedAisMessage = Serialize(aisMessage);

//            deserializedAisMessage.Should().NotBeNull();
//            deserializedAisMessage.MessageType.Should().Be(AisMessageType.BaseStationReport);
//            deserializedAisMessage.Repeat.Should().Be(aisMessage.Repeat);
//            deserializedAisMessage.Mmsi.Should().Be(aisMessage.Mmsi);
//            deserializedAisMessage.Year.Should().Be(aisMessage.Year);
//            deserializedAisMessage.Month.Should().Be(aisMessage.Month);
//            deserializedAisMessage.Day.Should().Be(aisMessage.Day);
//            deserializedAisMessage.Hour.Should().Be(aisMessage.Hour);
//            deserializedAisMessage.Minute.Should().Be(aisMessage.Minute);
//            deserializedAisMessage.Second.Should().Be(aisMessage.Second);
//            deserializedAisMessage.PositionAccuracy.Should().Be(aisMessage.PositionAccuracy);
//            deserializedAisMessage.Longitude.Should().Be(aisMessage.Longitude);
//            deserializedAisMessage.Latitude.Should().Be(aisMessage.Latitude);
//            deserializedAisMessage.PositionFixType.Should().Be(aisMessage.PositionFixType);
//            deserializedAisMessage.Spare.Should().Be(aisMessage.Spare);
//            deserializedAisMessage.Raim.Should().Be(aisMessage.Raim);
//            deserializedAisMessage.RadioStatus.Should().Be(aisMessage.RadioStatus);
//        }

//        [Fact]
//        public void Should_serialize_StaticAndVoyageRelatedDataMessage()
//        {
//            var faker = new Faker();
//            var aisMessage = new StaticAndVoyageRelatedDataMessage
//            {
//                Repeat = faker.Random.UInt(),
//                Mmsi = faker.Random.UInt(),
//                AisVersion = faker.Random.UInt(),
//                ImoNumber = faker.Random.UInt(),
//                CallSign = faker.Lorem.Word(),
//                ShipName = faker.Lorem.Word(),
//                ShipType = faker.PickRandom<ShipType>(),
//                DimensionToBow = faker.Random.UInt(),
//                DimensionToStern = faker.Random.UInt(),
//                DimensionToPort = faker.Random.UInt(),
//                DimensionToStarboard = faker.Random.UInt(),
//                PositionFixType = faker.PickRandom<PositionFixType>(),
//                EtaMonth = faker.Random.UInt(1, 12),
//                EtaDay = faker.Random.UInt(1, 31),
//                EtaHour = faker.Random.UInt(0, 23),
//                EtaMinute = faker.Random.UInt(0, 59),
//                Draught = faker.Random.Double(),
//                Destination = faker.Lorem.Word(),
//                DataTerminalReady = faker.Random.Bool(),
//                Spare = faker.Random.UInt()
//            };

//            var deserializedAisMessage = Serialize(aisMessage);

//            deserializedAisMessage.Should().NotBeNull();
//            deserializedAisMessage.MessageType.Should().Be(AisMessageType.StaticAndVoyageRelatedData);
//            deserializedAisMessage.Repeat.Should().Be(aisMessage.Repeat);
//            deserializedAisMessage.Mmsi.Should().Be(aisMessage.Mmsi);
//            deserializedAisMessage.AisVersion.Should().Be(aisMessage.AisVersion);
//            deserializedAisMessage.ImoNumber.Should().Be(aisMessage.ImoNumber);
//            deserializedAisMessage.CallSign.Should().Be(aisMessage.CallSign);
//            deserializedAisMessage.ShipName.Should().Be(aisMessage.ShipName);
//            deserializedAisMessage.ShipType.Should().Be(aisMessage.ShipType);
//            deserializedAisMessage.DimensionToBow.Should().Be(aisMessage.DimensionToBow);
//            deserializedAisMessage.DimensionToStern.Should().Be(aisMessage.DimensionToStern);
//            deserializedAisMessage.DimensionToPort.Should().Be(aisMessage.DimensionToPort);
//            deserializedAisMessage.DimensionToStarboard.Should().Be(aisMessage.DimensionToStarboard);
//            deserializedAisMessage.PositionFixType.Should().Be(aisMessage.PositionFixType);
//            deserializedAisMessage.EtaMonth.Should().Be(aisMessage.EtaMonth);
//            deserializedAisMessage.EtaDay.Should().Be(aisMessage.EtaDay);
//            deserializedAisMessage.EtaHour.Should().Be(aisMessage.EtaHour);
//            deserializedAisMessage.EtaMinute.Should().Be(aisMessage.EtaMinute);
//            deserializedAisMessage.Draught.Should().Be(aisMessage.Draught);
//            deserializedAisMessage.Destination.Should().Be(aisMessage.Destination);
//            deserializedAisMessage.DataTerminalReady.Should().Be(aisMessage.DataTerminalReady);
//            deserializedAisMessage.Spare.Should().Be(aisMessage.Spare);
//        }

//        [Fact]
//        public void Should_serialize_StandardClassBCsPositionReportMessage()
//        {
//            var faker = new Faker();
//            var aisMessage = new StandardClassBCsPositionReportMessage
//            {
//                Repeat = faker.Random.UInt(),
//                Mmsi = faker.Random.UInt(),
//                SpeedOverGround = faker.Random.UInt(),
//                PositionAccuracy = faker.PickRandom<PositionAccuracy>(),
//                Longitude = faker.Random.Double(),
//                Latitude = faker.Random.Double(),
//                CourseOverGround = faker.Random.Double(),
//                TrueHeading = faker.Random.UInt(),
//                Timestamp = faker.Random.UInt(),
//                IsCsUnit = faker.Random.Bool(),
//                HasDisplay = faker.Random.Bool(),
//                HasDscCapability = faker.Random.Bool(),
//                Band = faker.Random.Bool(),
//                CanAcceptMessage22 = faker.Random.Bool(),
//                Assigned = faker.Random.Bool(),
//                Raim = faker.PickRandom<Raim>(),
//                RadioStatus = faker.Random.UInt()
//            };

//            var deserializedAisMessage = Serialize(aisMessage);

//            deserializedAisMessage.Should().NotBeNull();
//            deserializedAisMessage.MessageType.Should().Be(AisMessageType.StandardClassBCsPositionReport);
//            deserializedAisMessage.Repeat.Should().Be(aisMessage.Repeat);
//            deserializedAisMessage.Mmsi.Should().Be(aisMessage.Mmsi);
//            deserializedAisMessage.SpeedOverGround.Should().Be(aisMessage.SpeedOverGround);
//            deserializedAisMessage.PositionAccuracy.Should().Be(aisMessage.PositionAccuracy);
//            deserializedAisMessage.Longitude.Should().Be(aisMessage.Longitude);
//            deserializedAisMessage.Latitude.Should().Be(aisMessage.Latitude);
//            deserializedAisMessage.CourseOverGround.Should().Be(aisMessage.CourseOverGround);
//            deserializedAisMessage.TrueHeading.Should().Be(aisMessage.TrueHeading);
//            deserializedAisMessage.Timestamp.Should().Be(aisMessage.Timestamp);
//            deserializedAisMessage.IsCsUnit.Should().Be(aisMessage.IsCsUnit);
//            deserializedAisMessage.HasDisplay.Should().Be(aisMessage.HasDisplay);
//            deserializedAisMessage.HasDscCapability.Should().Be(aisMessage.HasDscCapability);
//            deserializedAisMessage.Band.Should().Be(aisMessage.Band);
//            deserializedAisMessage.CanAcceptMessage22.Should().Be(aisMessage.CanAcceptMessage22);
//            deserializedAisMessage.Assigned.Should().Be(aisMessage.Assigned);
//            deserializedAisMessage.Raim.Should().Be(aisMessage.Raim);
//            deserializedAisMessage.RadioStatus.Should().Be(aisMessage.RadioStatus);
//        }

//        [Fact]
//        public void Should_serialize_AidToNavigationReportMessage()
//        {
//            var faker = new Faker();
//            var aisMessage = new AidToNavigationReportMessage
//            {
//                Repeat = faker.Random.UInt(),
//                Mmsi = faker.Random.UInt(),
//                NavigationalAidType = faker.PickRandom<NavigationalAidType>(),
//                Name = faker.Lorem.Word(),
//                PositionAccuracy = faker.PickRandom<PositionAccuracy>(),
//                Longitude = faker.Random.Double(),
//                Latitude = faker.Random.Double(),
//                DimensionToBow = faker.Random.UInt(),
//                DimensionToStern = faker.Random.UInt(),
//                DimensionToPort = faker.Random.UInt(),
//                DimensionToStarboard = faker.Random.UInt(),
//                PositionFixType = faker.PickRandom<PositionFixType>(),
//                Timestamp = faker.Random.UInt(),
//                OffPosition = faker.Random.Bool(),
//                Raim = faker.PickRandom<Raim>(),
//                VirtualAid = faker.Random.Bool(),
//                Assigned = faker.Random.Bool(),
//                Spare = faker.Random.UInt(),
//                NameExtension = faker.Lorem.Word()
//            };

//            var deserializedAisMessage = Serialize(aisMessage);

//            deserializedAisMessage.Should().NotBeNull();
//            deserializedAisMessage.MessageType.Should().Be(AisMessageType.AidToNavigationReport);
//            deserializedAisMessage.Repeat.Should().Be(aisMessage.Repeat);
//            deserializedAisMessage.Mmsi.Should().Be(aisMessage.Mmsi);
//            deserializedAisMessage.NavigationalAidType.Should().Be(aisMessage.NavigationalAidType);
//            deserializedAisMessage.Name.Should().Be(aisMessage.Name);
//            deserializedAisMessage.PositionAccuracy.Should().Be(aisMessage.PositionAccuracy);
//            deserializedAisMessage.Longitude.Should().Be(aisMessage.Longitude);
//            deserializedAisMessage.Latitude.Should().Be(aisMessage.Latitude);
//            deserializedAisMessage.DimensionToBow.Should().Be(aisMessage.DimensionToBow);
//            deserializedAisMessage.DimensionToStern.Should().Be(aisMessage.DimensionToStern);
//            deserializedAisMessage.DimensionToPort.Should().Be(aisMessage.DimensionToPort);
//            deserializedAisMessage.DimensionToStarboard.Should().Be(aisMessage.DimensionToStarboard);
//            deserializedAisMessage.PositionFixType.Should().Be(aisMessage.PositionFixType);
//            deserializedAisMessage.Timestamp.Should().Be(aisMessage.Timestamp);
//            deserializedAisMessage.OffPosition.Should().Be(aisMessage.OffPosition);
//            deserializedAisMessage.Raim.Should().Be(aisMessage.Raim);
//            deserializedAisMessage.VirtualAid.Should().Be(aisMessage.VirtualAid);
//            deserializedAisMessage.Assigned.Should().Be(aisMessage.Assigned);
//            deserializedAisMessage.Spare.Should().Be(aisMessage.Spare);
//            deserializedAisMessage.NameExtension.Should().Be(aisMessage.NameExtension);
//        }

//        [Fact]
//        public void Should_serialize_DataLinkManagementMessage()
//        {
//            var faker = new Faker();
//            var aisMessage = new DataLinkManagementMessage
//            {
//                Repeat = faker.Random.UInt(),
//                Mmsi = faker.Random.UInt(),
//                Spare = faker.Random.UInt(),
//                Offset1 = faker.Random.UInt(),
//                ReservedSlots1 = faker.Random.UInt(),
//                Timeout1 = faker.Random.UInt(),
//                Increment1 = faker.Random.UInt(),
//                Offset2 = faker.Random.UInt(),
//                ReservedSlots2 = faker.Random.UInt(),
//                Timeout2 = faker.Random.UInt(),
//                Increment2 = faker.Random.UInt(),
//                Offset3 = faker.Random.UInt(),
//                ReservedSlots3 = faker.Random.UInt(),
//                Timeout3 = faker.Random.UInt(),
//                Increment3 = faker.Random.UInt(),
//                Offset4 = faker.Random.UInt(),
//                ReservedSlots4 = faker.Random.UInt(),
//                Timeout4 = faker.Random.UInt(),
//                Increment4 = faker.Random.UInt(),
//            };

//            var deserializedAisMessage = Serialize(aisMessage);

//            deserializedAisMessage.Should().NotBeNull();
//            deserializedAisMessage.MessageType.Should().Be(AisMessageType.DataLinkManagement);
//            deserializedAisMessage.Repeat.Should().Be(aisMessage.Repeat);
//            deserializedAisMessage.Mmsi.Should().Be(aisMessage.Mmsi);
//            deserializedAisMessage.Spare.Should().Be(aisMessage.Spare);
//            deserializedAisMessage.Offset1.Should().Be(aisMessage.Offset1);
//            deserializedAisMessage.ReservedSlots1.Should().Be(aisMessage.ReservedSlots1);
//            deserializedAisMessage.Timeout1.Should().Be(aisMessage.Timeout1);
//            deserializedAisMessage.Increment1.Should().Be(aisMessage.Increment1);
//            deserializedAisMessage.Offset2.Should().Be(aisMessage.Offset2);
//            deserializedAisMessage.ReservedSlots2.Should().Be(aisMessage.ReservedSlots2);
//            deserializedAisMessage.Timeout2.Should().Be(aisMessage.Timeout2);
//            deserializedAisMessage.Increment2.Should().Be(aisMessage.Increment2);
//            deserializedAisMessage.Offset3.Should().Be(aisMessage.Offset3);
//            deserializedAisMessage.ReservedSlots3.Should().Be(aisMessage.ReservedSlots3);
//            deserializedAisMessage.Timeout3.Should().Be(aisMessage.Timeout3);
//            deserializedAisMessage.Increment3.Should().Be(aisMessage.Increment3);
//            deserializedAisMessage.Offset4.Should().Be(aisMessage.Offset4);
//            deserializedAisMessage.ReservedSlots4.Should().Be(aisMessage.ReservedSlots4);
//            deserializedAisMessage.Timeout4.Should().Be(aisMessage.Timeout4);
//            deserializedAisMessage.Increment4.Should().Be(aisMessage.Increment4);
//        }

//        [Fact]
//        public void Should_serialize_StaticDataReportPartAMessage()
//        {
//            var faker = new Faker();
//            var aisMessage = new StaticDataReportPartAMessage
//            {
//                Repeat = faker.Random.UInt(),
//                Mmsi = faker.Random.UInt(),
//                ShipName = faker.Lorem.Word(),
//                Spare = faker.Random.UInt()
//            };

//            var deserializedAisMessage = Serialize(aisMessage);

//            deserializedAisMessage.Should().NotBeNull();
//            deserializedAisMessage.MessageType.Should().Be(AisMessageType.StaticDataReport);
//            deserializedAisMessage.Repeat.Should().Be(aisMessage.Repeat);
//            deserializedAisMessage.Mmsi.Should().Be(aisMessage.Mmsi);
//            deserializedAisMessage.PartNumber.Should().Be(aisMessage.PartNumber);
//            deserializedAisMessage.ShipName.Should().Be(aisMessage.ShipName);
//            deserializedAisMessage.Spare.Should().Be(aisMessage.Spare);
//        }

//        [Fact]
//        public void Should_serialize_StaticDataReportPartBMessage()
//        {
//            var faker = new Faker();
//            var aisMessage = new StaticDataReportPartBMessage
//            {
//                Repeat = faker.Random.UInt(),
//                Mmsi = faker.Random.UInt(),
//                ShipType = faker.PickRandom<ShipType>(),
//                VendorId = faker.Lorem.Word(),
//                UnitModelCode = faker.Random.UInt(),
//                SerialNumber = faker.Random.UInt(),
//                CallSign = faker.Random.Word(),
//                DimensionToBow = faker.Random.UInt(),
//                DimensionToStern = faker.Random.UInt(),
//                DimensionToPort = faker.Random.UInt(),
//                DimensionToStarboard = faker.Random.UInt(),
//                Spare = faker.Random.UInt()
//            };

//            var deserializedAisMessage = Serialize(aisMessage);

//            deserializedAisMessage.Should().NotBeNull();
//            deserializedAisMessage.MessageType.Should().Be(AisMessageType.StaticDataReport);
//            deserializedAisMessage.Repeat.Should().Be(aisMessage.Repeat);
//            deserializedAisMessage.Mmsi.Should().Be(aisMessage.Mmsi);
//            deserializedAisMessage.PartNumber.Should().Be(aisMessage.PartNumber);
//            deserializedAisMessage.ShipType.Should().Be(aisMessage.ShipType);
//            deserializedAisMessage.VendorId.Should().Be(aisMessage.VendorId);
//            deserializedAisMessage.UnitModelCode.Should().Be(aisMessage.UnitModelCode);
//            deserializedAisMessage.SerialNumber.Should().Be(aisMessage.SerialNumber);
//            deserializedAisMessage.CallSign.Should().Be(aisMessage.CallSign);
//            deserializedAisMessage.DimensionToBow.Should().Be(aisMessage.DimensionToBow);
//            deserializedAisMessage.DimensionToStern.Should().Be(aisMessage.DimensionToStern);
//            deserializedAisMessage.DimensionToPort.Should().Be(aisMessage.DimensionToPort);
//            deserializedAisMessage.DimensionToStarboard.Should().Be(aisMessage.DimensionToStarboard);
//            deserializedAisMessage.Spare.Should().Be(aisMessage.Spare);
//        }

//        private static T Serialize<T>(T aisMessage) where T : AisMessage
//        {
//            var json = AisMessageJsonConvert.Serialize(aisMessage);
//            return AisMessageJsonConvert.Deserialize(json) as T;
//        }
//    }
//}
