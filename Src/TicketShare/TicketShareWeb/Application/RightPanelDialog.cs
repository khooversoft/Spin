using Microsoft.FluentUI.AspNetCore.Components;
using Toolbox.Tools;

namespace TicketShareWeb.Application;

public class RightPanelDialog
{
    public const int Canceled = 0;
    public const int Set = 1;
    public const int Delete = 2;
}

public class RightPanelDialog<TModel, TDialog> where TDialog : IDialogContentComponent
{
    private readonly IDialogService _dialogService;
    private readonly Func<TModel, Task> _set;
    private readonly Func<TModel, Task>? _delete;

    public RightPanelDialog(IDialogService dialogService, Func<TModel, Task> set, Func<TModel, Task>? delete)
    {
        _dialogService = dialogService.NotNull();
        _set = set.NotNull();
        _delete = delete;
    }

    public async Task<int> Edit(TModel model, string title)
    {
        var parameters = new DialogParameters()
        {
            Alignment = HorizontalAlignment.Right,
            ShowDismiss = true,
            Title = title,
            PrimaryAction = null,
            SecondaryAction = null,
        };

        var panelParameters = new PanelParameters<TModel>(model, true);
        var dialog = await _dialogService.ShowPanelAsync<TDialog>(panelParameters, parameters);

        var result = await dialog.Result;
        if (result.Cancelled || result.Data is null || result.Data is not PanelResult<TModel> data) return RightPanelDialog.Canceled;

        if (data.DoDelete && _delete != null)
        {
            await _delete(model);
            return RightPanelDialog.Delete;
        }
        else
        {
            await _set(model);
            return RightPanelDialog.Set;
        }
    }

    public async Task<int> Add(TModel model, string title)
    {
        model.NotNull();
        title.NotEmpty();

        var parameters = new DialogParameters()
        {
            Alignment = HorizontalAlignment.Right,
            ShowDismiss = true,
            Title = title,
            PrimaryAction = null,
            SecondaryAction = null,
        };

        var panelParameters = new PanelParameters<TModel>(model);
        var dialog = await _dialogService.ShowPanelAsync<TDialog>(panelParameters, parameters);
        var result = await dialog.Result;
        if (result.Cancelled || result.Data is null || result.Data is not PanelResult<TModel> data) return RightPanelDialog.Canceled;

        await _set(model);
        return RightPanelDialog.Set;
    }
}
