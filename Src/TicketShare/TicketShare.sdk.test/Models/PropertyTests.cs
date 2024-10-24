//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using FluentAssertions;
//using Toolbox.Extensions;
//using Toolbox.Types;

//namespace TicketShare.sdk.test.Models;

//public class PropertyTests
//{
//    [Fact]
//    public void Serialization1()
//    {
//        var p1 = new Property
//        {
//            Key = "key1",
//        };

//        p1.Validate().IsOk().Should().BeTrue();
//        string j1 = p1.ToJson();
//        var p2 = j1.ToObject<Property>();
//        (p1 == p2).Should().BeTrue();
//    }

//    [Fact]
//    public void Serialization2()
//    {
//        var p1 = new Property
//        {
//            Key = "key1",
//            Value = "value1",
//        };

//        p1.Validate().IsOk().Should().BeTrue();
//        string j1 = p1.ToJson();
//        var p2 = j1.ToObject<Property>();
//        (p1 == p2).Should().BeTrue();
//    }

//    [Fact]
//    public void Compare1()
//    {
//        var p1 = new Property
//        {
//            Key = "key1",
//            Value = "value1",
//        };

//        var p2 = new Property
//        {
//            Key = "key1",
//            Value = "value1",
//        };

//        (p1 == p2).Should().BeTrue();
//    }

//    [Fact]
//    public void NegCompare1()
//    {
//        var p1 = new Property
//        {
//            Key = "key1",
//            Value = "value1",
//        };

//        var p2 = new Property
//        {
//            Key = "key2",
//            Value = "value1",
//        };

//        (p1 == p2).Should().BeFalse();
//    }

//    [Fact]
//    public void NegCompare2()
//    {
//        var p1 = new Property
//        {
//            Key = "key1",
//            Value = "value1",
//        };

//        var p2 = new Property
//        {
//            Key = "key1",
//            Value = null,
//        };

//        (p1 == p2).Should().BeFalse();

//        p1 = new Property
//        {
//            Key = "key1",
//            Value = null,
//        };

//        p2 = new Property
//        {
//            Key = "key1",
//            Value = "value1",
//        };

//        (p1 == p2).Should().BeFalse();
//    }

//    [Fact]
//    public void NegCompare3()
//    {
//        var p1 = new Property
//        {
//            Key = "key1",
//        };

//        var p2 = new Property
//        {
//            Key = "key2",
//        };

//        (p1 == p2).Should().BeFalse();
//    }
//}
