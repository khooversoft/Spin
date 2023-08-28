using SoftBank.sdk.Application;
using SoftBank.sdk.Models;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk.Trx;

public class SoftBankTrxClient
{
    protected readonly HttpClient _client;
    public SoftBankTrxClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option<TrxResponse>> Request(TrxRequest request, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{IdSoftbank.SoftBankTrxSchema}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(request)
        .PostAsync(context)
        .GetContent<TrxResponse>();
}
