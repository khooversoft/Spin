using DirectoryCmd.Activities;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace DirectoryCmd.Application;

internal class ListCommand : Command
{
    public ListCommand(IServiceProvider serviceProvider)
        : base("list", "List entries")
    {
        AddArgument(new Argument<string>("path", "Root path"));
        AddOption(new Option<bool>(new string[] { "--recursive", "-r" }, "Recursive listing"));

        Handler = CommandHandler.Create(async (string path, bool recursive, CancellationToken token) =>
        {
            await serviceProvider.GetRequiredService<ListActivity>().List(path, recursive, token);
            return 0;
        });
    }
}
