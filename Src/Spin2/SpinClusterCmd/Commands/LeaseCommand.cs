using System.CommandLine;
using SpinClusterCmd.Activities;

namespace SpinClusterCmd.Commands;

internal class LeaseCommand : Command
{
    private readonly Lease _lease;

    public LeaseCommand(Lease lease) : base("lease", "Cluster lease resource management")
    {
        _lease = lease;

        AddCommand(Get());
        AddCommand(IsValid());
        AddCommand(List());
        AddCommand(Release());
    }

    private Command Get()
    {
        var leaseKey = new Argument<string>("leaseKey", "Lease key to get");

        var cmd = new Command("get", "Get lease details");
        cmd.AddArgument(leaseKey);
        cmd.SetHandler(_lease.Get, leaseKey);

        return cmd;
    }

    private Command IsValid()
    {
        var leaseKey = new Argument<string>("leaseKey", "Lease key to check");

        var cmd = new Command("isValid", "Is lease valid");
        cmd.AddArgument(leaseKey);
        cmd.SetHandler(_lease.IsValid, leaseKey);

        return cmd;
    }

    private Command List()
    {
        var cmd = new Command("list", "List active leases");
        cmd.SetHandler(_lease.List);

        return cmd;
    }

    private Command Release()
    {
        var leaseKey = new Argument<string>("leaseKey", "Lease key to release");

        var cmd = new Command("release", "Release an active lease");
        cmd.SetHandler(_lease.Release, leaseKey);

        return cmd;
    }
}
