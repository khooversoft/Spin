using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class TicketGroupContext
{
    private readonly TicketGroupClient _ticketGroupClient;
    private readonly string _ticketGroupId;
    private TicketGroupModel _current = null!;

    public TicketGroupContext(string ticketGroupId, TicketGroupClient ticketGroupClient)
    {
        _ticketGroupId = ticketGroupId.NotEmpty();
        _ticketGroupClient = ticketGroupClient.NotNull();

        Roles = new CollectionAccessActor<RoleModel>(this, x => Input.Roles[x.Id] = x, x => Input.Roles.TryRemove(x.Id, out var _));
        Seats = new CollectionAccessActor<SeatModel>(this, x => Input.Seats[x.Id] = x, x => Input.Seats.TryRemove(x.Id, out var _));
    }

    public TicketGroupModel Input { get; private set; } = null!;
    public TicketGroupHeaderModel Header { get; private set; } = null!;

    public CollectionAccessActor<RoleModel> Roles { get; }
    public CollectionAccessActor<SeatModel> Seats { get; }

    public bool IsChanged() => Input != null && _current != null && (Input != _current);
    public bool IsLoaded() => Input != null;

    public async Task<Option> SetHeader(TicketGroupHeaderModel ticketGroupHeaderModel, ScopeContext context)
    {
        if (!IsLoaded()) await Get(context).ConfigureAwait(false);

        Input = Input with
        {
            Name = ticketGroupHeaderModel.Name,
            Description = ticketGroupHeaderModel.Description
        };

        return await Set(context).ConfigureAwait(false);
    }

    public async Task<Option> Get(ScopeContext context)
    {
        var readOption = await _ticketGroupClient.Get(_ticketGroupId, context).ConfigureAwait(false);
        if (readOption.IsError()) return readOption.ToOptionStatus();

        Input = readOption.Return().ConvertTo();
        _current = Input;
        Header = Input.ConvertToModel();

        return StatusCode.OK;
    }

    public TicketGroupHeaderModel GetHeader() => Input.NotNull().ConvertToModel();

    private async Task<Option> Set(ScopeContext context)
    {
        Input.NotNull("Input is not set");
        TicketGroupRecord ticketGroupRecord = Input.ConvertTo();

        var result = await _ticketGroupClient.Set(ticketGroupRecord, context).ConfigureAwait(false);
        if (result.IsError()) return result;

        _current = Input;
        Header = Input.ConvertToModel();

        return result;
    }

    public async Task<Option> Delete(ScopeContext context)
    {
        Input = null!;
        _current = null!;

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

        public async Task<Option> Set(T model, ScopeContext context)
        {
            model.NotNull();
            _set(model);
            return await _context.Set(context).ConfigureAwait(false);
        }

        public async Task<Option> Delete(T model, ScopeContext context)
        {
            bool removed = _remove(model);
            if (removed) return await _context.Set(context).ConfigureAwait(false);
            return StatusCode.OK;
        }
    }
}
