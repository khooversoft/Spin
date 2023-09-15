using System.CommandLine;
using SpinClusterCmd.Activities;

namespace SpinClusterCmd.Commands;

internal class ScheduleCommand : Command
{
    private readonly Schedule _schedule;

    public ScheduleCommand(Schedule schedule) : base("schedule", "Create, clear, dump schedule queues")
    {
        _schedule = schedule;

        AddCommand(Add());
        AddCommand(Clear());
        AddCommand(Get());
    }

    private Command Add()
    {
        var jsonFile = new Argument<string>("file", "Json file command details");

        var cmd = new Command("add", "Add a schedule");

        cmd.AddArgument(jsonFile);
        cmd.SetHandler(_schedule.Add, jsonFile);

        return cmd;
    }

    private Command Clear()
    {
        var principalId = new Argument<string>("principalId", "PrincipalId, ex. {user}@{domain}");

        var cmd = new Command("clear", "Clear schedule queues");

        cmd.AddArgument(principalId);
        cmd.SetHandler(_schedule.Clear, principalId);

        return cmd;
    }

    private Command Get()
    {
        var cmd = new Command("get", "Get schedules");

        cmd.SetHandler(_schedule.Get);

        return cmd;
    }
}
