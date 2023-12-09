namespace Toolbox.CommandRouter;

public interface ICommandRouterHost
{
    void Enqueue(params string[] args);
    Task<int> Run();
    Task<int> Run(params string[] args);
    IServiceProvider Service { get; }
}
