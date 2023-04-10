using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Models;
using Toolbox.Protocol;
using Toolbox.Security.Sign;
using Toolbox.Sign;
using Toolbox.Store;

namespace InstallmentContract.Provider.Test.TestServices;

public class TestDocumentStore : IBlockDocumentStore
{
    public Func<DocumentId, bool>? DeleteFunc { get; set; }
    public Func<DocumentId, bool>? ExistFunc { get; set; }
    public Func<DocumentId, Document?>? GetFunc { get; set; }
    public Func<Document, bool>? SetFunc { get; set; }

    public Task<bool> Delete(DocumentId id, CancellationToken token)
    {
        bool response = DeleteFunc?.Invoke(id) ?? throw new InvalidOperationException();
        return Task.FromResult(response);
    }

    public Task<bool> Exists(DocumentId id, CancellationToken token)
    {
        bool response = ExistFunc?.Invoke(id) ?? throw new InvalidOperationException();
        return Task.FromResult(response);
    }

    public Task<Document?> Get(DocumentId id, CancellationToken token)
    {
        Document? response = GetFunc?.Invoke(id) ?? throw new InvalidOperationException();
        return Task.FromResult<Document?>(response);
    }

    public Task<IReadOnlyList<StorePathItem>> Search(QueryParameter queryParameter)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Set(Document document, CancellationToken token)
    {
        bool response = SetFunc?.Invoke(document) ?? throw new InvalidOperationException();
        return Task.FromResult(response);
    }
}
