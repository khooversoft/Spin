using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public class TicketDataClient
{
    public const string Path = "config/TicketData.json";

    private readonly IFileStore _fileStore;
    private readonly ILogger<TicketDataClient> _logger;

    public TicketDataClient(IFileStore fileStore, ILogger<TicketDataClient> logger)
    {
        _fileStore = fileStore.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<TicketDataRecord>> Get(ScopeContext context)
    {
        context = context.With(_logger);

        Option<DataETag> dataETag = await _fileStore.Get(Path, context);
        dataETag.LogStatus(context, "Get ticket data model, path={path}", [Path]);
        if (dataETag.IsError()) return dataETag.ToOptionStatus<TicketDataRecord>();

        TicketDataRecord result = dataETag.Return().ToObject<TicketDataRecord>();
        return result;
    }

    public async Task<Option> CleatData(ScopeContext context)
    {
        context = context.With(_logger);

        var result = await _fileStore.Delete(Path, context);
        result.LogStatus(context, "Deticket data model, path={path}", [Path]);
        if (result.IsError()) return result;

        return result;
    }

    public async Task<Option<string>> Set(TicketDataRecord ticketDataModel, ScopeContext context)
    {
        context = context.With(_logger);

        var result = await _fileStore.Set(Path, ticketDataModel.ToJson().ToDataETag(), context);
        result.LogStatus(context, "Set ticket data model, path={path}", [Path]);
        if (result.IsError()) return result;

        return result;
    }
}
