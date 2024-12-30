using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

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

        Roles = new CollectionAccessActor<RoleModel>(this, x => Input.Roles[x.Id] = x, x => Input.Roles.TryRemove(x.Id, out var _));
        Seats = new CollectionAccessActor<SeatModel>(this, x => Input.Seats[x.Id] = x, x => Input.Seats.TryRemove(x.Id, out var _));
    }

    public TicketGroupModel Input { get; private set; } = null!;
    public TicketGroupHeaderModel Header { get; private set; } = null!;

    public CollectionAccessActor<RoleModel> Roles { get; }
    public CollectionAccessActor<SeatModel> Seats { get; }

    public bool IsChanged() => Input != null && _current != null && (Input != _current);
    public bool IsLoaded() => Input != null;

    public async Task<Option> SetHeader(TicketGroupHeaderModel ticketGroupHeaderModel)
    {
        if (!IsLoaded()) await Get().ConfigureAwait(false);

        Input = Input with
        {
            Name = ticketGroupHeaderModel.Name,
            Description = ticketGroupHeaderModel.Description
        };

        return await Set().ConfigureAwait(false);
    }

    public async Task<Option> Get()
    {
        var context = new ScopeContext(_logger);

        var readOption = await _ticketGroupClient.Get(_ticketGroupId, context).ConfigureAwait(false);
        if (readOption.IsError()) return readOption.ToOptionStatus();

        Input = readOption.Return().ConvertTo();
        _current = Input;
        Header = Input.ConvertToModel();

        return StatusCode.OK;
    }

    public TicketGroupHeaderModel GetHeader() => Input.NotNull().ConvertToModel();

    private async Task<Option> Set()
    {
        Input.NotNull("Input is not set");

        var context = new ScopeContext(_logger);
        TicketGroupRecord ticketGroupRecord = Input.ConvertTo();

        var result = await _ticketGroupClient.Set(ticketGroupRecord, context).ConfigureAwait(false);
        if (result.IsError()) return result;

        _current = Input;
        Header = Input.ConvertToModel();

        return result;
    }

    public async Task<Option> Delete()
    {
        Input = null!;
        _current = null!;
        var context = new ScopeContext(_logger);

        var result = await _ticketGroupClient.Delete(_ticketGroupId, context).ConfigureAwait(false);
        if (result.IsError()) return result;

        return result;
    }

    public class CollectionAccessActor<T>
    {
        private readonly TicketGroupContext _context;
        private readonly Action<T> _set;
        private readonly Func<T, bool> _remove;

        internal CollectionAccessActor(TicketGroupContext context, Action<T> set, Func<T, bool> remove)
        {
            _context = context.NotNull();
            _set = set;
            _remove = remove;
        }

        public async Task Set(T model)
        {
            model.NotNull();
            _set(model);
            await _context.Set();
        }
        public async Task Delete(T model)
        {
            bool removed = _remove(model);
            if (removed) await _context.Set();
        }
    }
}
