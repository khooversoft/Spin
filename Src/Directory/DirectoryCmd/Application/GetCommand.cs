using DirectoryCmd.Activities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;

namespace DirectoryCmd.Application;

internal class GetCommand : Command
{
    public GetCommand(IServiceProvider serviceProvider)
        : base("get", "Write file to artifact")
    {
        AddCommand(new GetFileCommand(serviceProvider));
        AddCommand(new DumpProperty(serviceProvider));
    }

    private class GetFileCommand : Command
    {
        public GetFileCommand(IServiceProvider serviceProvider)
            : base("file", "Write entries to file")
        {
            AddArgument(new Argument<string>("file", "File to write directory entries to"));
            AddOption(new Option<string?>(new string[] { "--path", "-P" }, "Path for searching directory entries, default is root"));
            AddOption(new Option<bool>(new string[] { "--recursive", "-r" }, "Recursive search"));

            Handler = CommandHandler.Create(async (string file, string? path, bool recursive, CancellationToken token) =>
            {
                path ??= "/";
                await serviceProvider.GetRequiredService<GetActivity>().WriteFile(file, path, recursive, token);
                return 0;
            });
        }
    }

    private  class DumpProperty : Command
    {
        public DumpProperty(IServiceProvider serviceProvider)
            : base("dump", "Dump property of an directory entry")
        {
            AddArgument(new Argument<string>("directoryId", "Directory id of entry"));

            Handler = CommandHandler.Create(async (string directoryId, CancellationToken token) =>
            {
                await serviceProvider.GetRequiredService<GetActivity>().DumpProperty(directoryId, token);
                return 0;
            });
        }
    }
}

