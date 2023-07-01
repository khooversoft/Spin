using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SpinPortal.Application;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinPortal.Shared;

public partial class PathText
{
    [Inject] public NavigationManager NavManager { get; set; } = null!;

    [Parameter] public string Path { get; set; } = null!;
    [Parameter] public string BaseAddress { get; set; } = null!;

    private string _editAddress = null!;
    private bool _showEditor { get; set; }
    private string _buttonStyle = PortalConstants.NormalText + ";border-width:0";

    protected override void OnParametersSet()
    {
        Path.NotEmpty();
        BaseAddress.NotEmpty();

        _editAddress = Path + (Path.EndsWith('/') ? string.Empty : "/");
    }

    private IReadOnlyList<PathElement> _items
    {
        get
        {
            string[] parts = Path.NotEmpty().Split('/', StringSplitOptions.RemoveEmptyEntries);

            return parts switch
            {
                string[] v when v.Length <= 2 => Array.Empty<PathElement>(),
                string[] v => Enumerable.Range(2, v.Length).Select(x => createPathElement(parts, x)).ToArray()
            };
        }
    }

    private void HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            GoTo();
        }
    }

    private void GoTo() => NavManager.NavigateTo($"{BaseAddress}/{_editAddress}", true);
    private void OnPathElementClick(int index) => NavManager.NavigateTo(NavTools.ToObjectStorePath(_items[index].HRef), true);
    private void OnPanelClick() => _showEditor = true;
    private void OnLeaveClick() => _showEditor = false;

    private PathElement createPathElement(string[] parts, int index)
    {
        string prefix = BaseAddress.ToEnumerable().Concat(parts.Take(2)).Join('/');

        string href = parts switch
        {
            var v when v.Length == 1 => string.Empty,
            _ => prefix.ToEnumerable().Concat(parts[1..(index + 1)]).Join('/'),
        };

        return new PathElement { Title = parts[index], HRef = href };
    }

    private record PathElement
    {
        public required string Title { get; init; } = null!;
        public required string HRef { get; init; } = null!;
    }
}