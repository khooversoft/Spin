using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Client;
using SpinPortal.Application;
using SpinPortal.Shared;
using System.ComponentModel.DataAnnotations;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinPortal.Pages;

public partial class TenantEdit
{
    [Inject] public ILogger<TenantEdit> Logger { get; set; } = null!;
    [Inject] NavigationManager NavManager { get; set; } = null!;
    [Inject] public SpinClusterClient Client { get; set; } = null!;

    [Parameter] public ObjectId? TenantId { get; set; }

    private RegisterAccountForm _model = new RegisterAccountForm();
    private const string _notActive = "< not active >";
    private string? _errorMsg { get; set; }
    private string _addOrUpdateButtonText = null!;
    private bool _disableUserId;

    protected override async Task OnParametersSetAsync()
    {
        _addOrUpdateButtonText = TenantId != null ? "Update" : "Add";
        _disableUserId = TenantId != null ? true : false;

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
        var request = new TenantRegister
        {
            TenantId = _model.TenantId,
            GlobalPrincipleId = _model.GlobalPrincipleId,
            TenantName = _model.TenantName,
            Contact = _model.Contact,
            Email = _model.Email,
            AccountEnabled = _model.AccountEnabled,
            CreatedDate = _model.CreatedDate,
            ActiveDate = _model.ActiveDate,
        };

        StatusCode result = await Client.Data.Set(request.TenantId.ToObjectId("tenant", SpinConstants.SystemTenant), request, new ScopeContext(Logger));
        if (result.IsError())
        {
            _errorMsg = $"Failed to write, statusCode={result}";
        }
    }

    private async Task<RegisterAccountForm> Read()
    {
        if (TenantId == null) return new RegisterAccountForm();

        var result = await Client.Data.Get<TenantRegister>(TenantId, new ScopeContext(Logger));
        if (result.IsError())
        {
            _errorMsg = $"Fail to read TenantId={TenantId}";
            return new RegisterAccountForm();
        }

        return ConvertTo(result.Return());
    }
    private RegisterAccountForm ConvertTo(TenantRegister subject) => new RegisterAccountForm
    {
        TenantId = subject.TenantId,
        GlobalPrincipleId = subject.GlobalPrincipleId,
        TenantName = subject.TenantName,
        Contact = subject.Contact,
        Email = subject.Email,
        AccountEnabled = subject.AccountEnabled,
        CreatedDate = subject.CreatedDate.ToUniversalTime(),
        ActiveDate = subject.ActiveDate?.ToUniversalTime(),
    };

    private class RegisterAccountForm : IValidatableObject
    {
        public string GlobalPrincipleId { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ActiveDate { get; set; }
        public string ActiveDateText => ActiveDate?.ToString() ?? _notActive;

        [Required, StringLength(50)]
        public string TenantId { get; set; } = null!;

        [Required, StringLength(100)]
        public string TenantName { get; set; } = null!;

        [Required, StringLength(100)]
        public string Contact { get; set; } = null!;


        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        public bool AccountEnabled { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!ObjectId.IsPathValid(TenantId))
            {
                yield return new ValidationResult("Tenant Id is not valid, only alpha numeric, [-._]", new[] { nameof(TenantId) });
            }
        }
    }
}
