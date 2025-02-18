using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class UserAccountEditContext
{
    private readonly UserAccountContext _userAccountContext;
    private readonly ILogger _logger;
    private AccountModel? _currentStored;

    public UserAccountEditContext(UserAccountContext userAccountContext, ILogger logger)
    {
        _userAccountContext = userAccountContext.NotNull();
        _logger = logger.NotNull();

        Contact = new CollectionAccessActor<ContactModel>(this, x => Input.ContactItems[x.Id] = x, x => Input.ContactItems.TryRemove(x.Id, out var _));
        Address = new CollectionAccessActor<AddressModel>(this, x => Input.AddressItems[x.Id] = x, x => Input.AddressItems.TryRemove(x.Id, out var _));
        Calendar = new CollectionAccessActor<CalendarModel>(this, x => Input.CalendarItems[x.Id] = x, x => Input.CalendarItems.TryRemove(x.Id, out var _));
    }

    public AccountModel Input { get; private set; } = null!;
    public UserProfileEditModel UserProfile { get; private set; } = null!;

    public CollectionAccessActor<ContactModel> Contact { get; }
    public CollectionAccessActor<AddressModel> Address { get; }
    public CollectionAccessActor<CalendarModel> Calendar { get; }

    public bool IsChanged() => Input != null && _currentStored != null && (Input != _currentStored);
    public bool IsLoaded() => Input != null;

    public async Task Get(ScopeContext context)
    {
        if (Input != null) return;
        context = context.With(_logger);

        var accountOption = await _userAccountContext.GetAccount(context).ConfigureAwait(false);
        accountOption.ThrowOnError(nameof(UserAccountEditContext) + ":" + nameof(Get));

        Input = accountOption.Return().ConvertTo();
        _currentStored = Input.Clone();
        UserProfile = new UserProfileEditModel { Name = Input.Name };

        string json = Input.ToJson();
        _logger.LogInformation("Account read, Input={input}", json);
    }

    public async Task Set(ScopeContext context)
    {
        Input.NotNull("Input is not set");
        if (!IsChanged()) return;

        var identityOption = await _userAccountContext.GetPrincipalIdentity(context).ConfigureAwait(false);
        if (identityOption.IsError())
        {
            context.LogError("Cannot get principalId");
            return;
        }

        string principalId = identityOption.Return().PrincipalId;

        _logger.LogInformation("Updating account, Input={input}", Input.ToJson());
        var account = Input.NotNull().ConvertTo(principalId);

        var result = await _userAccountContext.SetAccount(account, context).ConfigureAwait(false);
        if (result.IsError()) result.ThrowOnError("Cannot update");
    }

    public async Task SetName(string name, ScopeContext context)
    {
        Input.NotNull("Input is not set");
        Input = Input with { Name = name };
        UserProfile = UserProfile with { Name = name };
        await Set(context);
    }

    public class CollectionAccessActor<T>
    {
        private readonly UserAccountEditContext _accountContext;
        private readonly Action<T> _set;
        private readonly Func<T, bool> _remove;

        internal CollectionAccessActor(UserAccountEditContext accountContext, Action<T> set, Func<T, bool> remove)
        {
            _accountContext = accountContext.NotNull();
            _set = set;
            _remove = remove;
        }

        public async Task Set(T model, ScopeContext context)
        {
            model.NotNull();
            _set(model);
            await _accountContext.Set(context);
        }
        public async Task Delete(T model, ScopeContext context)
        {
            bool removed = _remove(model);
            if (removed) await _accountContext.Set(context);
        }
    }
}
