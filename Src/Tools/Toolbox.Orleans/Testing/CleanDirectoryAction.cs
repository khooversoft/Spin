//using Microsoft.Extensions.Logging;
//using Toolbox.Graph;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Orleans.Testing;

//public class CleanDirectoryAction
//{
//    private readonly IClusterClient _clusterClient;
//    private readonly ILogger<CleanDirectoryAction> _logger;

//    public CleanDirectoryAction(IClusterClient clusterClient, ILogger<CleanDirectoryAction> logger)
//    {
//        _clusterClient = clusterClient.NotNull();
//        _logger = logger.NotNull();
//    }

//    public async Task<Option> Clean()
//    {
//        var context = new ScopeContext(_logger);
//        var directory = _clusterClient.GetDirectoryActor();

//        string command = "select (*);";
//        Option<QueryResult> resultOption = await directory.Execute(command, context);
//        if (resultOption.IsError()) return resultOption.LogStatus(context, command).ToOptionStatus();

//        var nodes = resultOption.Return().Nodes.ToArray();
//        if (nodes.Length == 0) return StatusCode.OK;

//        foreach (var node in nodes)
//        {
//            var deleteOption = await directory.Execute($"delete (key={node.Key});", NullScopeContext.Instance);
//            if (deleteOption.IsError()) return deleteOption.LogStatus(context, $"Failed to delete key={node.Key}").ToOptionStatus();
//        }

//        return StatusCode.OK;
//    }
//}
