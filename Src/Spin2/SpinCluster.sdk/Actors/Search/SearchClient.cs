using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Tools.Table;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Search;

public class SearchClient
{
    private readonly HttpClient _client;
    public SearchClient(HttpClient client) => _client = client.NotNull();

    public Task<Option<IReadOnlyList<StorePathItem>>> Query(QueryParameter queryParameter, ScopeContext context)
    {
        return Query(queryParameter.Filter, queryParameter.Index, queryParameter.Count, queryParameter.Recurse, context);
    }

    public async Task<Option<IReadOnlyList<StorePathItem>>> Query(string? filter, int? index, int? count, bool? recurse, ScopeContext context)
    {
        string query = new string?[]
        {
            index?.ToString()?.Func(x => $"index={x}"),
            count?.ToString()?.Func(x => $"count={x}"),
            recurse?.ToString()?.Func(x => $"recurse={x}"),
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

    public async Task<Option<ObjectTable>> Load(QueryParameter queryParameter, ScopeContext context)
    {
        try
        {
            Option<IReadOnlyList<StorePathItem>> batch = await Query(queryParameter, context);
            if (batch.IsError()) return batch.ToOption<ObjectTable>();

            string? tenant = queryParameter?.Filter?.Split('/')?.First();

            ObjectRow[] rows = batch.Return().Select(x => new ObjectRow(new object?[]
                {
                    x.Name.Split('/').Skip(1).Join('/'),
                    x.LastModified
                }, createTag(x), createName(tenant, x.Name))
            ).ToArray();

            ObjectTable table = new ObjectTableBuilder()
                .AddColumn(new[]
                {
                    "Name",
                    "LastModified"
                })
                .AddRow(rows)
                .Build();

            return table;
        }
        catch (OperationCanceledException ex)
        {
            string msg = "Query timed out";
            context.Location().LogError(ex, msg);
            return new Option<ObjectTable>(StatusCode.ServiceUnavailable, msg);
        }

        string createTag(StorePathItem item) => item.IsDirectory == true ? SpinConstants.Folder : SpinConstants.Open;
        string createName(string? tenant, string name) => tenant != null ? tenant + "/" + name : name;
    }
}
