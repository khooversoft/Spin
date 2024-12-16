using Toolbox.Tools;

namespace TicketShareWeb.Application;

public record PanelParameters<T>
{
    private PanelParameters(T value, bool isEdit) => (Value, IsEdit) = (value.NotNull(), isEdit);

    public T Value { get; }
    public bool IsEdit { get; }

    public static PanelParameters<T> Add(T value) => new PanelParameters<T>(value, false);
    public static PanelParameters<T> Edit(T value) => new PanelParameters<T>(value, true);
}
