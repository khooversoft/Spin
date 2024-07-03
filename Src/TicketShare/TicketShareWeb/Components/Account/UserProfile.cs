//using System.Collections.Frozen;
//using System.ComponentModel.DataAnnotations;
//using Microsoft.AspNetCore.Components;
//using Microsoft.FluentUI.AspNetCore.Components;
//using TicketShare.sdk;
//using Toolbox.Extensions;
//using Toolbox.Types;

//namespace TicketShareWeb.Components.Account;

//public partial class UserProfile
//{
//    [Inject] AccountConnector _accountConnector { get; set; } = default!;
//    [Inject] NavigationManager _navigationManager { get; set; } = default!;
//    [Inject] ILogger<UserProfile> _logger { get; set; } = default!;
//    [Inject] IDialogService _dialogService { get; set; } = default!;
//    [Inject] IToastService _toastService { get; set; } = default!;


//    //[SupplyParameterFromForm] private UserProfileModel Input { get; set; } = new();
//    //[SupplyParameterFromQuery] private string? PrincipalId { get; set; }

//    private string? errorMessage;
//    private IDialogReference? _dialog;

//    protected override async Task OnInitializedAsync()
//    {
//        var context = new ScopeContext(_logger);

//        var inputOption = await ReadProfile(context);
//        if (inputOption.IsError()) return;

//        UserProfileModel input = inputOption.Return();

//        var dialogParameters = new DialogParameters<UserProfileModel>
//        {
//            Content = input,
//            Alignment = HorizontalAlignment.Right,
//            Title = "Edit User Profile",
//            PrimaryAction = "Yes",
//            SecondaryAction = "No",
//        };

//        _dialog = await _dialogService.ShowPanelAsync<UserProfilePanel>(input, dialogParameters);
//        DialogResult result = await _dialog.Result;

//        if (!result.Cancelled)
//        {
//            _navigationManager.NavigateTo("/", true);
//            return;
//        }
//    }

//    private async Task<Toolbox.Types.Option<UserProfileModel>> ReadProfile(ScopeContext context)
//    {
//        var accountOption = await _accountConnector.Get(context);
//        if (accountOption.IsError() && !accountOption.IsNotFound())
//        {
//            _toastService.ShowError($"Error: {accountOption.Error} [{typeof(UserProfile).Name}]");
//            return StatusCode.BadRequest;
//        }

//        AccountRecord accountRecord = accountOption.Return();
//        AddressRecord? addressRecord = accountRecord.Address.FirstOrDefault();

//        UserProfileModel Input = new UserProfileModel
//        {
//            Name = accountRecord.Name,
//            Email = accountRecord.ContactItems.Where(x => x.Type == ContactType.Email).FirstOrDefault()?.Value,
//            PhoneNumber = accountRecord.ContactItems.Where(x => x.Type == ContactType.Cell).FirstOrDefault()?.Value,
//            Address1 = addressRecord?.Address1,
//            Address2 = addressRecord?.Address2,
//            City = addressRecord?.City,
//            State = addressRecord?.State,
//            ZipCode = addressRecord?.ZipCode,
//        };

//        return Input;
//    }

//    private async Task WriteProfile(UserProfileModel input, ScopeContext context)
//    {
//        var accountOption = await _accountConnector.Get(context);
//        if (accountOption.IsError() && !accountOption.IsNotFound())
//        {
//            _toastService.ShowError($"Error: {accountOption.Error} [{typeof(UserProfile).Name}]");
//            return;
//        }

//        AccountRecord accountRecord = accountOption.Return();

//        AccountRecord account = accountRecord
//            .Merge(createContacts(input))
//            .Merge(createAddress(input));

//        var result = await _accountConnector.Set(account, context);
//        if (result.IsError())
//        {
//            _toastService.ShowError($"User profile failed to updated - Error:{result.Error}");
//            return;
//        }

//        _toastService.ShowSuccess("User profile updated");
//        return;

//        static IEnumerable<ContactRecord> createContacts(UserProfileModel inputModel) => new[]
//        {
//                inputModel.Email.IsNotEmpty() ? new ContactRecord { Type = ContactType.Email, Value = inputModel.Email } : null,
//                inputModel.PhoneNumber.IsNotEmpty() ? new ContactRecord { Type = ContactType.Cell, Value = inputModel.PhoneNumber } : null,
//            }.OfType<ContactRecord>();

//        static IEnumerable<AddressRecord> createAddress(UserProfileModel inputModel) => [
//            new AddressRecord
//                {
//                    Address1 = inputModel.Address1,
//                    Address2 = inputModel.Address2,
//                    City = inputModel.City,
//                    State = inputModel.State,
//                    ZipCode = inputModel.ZipCode,
//                }
//        ];
//    }

//    //public async Task SetUser()
//    //{
//    //    var context = new ScopeContext(_logger);

//    //    var accountOption = await _accountConnector.Get(context);
//    //    if (accountOption.IsError() && !accountOption.IsNotFound())
//    //    {
//    //        errorMessage = accountOption.Error;
//    //        return;
//    //    }

//    //    AccountRecord account = accountOption.Return()
//    //        .Merge(createContacts(Input))
//    //        .Merge(createAddress(Input));

//    //    var result = await _accountConnector.Set(account, context);
//    //    return;

//    //    static IEnumerable<ContactRecord> createContacts(UserProfileModel inputModel) => new[]
//    //    {
//    //        inputModel.Email.IsNotEmpty() ? new ContactRecord { Type = ContactType.Email, Value = inputModel.Email } : null,
//    //        inputModel.PhoneNumber.IsNotEmpty() ? new ContactRecord { Type = ContactType.Cell, Value = inputModel.PhoneNumber } : null,
//    //    }.OfType<ContactRecord>();

//    //    static IEnumerable<AddressRecord> createAddress(UserProfileModel inputModel) => [
//    //        new AddressRecord
//    //        {
//    //            Address1 = inputModel.Address1,
//    //            Address2 = inputModel.Address2,
//    //            City = inputModel.City,
//    //            State = inputModel.State,
//    //            ZipCode = inputModel.ZipCode,
//    //        }
//    //    ];
//    //}
//}
