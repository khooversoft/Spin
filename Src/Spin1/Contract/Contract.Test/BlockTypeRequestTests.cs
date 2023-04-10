using Contract.sdk.Models;
using Contract.sdk.Service;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Contract.Test;

public class BlockTypeRequestTests
{
    [Fact]
    public void WhenConstructingNodata_ShouldPass()
    {
        var subject = new BlockTypeRequest();

        const string shouldResult = "*";
        string request = subject.ToString();        
        request.Should().Be(shouldResult);

        BlockTypeRequest parsedRequest = BlockTypeRequest.Parse(request);
        parsedRequest.Should().NotBeNull();
        parsedRequest.Count().Should().Be(0);
        parsedRequest.ToString().Should().Be(shouldResult);
    }
    
    [Fact]
    public void WhenConstructingWithSingleRequest_ShouldPass()
    {
        var subject = new BlockTypeRequest().Add(typeof(RecordA));

        const string shouldResult = "RecordA";
        string request = subject.ToString();        
        request.Should().Be(shouldResult);

        subject = typeof(RecordA).ToBlockTypeRequest();
        request = subject.ToString();
        request.Should().Be(shouldResult);

        BlockTypeRequest parsedRequest = BlockTypeRequest.Parse(request);
        parsedRequest.Should().NotBeNull();
        parsedRequest.Count().Should().Be(1);
        parsedRequest.First().Should().Be(("RecordA", false));
        parsedRequest.ToString().Should().Be(shouldResult);
    }

    [Fact]
    public void WhenConstructingWithSingleAndAllRequest_ShouldPass()
    {
        var subject = (BlockTypeRequest)(typeof(RecordB), true);

        const string shouldResult = "RecordB.*";
        string request = subject.ToString();        
        request.Should().Be(shouldResult);

        subject = (typeof(RecordB), true).ToBlockTypeRequest();
        request = subject.ToString();        
        request.Should().Be(shouldResult);

        BlockTypeRequest parsedRequest = BlockTypeRequest.Parse(request);
        parsedRequest.Should().NotBeNull();
        parsedRequest.Count().Should().Be(1);
        parsedRequest.First().Should().Be(("RecordB", true));
        parsedRequest.ToString().Should().Be(shouldResult);
    }

    [Fact]
    public void WhenConstructingRequest_ShouldPass()
    {
        var subject = (BlockTypeRequest)typeof(RecordA) + (typeof(RecordB), true);

        const string shouldResult = "RecordA;RecordB.*";
        string request = subject.ToString();        
        request.Should().Be(shouldResult);

        subject = typeof(RecordA).ToBlockTypeRequest() + (typeof(RecordB), true);
        request = subject.ToString();        
        request.Should().Be(shouldResult);

        BlockTypeRequest parsedRequest = BlockTypeRequest.Parse(request);
        parsedRequest.Should().NotBeNull();
        parsedRequest.Count().Should().Be(2);
        parsedRequest.First().Should().Be(("RecordA", false));
        parsedRequest.Skip(1).First().Should().Be(("RecordB", true));
        parsedRequest.ToString().Should().Be(shouldResult);
    }

    [Fact]
    public void WhenConstructingRequestWithAdd_ShouldPass()
    {
        const string shouldResult = "RecordA.*;RecordB.*";

        var subject = new BlockTypeRequest()
            .Add(typeof(RecordA), true)
            .Add(typeof(RecordB), true);

        string request = subject.ToString();
        request.Should().Be(shouldResult);

        subject = (typeof(RecordA), true)
            .ToBlockTypeRequest()
            .Add(typeof(RecordB), true);

        request = subject.ToString();
        request.Should().Be(shouldResult);

        BlockTypeRequest parsedRequest = BlockTypeRequest.Parse(request);
        parsedRequest.Should().NotBeNull();
        parsedRequest.Count().Should().Be(2);
        parsedRequest.First().Should().Be(("RecordA", true));
        parsedRequest.Skip(1).First().Should().Be(("RecordB", true));
        parsedRequest.ToString().Should().Be(shouldResult);
    }

    [Fact]
    public void WhenConstructingDynamic_ShouldPass()
    {
        var payloadList = new[]
        {
            new Payload("payloadName", "payloadValue"),
            new Payload("Pay2", "value2"),
        };

        string request = payloadList[0].GetType().ToBlockTypeRequest().ToString();
        request.Should().Be("Payload");

        string request2 = (payloadList[1].GetType(), true).ToBlockTypeRequest().ToString();
        request2.Should().Be("Payload.*");
    }

    [Fact]
    public void WhenConstructingGeneric_ShouldPass()
    {
        const string shouldResult = "DataGroup`1:Payload.*;DataGroup`1:Payload2";

        BlockTypeRequest blockTypes = new BlockTypeRequest()
            .Add<DataGroup<Payload>>(true)
            .Add<DataGroup<Payload2>>(false);

        blockTypes.Should().NotBeNull();
        blockTypes.Count().Should().Be(2);
        blockTypes.First().Should().Be(("DataGroup`1:Payload", true));
        blockTypes.Skip(1).First().Should().Be(("DataGroup`1:Payload2", false));
        blockTypes.ToString().Should().Be(shouldResult);

        string request = blockTypes.ToString();

        BlockTypeRequest parsedRequest = BlockTypeRequest.Parse(request);
        parsedRequest.Should().NotBeNull();
        parsedRequest.Count().Should().Be(2);
        parsedRequest.First().Should().Be(("DataGroup`1:Payload", true));
        parsedRequest.Skip(1).First().Should().Be(("DataGroup`1:Payload2", false));
        parsedRequest.ToString().Should().Be(shouldResult);
    }

    private record RecordA (string Name, int Index);
    private record RecordB (string Name, int Index);
    private record Payload(string Name, string Value);
    private record Payload2(int Id, string Data);
}
