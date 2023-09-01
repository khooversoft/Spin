//using System.CommandLine;
//using Microsoft.Extensions.Logging;
//using SpinCluster.sdk.Actors.Contract;
//using SpinCluster.sdk.Actors.Directory;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace SpinClusterCmd.Commands;

//internal class DirectoryCommand : Command
//{
//    private readonly DirectoryClient _client;
//    private readonly ILogger<DirectoryCommand> _logger;

//    public DirectoryCommand(DirectoryClient client, ILogger<DirectoryCommand> logger)
//        : base("directory", "Directory register")
//    {
//        _client = client.NotNull();
//        _logger = logger.NotNull();

//        AddCommand(DeleteCommand());
//        AddCommand(GetCommand());
//        AddCommand(ListCommand());
//        AddCommand(SetCommand());
//    }

//    private Command DeleteCommand()
//    {
//        var cmd = new Command("delete", "Delete directory");
//        Argument<string> resourceId = new Argument<string>("resourceId", "Resource ID to delete");

//        cmd.AddArgument(resourceId);

//        cmd.SetHandler(async (resourceId) =>
//        {
//            var context = new ScopeContext(_logger);
//            Toolbox.Types.Option response = await _client.Delete(resourceId, context);

//            context.Location().Log(
//                response.StatusCode.IsOk() ? LogLevel.Information : LogLevel.Error,
//                "Delete directory, statusCode={statusCode}",
//                response.StatusCode
//                );

//        }, resourceId);

//        return cmd;
//    }

//    private Command GetCommand()
//    {
//        var cmd = new Command("get", "Get directory item");
//        Argument<string> resourceId = new Argument<string>("resourceId", "Resource ID to return");
//        System.CommandLine.Option<string> writeToFile = new System.CommandLine.Option<string>("--file", "Write to file");

//        cmd.AddArgument(resourceId);
//        cmd.AddOption(writeToFile);

//        cmd.SetHandler(async (resourceId, writeToFile) =>
//        {
//            var context = new ScopeContext(_logger);

//            Toolbox.Types.Option<DirectoryEntry> response = await _client.Get(resourceId, context);

//            switch ((response.IsOk(), writeToFile.IsNotEmpty()))
//            {
//                case (true, true):
//                    context.Location().LogInformation("Writing to file={file}", writeToFile);
//                    await File.WriteAllTextAsync(writeToFile, response.Return().ToJson());
//                    break;

//                case (true, false):
//                    context.Location().LogInformation("Data: {data}", response.Return().ToJson());
//                    break;

//                default:
//                    context.Location().LogError("Failed to get directory, statusCode={statusCode}, error={error}", response.StatusCode, response.Error);
//                    break;
//            }

//        }, resourceId, writeToFile);

//        return cmd;
//    }

//    private Command ListCommand()
//    {
//        var cmd = new Command("list", "List all directory items");

//        cmd.SetHandler(async () =>
//        {
//            var context = new ScopeContext(_logger);

//            var query = new DirectoryQuery
//            {
//            };

//            Toolbox.Types.Option<IReadOnlyList<DirectoryEntry>> response = await _client.List(query, context);
//            if (response.IsError())
//            {
//                context.Location().LogError("Failed to list directory, statusCode={statusCode}, error={error}", response.StatusCode, response.Error);
//                return;
//            }

//            var list = response.Return()
//                .Select(x => x.ToString())
//                .Join(Environment.NewLine);

//            context.Location().LogInformation(list);
//        });

//        return cmd;
//    }

//    private Command SetCommand()
//    {
//        var cmd = new Command("set", "Add or update directory item");
//        Argument<string> file = new Argument<string>("file", "Json file to read");

//        cmd.SetHandler(async (file) =>
//        {
//            var context = new ScopeContext(_logger);

//            string json = File.ReadAllText(file);
//            DirectoryEntry? entry = json.ToObject<DirectoryEntry>();
//            if (entry == null)
//            {
//                context.Location().LogError("Failed to deserialize json for DirectoryEntry");
//                return;
//            }

//            var response = await _client.Set(entry, context);
//            if (response.IsError())
//            {
//                context.Location().LogError("Failed to list directory, statusCode={statusCode}, error={error}", response.StatusCode, response.Error);
//                return;
//            }

//            context.Location().LogInformation("File={file} was upload to the Spin cluster");
//        }, file);

//        return cmd;
//    }
//}
