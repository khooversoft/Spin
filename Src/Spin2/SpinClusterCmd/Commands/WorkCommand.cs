using System.CommandLine;
using SpinClusterCmd.Activities;

namespace SpinClusterCmd.Commands;

internal class WorkCommand : Command
{
    private readonly ScheduleWork _work;

    public WorkCommand(ScheduleWork work) : base("work", "Schedule work items")
    {
        _work = work;

        AddCommand(Delete());
        AddCommand(Get());
        AddCommand(Release());
    }

    private Command Delete()
    {
        var workId = new Argument<string>("workId", "WorkId ex. 'schedulework:WKID-9a8bf61d-e0b4-4c99-98da-ddca139a988f'}");

        var cmd = new Command("delete", "Delete schedule work");

        cmd.AddArgument(workId);
        cmd.SetHandler(_work.Delete, workId);

        return cmd;
    }

    private Command Get()
    {
        var workId = new Argument<string>("workId", "WorkId ex. 'schedulework:WKID-9a8bf61d-e0b4-4c99-98da-ddca139a988f'}");

        var cmd = new Command("get", "Get schedules");

        cmd.AddArgument(workId);
        cmd.SetHandler(_work.Get, workId);

        return cmd;
    }

    private Command Release()
    {
        var workId = new Argument<string>("workId", "WorkId ex. 'schedulework:WKID-9a8bf61d-e0b4-4c99-98da-ddca139a988f'}");

        var cmd = new Command("release", "Release assignment of work");

        cmd.AddArgument(workId);
        cmd.SetHandler(_work.ReleaseAssign, workId);

        return cmd;
    }
}
