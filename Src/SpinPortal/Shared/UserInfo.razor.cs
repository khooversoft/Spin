using Microsoft.AspNetCore.Components;
using Microsoft.Graph;
using Microsoft.Identity.Web;

namespace SpinPortal.Shared;

public partial class UserInfo
{
    [Inject]
    public GraphServiceClient GraphServiceClient { get; set; } = null!;

    [Inject]
    public MicrosoftIdentityConsentAndConditionalAccessHandler ConsentHandler { get; set; } = null!;

    [Parameter]
    public bool Show { get; set; }

    private User _user = null!;

    private string UserName { get; set; } = "Not logged on";

    protected override async Task OnParametersSetAsync()
    {
        try
        {
            _user = await GraphServiceClient.Me.Request().GetAsync();
            UserName = _user.DisplayName;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ConsentHandler.HandleException(ex);
        }
    }
}