using System.CommandLine;
using SpinClusterCmd.Activities;

namespace SpinClusterCmd.Commands;

internal class ScheduleCommand : Command
{
    private readonly EnqueueCommand _enqueueCommand;

    public ScheduleCommand(EnqueueCommand enqueueCommand) : base("schedule", "Create, clear, dump schedule queues")
    {
        _enqueueCommand = enqueueCommand;

        AddCommand(Create());
    }

    private Command Create()
    {
        var jsonFile = new Argument<string>("file", "Json file command details");

        var cmd = new Command("create", "Create schedule");

        cmd.AddArgument(jsonFile);
        cmd.SetHandler(_enqueueCommand.Enqueue, jsonFile);

        return cmd;
    }
}
