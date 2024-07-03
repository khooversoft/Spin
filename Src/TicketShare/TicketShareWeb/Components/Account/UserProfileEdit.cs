using Microsoft.FluentUI.AspNetCore.Components;
using TicketShare.sdk;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;

namespace TicketShareWeb.Components.Account;

public class UserProfileEdit
{
    private readonly AccountConnector _accountConnector;
    private readonly IDialogService _dialogService;
    private readonly IToastService _toastService;
    private readonly ILogger<UserProfileEdit> _logger;

    public UserProfileEdit(AccountConnector accountConnector, IDialogService dialogService, IToastService toastService, ILogger<UserProfileEdit> logger)
    {
        _accountConnector = accountConnector.NotNull();
        _dialogService = dialogService.NotNull();
        _toastService = toastService.NotNull();
        _logger = logger.NotNull();
    }

    public async Task EditCurrentUserProfile()
    {
        var context = new ScopeContext(_logger);

        var inputOption = await ReadProfile(context);
        if (inputOption.IsError()) return;

        UserProfileModel input = inputOption.Return();

        var dialogParameters = new DialogParameters<UserProfileModel>
        {
            Content = input,
            Alignment = HorizontalAlignment.Right,
            Title = "Edit User Profile",
            PrimaryAction = "Yes",
            SecondaryAction = "No",
            Width = "300px",
        };

        IDialogReference? dialog = await _dialogService.ShowPanelAsync<UserProfilePanel>(input, dialogParameters);
        DialogResult result = await dialog.Result;
        if (result.Cancelled) return;

        await WriteProfile(input, context);
    }

    private async Task<Toolbox.Types.Option<UserProfileModel>> ReadProfile(ScopeContext context)
    {
        var accountOption = await ReadAccountRecord(context);
        if (accountOption.IsError()) return accountOption.ToOptionStatus<UserProfileModel>();

        AccountRecord accountRecord = accountOption.Return();
        AddressRecord? addressRecord = accountRecord.Address.FirstOrDefault();

        UserProfileModel Input = new UserProfileModel
        {
            Name = accountRecord.Name,
            Email = accountRecord.ContactItems.Where(x => x.Type == ContactType.Email).FirstOrDefault()?.Value,
            PhoneNumber = accountRecord.ContactItems.Where(x => x.Type == ContactType.Cell).FirstOrDefault()?.Value,
            Address1 = addressRecord?.Address1,
            Address2 = addressRecord?.Address2,
            City = addressRecord?.City,
            State = addressRecord?.State,
            ZipCode = addressRecord?.ZipCode,
        };

        return Input;
    }


    private async Task WriteProfile(UserProfileModel input, ScopeContext context)
    {
        var accountOption = await ReadAccountRecord(context);
        if (accountOption.IsError()) return;

        AccountRecord accountRecord = accountOption.Return();
        AccountRecord account = accountRecord
            .Merge(createContacts(input))
            .Merge(createAddress(input).Where(x => x.HasData()));

        var result = await _accountConnector.Set(account, context);
        if (result.IsError())
        {
            _toastService.ShowError($"User profile failed to updated - Error:{result.Error}");
            return;
        }

        _toastService.ShowSuccess("User profile updated");
        return;

        static IEnumerable<ContactRecord> createContacts(UserProfileModel inputModel) => new[]
        {
                inputModel.Email.IsNotEmpty() ? new ContactRecord { Type = ContactType.Email, Value = inputModel.Email } : null,
                inputModel.PhoneNumber.IsNotEmpty() ? new ContactRecord { Type = ContactType.Cell, Value = inputModel.PhoneNumber } : null,
            }.OfType<ContactRecord>();

        static IEnumerable<AddressRecord> createAddress(UserProfileModel inputModel) => new AddressRecord
        {
            Label = "default",            
            Address1 = inputModel.Address1,
            Address2 = inputModel.Address2,
            City = inputModel.City,
            State = inputModel.State,
            ZipCode = inputModel.ZipCode,
        }.ToEnumerable();
    }

    private async Task<Toolbox.Types.Option<AccountRecord>> ReadAccountRecord(ScopeContext context)
    {
        var accountOption = await _accountConnector.Get(context);
        if (accountOption.IsError() && !accountOption.IsNotFound())
        {
            _toastService.ShowError($"Error: {accountOption.Error} [{typeof(UserProfile).Name}]");
            return StatusCode.BadRequest;
        }

        return accountOption;
    }
}
