//using SpinCluster.sdk.Actors.Directory;
//using SpinCluster.sdk.Application;
//using Toolbox.Rest;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace SpinCluster.sdk.Actors.Contract;

//public class DirectoryClient
//{
//    protected readonly HttpClient _client;
//    public DirectoryClient(HttpClient client) => _client = client.NotNull();

//    public async Task<Option> Delete(string resourceId, ScopeContext context) => await new RestClient(_client)
//        .SetPath($"/{SpinConstants.Schema.Directory}/{Uri.EscapeDataString(resourceId)}")
//        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
//        .DeleteAsync(context)
//        .ToOption();

//    public async Task<Option<DirectoryEntry>> Get(string resourceId, ScopeContext context) => await new RestClient(_client)
//        .SetPath($"/{SpinConstants.Schema.Directory}/{Uri.EscapeDataString(resourceId)}")
//        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
//        .GetAsync(context)
//        .GetContent<DirectoryEntry>();

//    public async Task<Option<IReadOnlyList<DirectoryEntry>>> List(DirectoryQuery query, ScopeContext context) => await new RestClient(_client)
//        .SetPath($"/{SpinConstants.Schema.Directory}/list")
//        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
//        .SetContent(query)
//        .PostAsync(context)
//        .GetContent<IReadOnlyList<DirectoryEntry>>();

//    public async Task<Option> Set(DirectoryEntry subject, ScopeContext context) => await new RestClient(_client)
//        .SetPath($"/{SpinConstants.Schema.Directory}")
//        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
//        .SetContent(subject)
//        .PostAsync(context)
//        .ToOption();
//}
