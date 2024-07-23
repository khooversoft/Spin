﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace TicketShare.sdk.test.Models;

public class RoleRecordTests
{
    [Fact]
    public void Serialization1()
    {
        var p1 = new RoleRecord
        {
            PrincipalId = "key1",
            MemberRole = RolePermission.Owner,
        };

        p1.Validate().IsOk().Should().BeTrue();
        string j1 = p1.ToJson();
        var p2 = j1.ToObject<RoleRecord>();
        (p1 == p2).Should().BeTrue();
    }

    [Fact]
    public void Compare1()
    {
        var p1 = new RoleRecord
        {
            PrincipalId = "key1",
            MemberRole = RolePermission.Owner,
        };

        var p2 = new RoleRecord
        {
            PrincipalId = "key1",
            MemberRole = RolePermission.Owner,
        };

        (p1 == p2).Should().BeTrue();
    }

    [Fact]
    public void NegCompare1()
    {
        var p1 = new RoleRecord
        {
            PrincipalId = "key1",
            MemberRole = RolePermission.Contributor,
        };

        var p2 = new RoleRecord
        {
            PrincipalId = "key1",
            MemberRole = RolePermission.Owner,
        };

        (p1 == p2).Should().BeFalse();
    }

    [Fact]
    public void NegCompare2()
    {
        var p1 = new RoleRecord
        {
            PrincipalId = "key2",
            MemberRole = RolePermission.Owner,
        };

        var p2 = new RoleRecord
        {
            PrincipalId = "key1",
            MemberRole = RolePermission.Owner,
        };

        (p1 == p2).Should().BeFalse();
    }
}
