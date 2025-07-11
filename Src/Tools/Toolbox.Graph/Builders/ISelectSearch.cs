namespace Toolbox.Graph;

public interface ISelectSearch
{
    string Build();
}

public class LeftJoinSearch : ISelectSearch
{
    public string Build() => "->";
}

public class RightJoinSearch : ISelectSearch
{
    public string Build() => "<-";
}

public class FullJoinSearch : ISelectSearch
{
    public string Build() => "<->";
}

