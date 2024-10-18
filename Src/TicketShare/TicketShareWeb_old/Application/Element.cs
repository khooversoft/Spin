using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace TicketShareWeb.Application;

public interface IElement { }

public record ButtonElement : IElement
{
    public string Text { get; init; } = null!;
    public Icon? StartEnd { get; init; }
    public Icon? IconEnd { get; init; }
    public Appearance Appearance { get; set; } = Appearance.Stealth;
    public EventCallback Callback { get; init; }
    public bool Disabled { get; set; }
}
