namespace Toolbox.CommandRouter;

public interface ISymbolDescriptor<T>
{
    TO GetValueDescriptor<TO>();
}

