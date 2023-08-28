//using SpinCluster.sdk.Application;
//using Toolbox.Rest;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace SpinCluster.sdk.Actors.Configuration;

//public class ConfigurationClient
//{
//    private readonly HttpClient _client;
//    public ConfigurationClient(HttpClient client) => _client = client.NotNull();

//    public async Task<Option<SiloConfigOption>> Get(ScopeContext context) => await new RestClient(_client)
//        .SetPath("configuration")
//        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
//        .GetAsync(context)
//        .GetContent<SiloConfigOption>();

//    public async Task<Option> Set(SiloConfigOption request, string leaseId, ScopeContext context) => await new RestClient(_client)
//        .SetPath("configuration")
//        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
//        .SetContent(request)
//        .PostAsync(context)
//        .GetContent<Option>()
//        .UnwrapAsync();
//}
