using Microsoft.FluentUI.AspNetCore.Components;
using TicketShareWeb.Components.Controls;
using Toolbox.Tools;

namespace TicketShareWeb.Application;

public class AskPanel
{
    private readonly IDialogService _dialogService;

    public AskPanel(IDialogService dialogService) => _dialogService = dialogService.NotNull();

    public async Task<bool> Show(string title, string question)
    {
        {
            var parameters = new DialogParameters()
            {
                Alignment = HorizontalAlignment.Right,
                ShowDismiss = true,
                Title = title,
                PrimaryAction = null,
                SecondaryAction = null,
            };

            var dialog = await _dialogService.ShowPanelAsync<AskDialog>(question, parameters);
            var result = await dialog.Result;

            return result.Cancelled ? false : true;
        }
    }
}
