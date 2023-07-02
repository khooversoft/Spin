using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Client;
using SpinPortal.Application;
using SpinPortal.Pages.Tenant;
using SpinPortal.Shared;
using System.ComponentModel.DataAnnotations;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinPortal.Pages.User;

public partial class UserEdit
{
    [Inject] public ILogger<TenantEdit> Logger { get; set; } = null!;
    [Inject] NavigationManager NavManager { get; set; } = null!;
    [Inject] public SpinClusterClient Client { get; set; } = null!;

    [Parameter] public string? path { get; set; }

    private ObjectId _objectId { get; set; } = null!;
    private RegisterAccountForm _model = new RegisterAccountForm();
    private const string _notActive = "< not active >";
    private string? _errorMsg { get; set; }
    private string _addOrUpdateButtonText = null!;
    private bool _disableUserId;

    protected override async Task OnParametersSetAsync()
    {
        _objectId = path != null ? new ObjectId("tenant", "$system", path) : null!;
        _addOrUpdateButtonText = _objectId != null ? "Update" : "Add";
        _disableUserId = _objectId != null ? true : false;

        _model = await Read();
    }

    private async void OnValidSubmit(EditContext context)
    {
        await AddOrUpdate();
        StateHasChanged();
    }

    private void Cancel() { NavManager.NavigateTo("/tenant"); }

    private async Task AddOrUpdate()
    {
        var request = new TenantModel
        {
            TenantId = _model.TenantId,
            GlobalPrincipleId = _model.GlobalPrincipleId,
            TenantName = _model.TenantName,
            Contact = _model.Contact,
            Email = _model.Email,
            AccountEnabled = _model.AccountEnabled,
            CreatedDate = _model.CreatedDate,
            ActiveDate = _model.ActiveDate,

            Phone = new UserPhoneModel { Type = "Default", Number = _model.PhoneNumber }.ToEnumerable().ToArray(),

            Addresses = new UserAddressModel
            {
                Type = "Default",
                Address1 = _model.Address1,
                Address2 = _model.Address2,
                City = _model.City,
                State = _model.State,
                ZipCode = _model.ZipCode,
                Country = _model.Country,
            }.ToEnumerable().ToArray(),
        };

        Option<StatusResponse> result = await Client.Tenant.Set(request.TenantId.ToObjectId("tenant", SpinConstants.SystemTenant), request, new ScopeContext(Logger));
        if (result.IsError())
        {
            _errorMsg = $"Failed to write, statusCode={result.StatusCode}, error={result.Error}";
        }

        NavManager.NavigateTo("/tenant");
    }

    private async Task<RegisterAccountForm> Read()
    {
        if (_objectId == null) return new RegisterAccountForm
        {
            TenantId = "Tenant1",
            TenantName = "Company1",
            Contact = "Contact1",
            Email = "contact1@company1.com",
            PhoneNumber = "206-555-1212",
            Address1 = "Address1",
            Address2 = "Address2",
            City = "City1",
            State = "State1",
            ZipCode = "ZipCode1",
            Country = "Country1",
        };

        var result = await Client.Tenant.Get(_objectId, new ScopeContext(Logger));
        if (result.IsError())
        {
            _errorMsg = $"Fail to read TenantId={_objectId}";
            return new RegisterAccountForm();
        }

        return ConvertTo(result.Return());
    }
    private RegisterAccountForm ConvertTo(TenantModel subject) => new RegisterAccountForm
    {
        TenantId = subject.TenantId,
        GlobalPrincipleId = subject.GlobalPrincipleId,
        TenantName = subject.TenantName,
        Contact = subject.Contact,
        Email = subject.Email,
        AccountEnabled = subject.AccountEnabled,
        CreatedDate = subject.CreatedDate.ToUniversalTime(),
        ActiveDate = subject.ActiveDate?.ToUniversalTime(),

        PhoneNumber = subject.Phone.FirstOrDefault()?.Number!,
        Address1 = subject.Addresses.FirstOrDefault()?.Address1!,
        Address2 = subject.Addresses.FirstOrDefault()?.Address2!,
        City = subject.Addresses.FirstOrDefault()?.City!,
        State = subject.Addresses.FirstOrDefault()?.State!,
        ZipCode = subject.Addresses.FirstOrDefault()?.ZipCode!,
        Country = subject.Addresses.FirstOrDefault()?.Country!,
    };

    private class RegisterAccountForm : IValidatableObject
    {
        public string GlobalPrincipleId { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ActiveDate { get; set; }
        public string ActiveDateText => ActiveDate?.ToString() ?? _notActive;

        [Required, StringLength(50)] public string TenantId { get; set; } = null!;
        [Required, StringLength(100)] public string TenantName { get; set; } = null!;
        [Required, StringLength(100)] public string Contact { get; set; } = null!;
        [Required, EmailAddress] public string Email { get; set; } = null!;
        public bool AccountEnabled { get; set; }
        [Required] public string PhoneNumber { get; set; } = null!;
        [Required] public string Address1 { get; set; } = null!;
        public string? Address2 { get; set; }
        [Required] public string City { get; set; } = null!;
        [Required] public string State { get; set; } = null!;
        [Required] public string ZipCode { get; set; } = null!;
        [Required] public string Country { get; set; } = null!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!ObjectId.IsPathValid(TenantId))
            {
                yield return new ValidationResult("Tenant Id is not valid, only alpha numeric, [-._]", new[] { nameof(TenantId) });
            }
        }
    }
}
