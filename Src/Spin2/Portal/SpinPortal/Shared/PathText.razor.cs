using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SpinPortal.Application;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinPortal.Shared;

public partial class PathText
{
    [Inject]
    public NavigationManager NavManager { get; set; } = null!;

    [Parameter]
    public string Path { get; set; } = null!;

    private bool _showEditor { get; set; }

    private string _buttonStyle = PortalConstants.NormalText + ";border-width:0";

    protected override void OnParametersSet()
    {
        Path.NotEmpty();
        base.OnParametersSet();  //new List<BreadcrumbItem>
    }

    private List<PathElement> _items
    {
        get
        {
            string[] parts = Path.NotEmpty().Split('/', StringSplitOptions.RemoveEmptyEntries);

            return Enumerable.Range(0, parts.Length)
                .Select(x => createPathElement(parts, x))
                .ToList();
        }
    }

    private void HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            GoTo();
        }
    }

    private void GoTo()
    {
        NavManager.NavigateTo($"objectStore/{Path}", true);
    }

    private void OnPathElementClick(int index)
    {
        NavManager.NavigateTo(NavTools.ToObjectStorePath(_items[index].HRef), true);
    }

    private void OnPanelClick()
    {
        _showEditor = true;
    }

    private void OnLeaveClick()
    {
        _showEditor = false;
    }

    private PathElement createPathElement(string[] parts, int index)
    {
        string domain = parts[0];

        string href = parts switch
        {
            var v when v.Length == 1 => string.Empty,
            _ => domain.ToEnumerable().Concat(parts[1..(index+1)]).Join("/"),
        };        

        return new PathElement { Title = parts[index], HRef = href };
    }

    private record PathElement
    {
        public required string Title { get; init; } = null!;
        public required string HRef { get; init; } = null!;
    }
}