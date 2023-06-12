﻿using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IInMemoryStore
{
    Task<StatusCode> CreateIfNotExists(Document document, ScopeContext context);
    Task<StatusCode> Delete(string id, ScopeContext context, string? eTag = null, string? leaseId = null);
    Task<StatusCode> Exists(string id, ScopeContext context);
    Task<Option<Document>> Get(string id, string? eTag = null);
    Task<StatusCode> Set(Document document, ScopeContext context, string? eTag = null, string? leaseId = null);
}

//public class InMemoryStore : IInMemoryStore
//{
//    private readonly DocumentObjectLease _lease;
//    private readonly ILogger<InMemoryStore> _logger;
//    private ConcurrentDictionary<string, Payload> _store = new();
//    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

//    public InMemoryStore(DocumentObjectLease lease, ILogger<InMemoryStore> logger)
//    {
//        _lease = lease.NotNull();
//        _logger = logger.NotNull();
//    }

//    public async Task<StatusCode> CreateIfNotExists(Document document, ScopeContext context)
//    {
//        context.With(_logger);
//        document.Verify();

//        await _lock.WaitAsync();
//        try
//        {
//            if ((await Exists(document.ObjectId, context)) == StatusCode.NotFound) return StatusCode.NotFound;

//            await InternalSet(document);
//            context.Location().LogInformation("Created document id={documentId}", document.ObjectId);

//            return StatusCode.OK;
//        }
//        finally
//        {
//            _lock.Release();
//        }
//    }

//    public async Task<StatusCode> Delete(string id, ScopeContext context, string? eTag = null, string? leaseId = null)
//    {
//        context.With(_logger);
//        id.NotEmpty();

//        await _lock.WaitAsync();
//        try
//        {
//            var status = await CanProceed(id, eTag, leaseId, context);
//            if (status.IsError()) return status;

//            return _store.TryRemove(id, out _) switch
//            {
//                true => StatusCode.OK.Action(x => context.Location().LogInformation("Removed id={id}", id)),
//                false => StatusCode.NotFound,
//            };
//        }
//        finally
//        {
//            _lock.Release();
//        }
//    }

//    public Task<StatusCode> Exists(string id, ScopeContext context) => _store.ContainsKey(id) switch
//    {
//        false => Task.FromResult(StatusCode.NotFound),
//        true => Task.FromResult(StatusCode.OK),
//    };

//    public async Task<Option<Document>> Get(string id, string? eTag = null)
//    {
//        context.With(_logger);
//        id.NotEmpty();

//        await _lock.WaitAsync();
//        try
//        {
//            if (!(await IsEtagCurrent(id, eTag))) return new Option<Document>(StatusCode.Conflict);

//            return await InternalGet(id);
//        }
//        finally
//        {
//            _lock.Release();
//        }
//    }

//    public async Task<StatusCode> Set(Document document, ScopeContext context, string? eTag = null, string? leaseId = null)
//    {
//        context.With(_logger);
//        document.Verify();

//        await _lock.WaitAsync();
//        try
//        {
//            var status = await CanProceed(document.ObjectId, eTag, leaseId, context);
//            if (status.IsError()) return status;

//            await InternalSet(document);

//            return StatusCode.OK;
//        }
//        finally
//        {
//            _lock.Release();
//        }
//    }

//    private async Task<StatusCode> CanProceed(string id, string? eTag, string? leaseId, ScopeContext context)
//    {
//        if (_lease.IsLeased(id, leaseId))
//        {
//            context.Location().LogWarning("Locked id={id}", id);
//            return StatusCode.Forbidden;
//        }

//        if (!(await IsEtagCurrent(id, eTag)))
//        {
//            context.Location().LogWarning("ETag conflict id={id}", id);
//            return StatusCode.Conflict;
//        }

//        return StatusCode.OK;
//    }

//    private async Task<bool> IsEtagCurrent(string id, string? eTag)
//    {
//        return eTag switch
//        {
//            string e => await InternalGet(id) switch
//            {
//                var o when o.StatusCode == StatusCode.NotFound => true,
//                var o when o.Return().ETag == e => true,
//                _ => false,
//            },

//            _ => true,
//        };
//    }

//    private Task<Option<Document>> InternalGet(string id) => _store.TryGetValue(id, out Payload result) switch
//    {
//        false => Task.FromResult(StatusCode.NotFound.ToOption<Document>()),
//        _ => Task.FromResult(new Option<Document>(result.Data.ToDocument())),
//    };

//    private Task InternalSet(Document document)
//    {
//        byte[] bytes = document.ToBytes();
//        var payload = new Payload
//        {
//            Document = document,
//            Data = bytes,
//        };

//        _store[document.ObjectId] = payload;

//        return Task.CompletedTask;
//    }

//    private readonly struct Payload
//    {
//        public Document Document { get; init; }
//        public byte[] Data { get; init; }
//    }
//}
