using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using TicketShare.sdk;
using Toolbox.Extensions;
using Toolbox.Types;

namespace TicketShareWeb.Components.Account;

public partial class UserProfile
{
    [Inject] AccountConnector _accountConnector { get; set; } = default!;
    [Inject] NavigationManager _navigationManager { get; set; } = default!;
    [Inject] ILogger<UserProfile> _logger { get; set; } = default!;

    [SupplyParameterFromForm] private InputModel Input { get; set; } = new();
    [SupplyParameterFromQuery] private string? PrincipalId { get; set; }

    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        var context = new ScopeContext(_logger);

        var accountOption = await _accountConnector.Get(context);
        if (accountOption.IsError() && !accountOption.IsNotFound())
        {
            errorMessage = accountOption.Error;
            return;
        }

        AccountRecord accountRecord = accountOption.Return();
        AddressRecord? addressRecord = accountRecord.Address.FirstOrDefault();

        Input = new InputModel
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
    }

    public async Task SetUser()
    {
        var context = new ScopeContext(_logger);

        var accountOption = await _accountConnector.Get(context);
        if (accountOption.IsError() && !accountOption.IsNotFound())
        {
            errorMessage = accountOption.Error;
            return;
        }

        AccountRecord account = accountOption.Return()
            .Merge(createContacts(Input))
            .Merge(createAddress(Input));

        var result = await _accountConnector.Set(account, context);
        return;

        static IEnumerable<ContactRecord> createContacts(InputModel inputModel) => new[]
        {
            inputModel.Email.IsNotEmpty() ? new ContactRecord { Type = ContactType.Email, Value = inputModel.Email } : null,
            inputModel.PhoneNumber.IsNotEmpty() ? new ContactRecord { Type = ContactType.Cell, Value = inputModel.PhoneNumber } : null,
        }.OfType<ContactRecord>();

        static IEnumerable<AddressRecord> createAddress(InputModel inputModel) => [
            new AddressRecord
            {
                Address1 = inputModel.Address1,
                Address2 = inputModel.Address2,
                City = inputModel.City,
                State = inputModel.State,
                ZipCode = inputModel.ZipCode,
            }
        ];
    }

    private sealed class InputModel
    {
        [Display(Name = "Name")]
        public string? Name { get; set; } = null!;

        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; } = null!;

        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; } = null!;

        [Display(Name = "Address 1")]
        public string? Address1 { get; set; } = null!;

        [Display(Name = "Address 2")]
        public string? Address2 { get; set; } = null!;

        [Display(Name = "City")]
        public string? City { get; set; } = null!;

        [Display(Name = "State")]
        public string? State { get; set; } = null!;

        [Display(Name = "Zip Code")]
        public string? ZipCode { get; set; } = null!;
    }
}
