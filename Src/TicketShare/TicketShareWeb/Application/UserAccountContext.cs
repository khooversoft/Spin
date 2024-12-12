using TicketShare.sdk;
using TicketShareWeb.Components.Pages.Profile.Models;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShareWeb.Application;

public class UserAccountContext
{
    private readonly UserAccountManager _userAccountManager;
    private readonly ILogger _logger;
    private InputModel? _currentStored;

    public UserAccountContext(UserAccountManager userAccountManager, ILogger logger)
    {
        _userAccountManager = userAccountManager.NotNull();
        _logger = logger.NotNull();

        Contact = new CollectionAccessActor<ContactModel>(this, x => Input.ContactItems[x.Id] = x, x => Input.ContactItems.TryRemove(x.Id, out var _));
        Address = new CollectionAccessActor<AddressModel>(this, x => Input.AddressItems[x.Id] = x, x => Input.AddressItems.TryRemove(x.Id, out var _));
        Calendar = new CollectionAccessActor<CalendarModel>(this, x => Input.CalendarItems[x.Id] = x, x => Input.CalendarItems.TryRemove(x.Id, out var _));
    }

    public InputModel Input { get; private set; } = null!;
    public UserProfileEditModel UserProfile { get; private set; } = null!;

    public CollectionAccessActor<ContactModel> Contact { get; }
    public CollectionAccessActor<AddressModel> Address { get; }
    public CollectionAccessActor<CalendarModel> Calendar { get; }

    public bool IsChanged() => Input != null && _currentStored != null && (Input != _currentStored);
    public bool IsLoaded() => Input != null;

    public async Task Get()
    {
        if (Input != null) return;

        var accountOption = await _userAccountManager.GetAccount().ConfigureAwait(false);
        accountOption.ThrowOnError(nameof(UserAccountContext) + ":" + nameof(Get));

        Input = accountOption.Return().ConvertTo();
        _currentStored = Input.Clone();
        UserProfile = new UserProfileEditModel { Name = Input.Name };

        string json = Input.ToJson();
        _logger.LogInformation("Account read, Input={input}", json);
    }

    public async Task Set()
    {
        Input.NotNull("Input is not set");
        if (!IsChanged()) return;

        string principalId = await _userAccountManager.GetPrincipalId().ConfigureAwait(false);

        _logger.LogInformation("Updating account, Input={input}", Input.ToJson());
        var account = Input.NotNull().ConvertTo(principalId);

        var result = await _userAccountManager.SetAccount(account).ConfigureAwait(false);
        if( result.IsError()) result.ThrowOnError("Cannot update");
    }

    public async Task SetName(string name)
    {
        Input.NotNull("Input is not set");
        Input = Input with { Name = name };
        UserProfile = UserProfile with { Name = name };
        await Set();
    }

    public class CollectionAccessActor<T>
    {
        private readonly UserAccountContext _context;
        private readonly Action<T> _set;
        private readonly Func<T, bool> _remove;

        public CollectionAccessActor(UserAccountContext context, Action<T> set, Func<T, bool> remove)
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
            if( removed) await _context.Set();
        }
    }
}
