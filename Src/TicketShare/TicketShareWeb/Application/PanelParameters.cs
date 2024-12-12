using Toolbox.Tools;

namespace TicketShareWeb.Application;

public record PanelParameters<T>
{
    public PanelParameters(T value) => Value = value.NotNull();
    public PanelParameters(T value, bool showDelete) => (Value, ShowDelete) = (value.NotNull(), showDelete);

    public T Value { get; }
    public bool ShowDelete { get; }
}
