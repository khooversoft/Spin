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

    public async Task<Option<IReadOnlyList<StorePathItem>>> Query(SearchQuery searchQuery, ScopeContext context)
    {
        searchQuery.NotNull();

        string query = new string[]
        {
            $"schema={searchQuery.Schema}",
            $"tenant={searchQuery.Tenant}",
            $"filter={searchQuery.Filter}",
            $"index={searchQuery.Index}",
            $"count={searchQuery.Count}",
            $"recurse={searchQuery.Recurse}",
        }.Join('&');

        return await new RestClient(_client)
            .SetPath($"/search?{query}")
            .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
            .GetAsync(context)
            .GetContent<IReadOnlyList<StorePathItem>>();
    }

    public async Task<Option<ObjectTable>> Load(SearchQuery searchQuery, ScopeContext context)
    {
        try
        {
            Option<IReadOnlyList<StorePathItem>> response = await Query(searchQuery, context);
            if (response.IsError()) return response.ToOptionStatus<ObjectTable>();

            ObjectRow[] rows = response.Return().Select(x => new ObjectRow(new object?[]
                {
                    x.Name.Split('/').Skip(1).Join('/'),
                    x.LastModified
                }, createTag(x), createName(searchQuery.Schema, x.Name))
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
