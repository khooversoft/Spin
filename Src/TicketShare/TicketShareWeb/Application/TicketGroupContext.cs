using TicketShare.sdk;
using TicketShareWeb.Components.Pages.Ticket.Model;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShareWeb.Application;

public class TicketGroupContext
{
    private readonly TicketGroupClient _ticketGroupClient;
    private readonly ILogger _logger;
    private readonly string _ticketGroupId;
    private TicketGroupModel _current = null!;

    public TicketGroupContext(string ticketGroupId, TicketGroupClient ticketGroupClient, ILogger logger)
    {
        _ticketGroupId = ticketGroupId.NotEmpty();
        _ticketGroupClient = ticketGroupClient.NotNull();
        _logger = logger.NotNull();
    }

    public TicketGroupModel Input { get; private set; } = null!;

    public bool IsChanged() => Input != null && _current != null && (Input != _current);
    public bool IsLoaded() => Input != null;

    public async Task<Option> SetDescription(string? description)
    {
        if (!IsLoaded()) await Get().ConfigureAwait(false);

        Input = Input with { Description = description };
        return await Set().ConfigureAwait(false);
    }

    public async Task<Option> Get()
    {
        ScopeContext context = new ScopeContext(_logger);

        var readOption = await _ticketGroupClient.Get(_ticketGroupId, context).ConfigureAwait(false);
        if (readOption.IsError()) return readOption.ToOptionStatus();

        Input = readOption.Return().ConvertTo();
        _current = Input;
        return StatusCode.OK;
    }

    public TicketGroupHeaderModel GetHeader() => Input.NotNull().ConvertToModel();

    private async Task<Option> Set()
    {
        Input.NotNull("Input is not set");

        ScopeContext context = new ScopeContext(_logger);
        TicketGroupRecord ticketGroupRecord = Input.ConvertTo();

        var result = await _ticketGroupClient.Set(ticketGroupRecord, context).ConfigureAwait(false);
        if (result.IsError()) return result;

        _current = Input;
        return result;
    }

    private async Task<Option> Delete(ScopeContext context)
    {
        Input = null!;
        _current = null!;

        var result = await _ticketGroupClient.Delete(_ticketGroupId, context).ConfigureAwait(false);
        if (result.IsError()) return result;

        return result;
    }
}
