using System.CommandLine;
using SpinClusterCmd.Activities;

namespace SpinClusterCmd.Commands;

internal class LoadScenarioCommand : Command
{
    public LoadScenarioCommand(LoadScenario loadScenario) : base("load", "Load scenario")
    {
        var jsonFile = new Argument<string>("file", "Json file with scenario details");

        this.AddArgument(jsonFile);
        this.SetHandler(loadScenario.Load, jsonFile);
    }
}
