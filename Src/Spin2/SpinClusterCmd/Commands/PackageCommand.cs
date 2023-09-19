using System.CommandLine;
using SpinClusterCmd.Activities;

namespace SpinClusterCmd.Commands;

internal class PackageCommand : Command
{
    private readonly SmartcPackage _smartcPackage;

    public PackageCommand(SmartcPackage smartcPackage) : base("package", "Create or expand SmartC package")
    {
        _smartcPackage = smartcPackage;

        AddCommand(Create());
        AddCommand(Download());
    }

    private Command Create()
    {
        var jsonFile = new Argument<string>("jsonFile", "Json file with package details");
        var verboseOption = new Option<bool>("--verbose", "List all files being packaged");

        var cmd = new Command("create", "Create SmartC package");
        cmd.AddArgument(jsonFile);
        cmd.AddOption(verboseOption);
        cmd.SetHandler(_smartcPackage.CreateAndUpload, jsonFile, verboseOption);

        return cmd;
    }

    private Command Download()
    {
        var jsonFile = new Argument<string>("jsonFile", "Json file with package details");

        var cmd = new Command("download", "Upload SmartC package");
        cmd.AddArgument(jsonFile);
        cmd.SetHandler(_smartcPackage.Download, jsonFile);

        return cmd;
    }
}
