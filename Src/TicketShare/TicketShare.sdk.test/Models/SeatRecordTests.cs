﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace TicketShare.sdk.test.Models;

public class SeatRecordTests
{
    [Fact]
    public void Serialization1()
    {
        var p1 = new SeatRecord
        {
            SeatId = "seatid1",
            Date = DateTime.Now,
            AssignedToPrincipalId = "user1@domain.com",
        };

        p1.Validate().IsOk().Should().BeTrue();
        string j1 = p1.ToJson();
        var p2 = j1.ToObject<SeatRecord>();
        (p1 == p2).Should().BeTrue();
    }

    [Fact]
    public void Serialization2()
    {
        var p1 = new SeatRecord
        {
            SeatId = "seatid1",
            Date = DateTime.Now,
        };

        p1.Validate().IsOk().Should().BeTrue();
        string j1 = p1.ToJson();
        var p2 = j1.ToObject<SeatRecord>();
        (p1 == p2).Should().BeTrue();
    }

    [Fact]
    public void Compare1()
    {
        DateTime dt = DateTime.Now;

        var p1 = new SeatRecord
        {
            SeatId = "seatid1",
            Date = dt,
            AssignedToPrincipalId = "user1@domain.com",
        };

        var p2 = new SeatRecord
        {
            SeatId = "seatid1",
            Date = dt,
            AssignedToPrincipalId = "user1@domain.com",
        };

        (p1 == p2).Should().BeTrue();
    }

    [Fact]
    public void Compare2()
    {
        DateTime dt = DateTime.Now;

        var p1 = new SeatRecord
        {
            SeatId = "seatid1",
            Date = dt,
        };

        var p2 = new SeatRecord
        {
            SeatId = "seatid1",
            Date = dt,
        };

        (p1 == p2).Should().BeTrue();
    }

    [Fact]
    public void NegCompare1()
    {
        DateTime dt = DateTime.Now;

        var p1 = new SeatRecord
        {
            SeatId = "seatid1",
            Date = dt,
            AssignedToPrincipalId = "user1@domain.com",
        };

        var p2 = new SeatRecord
        {
            SeatId = "seatid1-mod",
            Date = dt,
            AssignedToPrincipalId = "user1@domain.com",
        };

        (p1 == p2).Should().BeFalse();
    }

    [Fact]
    public void NegCompare2()
    {
        DateTime dt = DateTime.Now;

        var p1 = new SeatRecord
        {
            SeatId = "seatid1",
            Date = dt,
        };

        var p2 = new SeatRecord
        {
            SeatId = "seatid1",
            Date = dt,
            AssignedToPrincipalId = "user1@domain.com",
        };

        (p1 == p2).Should().BeFalse();
    }
}
