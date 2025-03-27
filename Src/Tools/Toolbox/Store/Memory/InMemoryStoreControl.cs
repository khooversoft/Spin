using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Logging;
using Toolbox.Tools.Should;
using System.Collections.Immutable;

namespace Toolbox.Store;

internal class InMemoryStoreControl
{
    private record Payload(StorePathDetail PathDetail, DataETag Data, LeaseRecord? LeaseRecord);

    private readonly ConcurrentDictionary<string, Payload> _store = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger _logger;

    public InMemoryStoreControl(ILogger logger) => _logger = logger.NotNull();

    public int Count => _store.Count;

    public Task<Option<string>> Add(string path, DataETag data, ScopeContext context)
    {
        context = context.With(_logger);
        return addInternal(path, data, context).ToTaskResult();

        Option<string> addInternal(string path, DataETag data, ScopeContext context)
        {
            if (!FileStoreTool.IsPathValid(path)) return (StatusCode.BadRequest, "Bad path");

            Option<string> result = _store.TryAdd(path, new Payload(data.ConvertTo(path), data, null)) switch
            {
                true => data.ETag.NotEmpty(),
                false => (StatusCode.Conflict, $"path={path} already exist"),
            };

            result.LogStatus(context, "Add Path={path}", [path]);
            return result;
        }
    }

    public Task<Option> Append(string path, DataETag data, ScopeContext context, string? leaseId = null)
    {
        DataETag value = data;

        var result = _store.AddOrUpdate(path, p => new Payload(data.ConvertTo(path), data, null), (p, current) =>
        {
            var appendedData = current.Data.Data.Concat(data.Data).ToDataETag().WithHash();

            var payload = current with
            {
                Data = appendedData,
                PathDetail = current.PathDetail with
                {
                    ContentLength = current.PathDetail.ContentLength + data.Data.Length,
                    ETag = appendedData.ETag.NotEmpty(),
                },
            };

            return payload;
        });

        context.LogTrace("Append Path={path} with {length} bytes", path, data.Data.Length);
        return new Option(StatusCode.OK).ToTaskResult();
    }

    public Task<Option> Delete(string path, ScopeContext context)
    {
        if (!FileStoreTool.IsPathValid(path)) return new Option(StatusCode.BadRequest).ToTaskResult();

        Option option = _store.TryRemove(path, out var _) switch
        {
            true => StatusCode.OK,
            false => (StatusCode.NotFound, $"path={path} does not exist"),
        };

        option.LogStatus(context, "Delete, path={path}", [path]);
        return option.ToTaskResult();
    }

    public Task<Option> DeleteFolder(string path, ScopeContext context)
    {
        return deleteFolderInternal(path, context).ToTaskResult();

        Option deleteFolderInternal(string path, ScopeContext context)
        {
            if (!FileStoreTool.IsPathValid(path)) return StatusCode.BadRequest;
            if (_store.ContainsKey(path)) return (StatusCode.Conflict, "Not a folder");

            var list = _store.Keys.Where(x => x.StartsWith(path, StringComparison.OrdinalIgnoreCase)).ToArray();
            list.ForEach(x => _store.TryRemove(x, out var _));
            return StatusCode.OK;
        }
    }

    public Task<Option> Exist(string path, ScopeContext context)
    {
        if (!FileStoreTool.IsPathValid(path)) return new Option(StatusCode.BadRequest).ToTaskResult();

        Option option = _store.ContainsKey(path) switch
        {
            true => StatusCode.OK,
            false => (StatusCode.NotFound, $"path={path} does not exist"),
        };

        return option.ToTaskResult();
    }

    public Task<Option<DataETag>> Get(string path, ScopeContext context)
    {
        if (!FileStoreTool.IsPathValid(path)) return new Option<DataETag>(StatusCode.BadRequest).ToTaskResult();

        Option<DataETag> option = _store.TryGetValue(path, out var value) switch
        {
            true => value.Data,
            false => (StatusCode.NotFound, $"path={path} does not exist"),
        };

        option.LogStatus(context, "Get Path", [path]);
        return option.ToTaskResult();
    }

    public Task<Option<IStorePathDetail>> GetDetail(string path, ScopeContext context)
    {
        Option<IStorePathDetail> option = _store.TryGetValue(path, out var value) switch
        {
            true => value.PathDetail,
            false => (StatusCode.NotFound, $"path={path} does not exist"),
        };

        option.LogStatus(context, "Get Path", [path]);
        return option.ToTaskResult();
    }

    public async Task<IReadOnlyList<string>> Search(string pattern, ScopeContext context)
    {
        var list = (await DetailSearch(pattern, context))
            .Select(x => x.Path)
            .ToImmutableArray();

        return list;
    }

    public Task<IReadOnlyList<IStorePathDetail>> DetailSearch(string pattern, ScopeContext context)
    {
        var query = QueryParameter.Parse(pattern).GetMatcher();

        IReadOnlyList<IStorePathDetail> list = _store.Values
            .Where(x => pattern switch
            {
                "*" => true,
                _ => query.IsMatch(x.PathDetail.Path, false)
            })
            .Select(x => x.PathDetail)
            .OfType<IStorePathDetail>()
            .ToImmutableArray();

        return list.ToTaskResult();
    }

    public Task<Option<string>> Set(string path, DataETag data, ScopeContext context)
    {
        context = context.With(_logger);
        return setInternal(path, data, context).ToTaskResult();

        Option<string> setInternal(string path, DataETag data, ScopeContext context)
        {
            if (!FileStoreTool.IsPathValid(path)) return (StatusCode.BadRequest, "Bad path");

            var result = _store.AddOrUpdate(path, x => new Payload(data.ConvertTo(x), data, null), (x, current) =>
            {
                var payload = current with
                {
                    Data = data,
                    PathDetail = current.PathDetail with
                    {
                        LastModified = DateTimeOffset.UtcNow,
                        ETag = data.ToHash()
                    },
                };

                return payload;
            });

            return result.PathDetail.ETag;
        }
    }
}
