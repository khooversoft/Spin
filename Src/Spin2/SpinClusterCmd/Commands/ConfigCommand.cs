using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinClusterCmd.Activities;

namespace SpinClusterCmd.Commands;

internal class ConfigCommand : Command
{
    private readonly Configuration _configuration;

    public ConfigCommand(Configuration configuration) : base("config", "Configuration")
    {
        _configuration = configuration;

        AddCommand(Create());
        AddCommand(SetProperty());
        AddCommand(RemoveProperty());
        AddCommand(Get());
    }

    private Command Create()
    {
        var jsonFile = new Argument<string>("jsonFile", "Json file with package details");

        var cmd = new Command("create", "Create SmartC package");
        cmd.AddArgument(jsonFile);
        cmd.SetHandler(_configuration.Create, jsonFile);

        return cmd;
    }

    private Command SetProperty()
    {
        var configId = new Argument<string>("configId", "Configuration ID, ex: config:{name}");
        var key = new Argument<string>("key", "Key of property");
        var value = new Argument<string>("value", "Value of property");

        var cmd = new Command("set", "Set property in configuration");
        cmd.AddArgument(configId);
        cmd.AddArgument(key);
        cmd.AddArgument(value);
        cmd.SetHandler(_configuration.SetProperty, configId, key, value);

        return cmd;
    }

    private Command RemoveProperty()
    {
        var configId = new Argument<string>("configId", "Configuration ID, ex: config:{name}");
        var key = new Argument<string>("key", "Key of property");

        var cmd = new Command("remove", "Remove property in configuration");
        cmd.AddArgument(configId);
        cmd.AddArgument(key);
        cmd.SetHandler(_configuration.RemoveProperty, configId, key);

        return cmd;
    }

    private Command Get()
    {
        var configId = new Argument<string>("configId", "Configuration ID, ex: config:{name}");

        var cmd = new Command("Get", "Get properties in configuration");
        cmd.AddArgument(configId);
        cmd.SetHandler(_configuration.Get, configId);

        return cmd;
    }
}
