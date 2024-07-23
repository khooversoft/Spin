using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace TicketShare.sdk.test.Models;

public class SeasonTicketRecordTests
{
    DateTime _date = DateTime.Now;

    [Fact]
    public void Serialization1()
    {
        DateTime dt = DateTime.Now;

        var p1 = new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        };

        p1.Validate().IsOk().Should().BeTrue();
        string j1 = p1.ToJson();
        var p2 = j1.ToObject<SeasonTicketRecord>();
        (p1 == p2).Should().BeTrue();
    }

    [Fact]
    public void Serialization2()
    {
        DateTime dt = DateTime.Now;

        var p1 = new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
        };

        p1.Validate().IsOk().Should().BeTrue();
        string j1 = p1.ToJson();
        var p2 = j1.ToObject<SeasonTicketRecord>();
        (p1 == p2).Should().BeTrue();
    }

    [Fact]
    public void Compare1()
    {
        DateTime dt = DateTime.Now;

        var p1 = new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        };

        new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        }.Action(x => (p1 == x).Should().BeTrue());
    }

    [Fact]
    public void NegCompare()
    {
        DateTime dt = DateTime.Now;

        var p1 = new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        };

        new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        }.Action(x => (p1 == x).Should().BeFalse());
    }


    [Fact]
    public void NegCompare2()
    {
        DateTime dt = DateTime.Now;

        var p1 = new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        }; new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "ame1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        }.Action(x => (p1 == x).Should().BeFalse());
    }

    [Fact]
    public void NegCompare3()
    {
        DateTime dt = DateTime.Now;

        var p1 = new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        };

        new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "des1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        }.Action(x => (p1 == x).Should().BeFalse());
    }

    [Fact]
    public void NegCompare4()
    {
        DateTime dt = DateTime.Now;

        var p1 = new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        };

        new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1x@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        }.Action(x => (p1 == x).Should().BeFalse());
    }

    [Fact]
    public void NegCompare5()
    {
        DateTime dt = DateTime.Now;

        var p1 = new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        };
        new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1xx",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        }.Action(x => (p1 == x).Should().BeFalse());
    }

    [Fact]
    public void NegCompare6()
    {
        DateTime dt = DateTime.Now;

        var p1 = new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        };
        new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1x", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        }.Action(x => (p1 == x).Should().BeFalse());
    }

    [Fact]
    public void NegCompare7()
    {
        DateTime dt = DateTime.Now;

        var p1 = new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        };
        new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domainx.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        }.Action(x => (p1 == x).Should().BeFalse());
    }

    [Fact]
    public void NegCompare8()
    {
        DateTime dt = DateTime.Now;

        var p1 = new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        };
        new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "userx1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        }.Action(x => (p1 == x).Should().BeFalse());
    }

    [Fact]
    public void NegCompare9()
    {
        DateTime dt = DateTime.Now;

        var p1 = new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog()],
        };
        new SeasonTicketRecord
        {
            SeasonTicketId = "season/2024/t1",
            Name = "name1",
            Description = "desc1",
            OwnerPrincipalId = "user1@domain.com",
            Tags = "tags1",
            Properties = [new Property { Key = "key1", Value = "value1" }],
            Members = [new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }],
            Seats = [new SeatRecord { SeatId = "r1s2", AssignedToPrincipalId = "user1@domain.com", Date = dt }],
            ChangeLogs = [CreateChangeLog() with { OldValue = "oldxxx" }],
        }.Action(x => (p1 == x).Should().BeFalse());
    }

    private ChangeLog CreateChangeLog() => new ChangeLog
    {
        ChangedByPrincipalId = "user1@domain.com",
        Date = _date,
        Description = "desc1",
        PropertyName = "propertyName1",
        OldValue = "oldValue1",
        NewValue = "newValue"
    };
}
