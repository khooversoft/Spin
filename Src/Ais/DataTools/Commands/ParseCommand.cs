using DataTools.Activities;
using DataTools.Services;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;
using Toolbox.Tools;

namespace DataTools.Commands
{
    internal class ParseCommand : Command
    {
        private readonly ILogger<ParseCommand> _logger;

        public ParseCommand(ParseActivity parseActivity, AisStore aisStore, ILogger<ParseCommand> logger)
            : base("parse", "Parse file to AIS messages")
        {
            _logger = logger;

            Add(new Argument<string>("store", "AIS parsed store"));
            Add(new Argument<string[]>("file", "File(s) to analyze, can use wild card syntax"));
            Add(new Option("--recursive", "Recursive search if wild card syntax is used"));
            Add(new Option("--resetStore", "Reset (clear) the store"));
            Add(new Option<int?>("--max", "Maximum number of files to process"));

            Handler = CommandHandler.Create(async (string store, string[] file, bool recursive, bool resetStore, int? max, CancellationToken token) =>
            {
                _logger.LogInformation($"Store={store}");
                aisStore.SetStoreFolder(store);

                if (resetStore) await aisStore.RestStore();

                await parseActivity.Parse(file, recursive, max, token);
            });
        }
    }
}
