using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Types.Maybe;

namespace Toolbox.DocumentContainer;

public interface IDocumentStore
{
    Task<OptionStatus> CreateIfNotExists(Document document, ScopeContext context);
    Task<OptionStatus> Delete(string id, ScopeContext context, string? eTag = null, string? leaseId = null);
    Task<Option> Exists(string id, ScopeContext context);
    Task<Option<Document>> Get(string id, string? eTag = null);
    Task<OptionStatus> Set(Document document, ScopeContext context, string? eTag = null, string? leaseId = null);
}

public class DocumentStoreInMemory : IDocumentStore
{
    private readonly DocumentLease _lease;
    private readonly ILogger<DocumentStoreInMemory> _logger;
    private ConcurrentDictionary<string, Payload> _store = new();
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    public DocumentStoreInMemory(DocumentLease lease, ILogger<DocumentStoreInMemory> logger)
    {
        _lease = lease.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<OptionStatus> CreateIfNotExists(Document document, ScopeContext context)
    {
        document.Verify();

        await _lock.WaitAsync();
        try
        {
            if ((await Exists(document.DocumentId, context)) == OptionStatus.NotFound) return OptionStatus.NotFound;

            await InternalSet(document);
            _logger.LogInformation(context.Location(), "Created document id={documentId}", document.DocumentId);

            return OptionStatus.OK.ToOption();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<OptionStatus> Delete(string id, ScopeContext context, string? eTag = null, string? leaseId = null)
    {
        id.NotEmpty();

        await _lock.WaitAsync();
        try
        {
            if (_lease.IsLeased(id, leaseId))
            {
                _logger.LogWarning(context.Location(), "Locked id={id}", id);
                return OptionStatus.Forbidden;
            }

            if (!(await IsEtagCurrent(id, eTag)))
            {
                _logger.LogWarning(context.Location(), "ETag conflict id={id}", id);
                return OptionStatus.Conflict;
            }

            return _store.TryRemove(id, out _) switch
            {
                true => OptionStatus.OK.Action(x => _logger.LogInformation(context.Location(), "Removed id={id}", id)),
                false => OptionStatus.NotFound,
            };
        }
        finally
        {
            _lock.Release();
        }
    }

    public Task<Option> Exists(string id, ScopeContext context) => _store.ContainsKey(id) switch
    {
        false => Task.FromResult(new Option(OptionStatus.NotFound)),
        true => Task.FromResult(new Option(OptionStatus.OK)),
    };

    public async Task<Option<Document>> Get(string id, string? eTag = null)
    {
        id.NotEmpty();

        await _lock.WaitAsync();
        try
        {
            if (!(await IsEtagCurrent(id, eTag))) return new Option<Document>(OptionStatus.Conflict);

            return await InternalGet(id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<OptionStatus> Set(Document document, ScopeContext context, string? eTag = null, string? leaseId = null)
    {
        document.Verify();

        await _lock.WaitAsync();
        try
        {
            if (_lease.IsLeased(document.DocumentId, leaseId))
            {
                _logger.LogWarning(context.Location(), "Locked id={id}", document.DocumentId);
                return OptionStatus.Forbidden;
            }

            if (!(await IsEtagCurrent(document.DocumentId, eTag)))
            {
                _logger.LogWarning(context.Location(), "ETag conflict id={id}", document.DocumentId);
                return OptionStatus.Conflict;
            }

            await InternalSet(document);

            return OptionStatus.OK;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<bool> IsEtagCurrent(string id, string? eTag)
    {
        return eTag switch
        {
            string e => await InternalGet(id) switch
            {
                var o when o.StatusCode == OptionStatus.NotFound => true,
                var o when o.Return().ETag == e => true,
                _ => false,
            },

            _ => true,
        };
    }

    private Task<Option<Document>> InternalGet(string id) => _store.TryGetValue(id, out Payload result) switch
    {
        false => Task.FromResult(OptionStatus.NotFound.ToOption<Document>()),
        _ => Task.FromResult(new Option<Document>(result.Data.ToDocument())),
    };

    private Task InternalSet(Document document)
    {
        byte[] bytes = document.ToBytes();
        var payload = new Payload
        {
            Document = document,
            Data = bytes,
        };

        _store[document.DocumentId] = payload;

        return Task.CompletedTask;
    }

    private readonly struct Payload
    {
        public Document Document { get; init; }
        public byte[] Data { get; init; }
    }
}
