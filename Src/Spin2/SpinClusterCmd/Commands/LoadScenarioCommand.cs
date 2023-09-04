using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinClusterCmd.Activities;
using Toolbox.Types;

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
