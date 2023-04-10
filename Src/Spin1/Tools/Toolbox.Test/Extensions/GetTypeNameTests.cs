using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Xunit;

namespace Toolbox.Test.Extensions;

public class GetTypeNameTests
{
    [Fact]
    public void GivenStringType_WhenAskForTypeName_ShouldPass()
    {
        string v1 = "this";
        string v1name = v1.GetTypeName();
        v1name.Should().Be("String");

        string v1Type = v1.GetType().GetTypeName();
        v1Type.Should().Be("String");
    }

    [Fact]
    public void GivenListOfStringType_WhenAskForTypeName_ShouldPass()
    {
        var v1 = new List<string>();
        string v1name = v1.GetTypeName();
        v1name.Should().Be("List`1:String");
    }

    [Fact]
    public void GivenClassType_WhenAskForTypeName_ShouldPass()
    {
        var v1 = new ClassA();
        string v1name = v1.GetTypeName();
        v1name.Should().Be("ClassA");
    }

    [Fact]
    public void GivenRecordType_WhenAskForTypeName_ShouldPass()
    {
        var v1 = new RecordA();
        string v1name = v1.GetTypeName();
        v1name.Should().Be("RecordA");
    }

    [Fact]
    public void GivenGroupype_WhenAskForTypeName_ShouldPass()
    {
        var group = new GroupA<RecordA>
        {
            Items = Array.Empty<RecordA>(),
        };

        string v1name = group.GetTypeName();
        v1name.Should().Be("GroupA`1:RecordA");
    }

    private class ClassA
    {
        public string? Name { get; set; }
    }

    private record RecordA
    {
        public string? Name { get; set; }
    }

    public record GroupA<T> where T : class
    {
        public IReadOnlyList<T>? Items { get; set; }
    }
}
