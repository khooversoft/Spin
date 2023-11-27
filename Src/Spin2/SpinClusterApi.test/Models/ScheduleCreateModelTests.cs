using FluentAssertions;
using SpinCluster.sdk.Actors.ScheduleWork;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinClusterApi.test.Models;

public class ScheduleCreateModelTests
{
    //private const string _agentId = "agent:test-agent";
    private const string _schedulerId = "scheduler:test";
    private const string _smartcId = "smartc:company30.com/contract1";
    private const string _principalId = "user1@company30.com";
    //private const string _contractId = "contract:company30.com/contract1";
    private const string _sourceId = "source1";
    private const string _command = "create";

    [Fact]
    public void ScheduleCreateModelNoPayloadValidation()
    {
        var model = new ScheduleCreateModel
        {
            SmartcId = _smartcId,
            SchedulerId = _schedulerId,
            PrincipalId = _principalId,
            SourceId = _sourceId,
            Command = _command,
        };

        Option v = model.Validate();
        v.IsOk().Should().BeTrue(v.ToString());
    }

    [Fact]
    public void ScheduleWithPayloadModelValidation()
    {
        var t = new Test("name1", "value1");
        var t2 = new Test2(15, "ca");

        var model = new ScheduleCreateModel
        {
            SmartcId = _smartcId,
            SchedulerId = _schedulerId,
            PrincipalId = _principalId,
            SourceId = _sourceId,
            Command = _command,
            Payloads = new DataObjectSet().Set(t).Set(t2)
        };

        Option v = model.Validate();
        v.IsOk().Should().BeTrue();

        model.Payloads.GetObject<Test>().Action(x =>
        {
            x.Should().NotBeNull();
            x.Name.Should().Be("name1");
            x.Value.Should().Be("value1");
            (t == x).Should().BeTrue();
        });

        model.Payloads.GetObject<Test2>().Action(x =>
        {
            x.Should().NotBeNull();
            x.Age.Should().Be(15);
            x.BirthPlace.Should().Be("ca");
            (t2 == x).Should().BeTrue();
        });
    }

    private record Test(string Name, string Value);
    private record Test2(int Age, string BirthPlace);
}
