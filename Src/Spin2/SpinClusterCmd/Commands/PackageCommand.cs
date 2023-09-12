using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinClusterCmd.Activities;

namespace SpinClusterCmd.Commands;

internal class PackageCommand : Command
{
    private readonly SmartcPackage _smartcPackage;

    public PackageCommand(SmartcPackage smartcPackage) : base("package", "Create or expand SmartC package")
    {
        _smartcPackage = smartcPackage;
    }

    private Command Package(string jsonFile
}
