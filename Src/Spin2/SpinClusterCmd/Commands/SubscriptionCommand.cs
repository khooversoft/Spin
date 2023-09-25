using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinClusterCmd.Activities;

namespace SpinClusterCmd.Commands;

internal class SubscriptionCommand : Command
{
    private readonly Subscription _subscription;

    public SubscriptionCommand(Subscription subscription) : base("subscription", "Subscription management")
{
        _subscription = subscription;

        AddCommand(Delete());
        AddCommand(Get());
        AddCommand(Set());
    }

    private Command Delete()
    {
        var nameId = new Argument<string>("nameId", "Name of subscription");

        var cmd = new Command("delete", "Delete a subscription");
        cmd.AddArgument(nameId);
        cmd.SetHandler(_subscription.Delete, nameId);

        return cmd;
    }

    private Command Get()
    {
        var nameId = new Argument<string>("nameId", "Name of subscription");

        var cmd = new Command("get", "Get subscription details");
        cmd.AddArgument(nameId);
        cmd.SetHandler(_subscription.Get, nameId);

        return cmd;
    }

    private Command Set()
    {
        var jsonFile = new Argument<string>("jsonFile", "Json with subscription details");

        var cmd = new Command("set", "Create or update subscription details");
        cmd.AddArgument(jsonFile);
        cmd.SetHandler(_subscription.Set, jsonFile);

        return cmd;
    }
}
