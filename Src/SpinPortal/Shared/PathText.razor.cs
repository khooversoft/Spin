using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SpinPortal.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinPortal.Shared;

public partial class PathText
{
    [Inject] public NavigationManager NavManager { get; set; } = null!;

    [Parameter] public ObjectId DataObjectId { get; set; } = null!;

    private string _editAddress = null!;
    private bool _showEditor { get; set; }
    private string _buttonStyle = PortalConstants.NormalText + ";border-width:0";
    private IReadOnlyList<PathElement> _items = Array.Empty<PathElement>();

    protected override void OnParametersSet()
    {
        DataObjectId.NotNull();

        _items = DataObjectId.Paths
            .Select((x, i) => createPathElement(DataObjectId.Paths, i))
            .ToArray();
    }

    private void HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            GoTo();
        }
    }

    private void GoTo() => NavManager.NavigateTo($"{_editAddress}", true);
    private void OnPathElementClick(int index) => NavManager.NavigateTo(_items[index].HRef, true);
    private void OnPanelClick() => _showEditor = true;
    private void OnLeaveClick() => _showEditor = false;

    private PathElement createPathElement(IReadOnlyList<string> parts, int index)
    {
        return new PathElement
        {
            Title = parts.Last(),
            HRef = $"data/{DataObjectId.Schema}/{DataObjectId.Tenant}/{DataObjectId.Paths.Take(index)}"
        };
    }

    private record PathElement
    {
        public required string Title { get; init; } = null!;
        public required string HRef { get; init; } = null!;
    }
}