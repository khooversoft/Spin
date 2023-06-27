using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;

namespace SpinCluster.sdk.Client;

public class SpinResourceClient
{
    private readonly HttpClient _client;
    public SpinResourceClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option<IReadOnlyList<StorePathItem>>> Search(string filter, int? index, int? start, ScopeContext context)
    {
        string query = new string?[]
        {
            index?.ToString(),
            start?.ToString(),
        }
        .Where(x => x != null)
        .Join("&")
        .Func(x => x.IsNotEmpty() ? "?" + x : string.Empty);

        return await new RestClient(_client)
            .SetPath($"/search/{filter}{query}")
            .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
            .GetAsync(context)
            .GetContent<IReadOnlyList<StorePathItem>>();
    }
}
