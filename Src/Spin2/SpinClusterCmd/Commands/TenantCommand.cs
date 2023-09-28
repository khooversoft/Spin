using System.CommandLine;
using SpinClusterCmd.Activities;

namespace SpinClusterCmd.Commands;

internal class TenantCommand : Command
{
    private readonly Tenant _tenant;

    public TenantCommand(Tenant tenant) : base("tenant", "Tenant management")
    {
        _tenant = tenant;

        AddCommand(Delete());
        AddCommand(Get());
        AddCommand(Set());
    }

    private Command Delete()
    {
        var tenantId = new Argument<string>("tenantId", "Id of Tenant");

        var cmd = new Command("delete", "Delete a Tenant");
        cmd.AddArgument(tenantId);
        cmd.SetHandler(_tenant.Delete, tenantId);

        return cmd;
    }

    private Command Get()
    {
        var tenantId = new Argument<string>("tenantId", "Id of Tenant");

        var cmd = new Command("get", "Get Tenant details");
        cmd.AddArgument(tenantId);
        cmd.SetHandler(_tenant.Get, tenantId);

        return cmd;
    }

    private Command Set()
    {
        var jsonFile = new Argument<string>("jsonFile", "Json with Tenant details");

        var cmd = new Command("set", "Create or update Tenant details");
        cmd.AddArgument(jsonFile);
        cmd.SetHandler(_tenant.Set, jsonFile);

        return cmd;
    }
}
