using System.CommandLine;
using System.CommandLine.Binding;

namespace Toolbox.CommandRouter;

public static class CommandSymbolHandlers
{
    public static void SetHandler(this CommandSymbol command, Func<Task> handle) => command.Command.SetHandler(handle);

    public static void SetHandler<T>(
        this CommandSymbol command,
        Func<T, Task> handle,
        ISymbolDescriptor<T> symbol
        ) => command.Command.SetHandler(
            handle,
            symbol.GetValueDescriptor<IValueDescriptor<T>>()
            );

    public static void SetHandler<T1, T2>(
        this CommandSymbol command,
        Func<T1, T2, Task> handle,
        ISymbolDescriptor<T1> symbol1,
        ISymbolDescriptor<T2> symbol2
    ) => command.Command.SetHandler(
        handle,
        symbol1.GetValueDescriptor<IValueDescriptor<T1>>(),
        symbol2.GetValueDescriptor<IValueDescriptor<T2>>()
        );

    public static void SetHandler<T1, T2, T3>(
        this CommandSymbol command,
        Func<T1, T2, T3, Task> handle,
        ISymbolDescriptor<T1> symbol1,
        ISymbolDescriptor<T2> symbol2,
        ISymbolDescriptor<T3> symbol3
        ) => command.Command.SetHandler(
            handle,
            symbol1.GetValueDescriptor<IValueDescriptor<T1>>(),
            symbol2.GetValueDescriptor<IValueDescriptor<T2>>(),
            symbol3.GetValueDescriptor<IValueDescriptor<T3>>()
            );

    public static void SetHandler<T1, T2, T3, T4>(
        this CommandSymbol command,
        Func<T1, T2, T3, T4, Task> handle,
        ISymbolDescriptor<T1> symbol1,
        ISymbolDescriptor<T2> symbol2,
        ISymbolDescriptor<T3> symbol3,
        ISymbolDescriptor<T4> symbol4
        ) => command.Command.SetHandler(
            handle,
            symbol1.GetValueDescriptor<IValueDescriptor<T1>>(),
            symbol2.GetValueDescriptor<IValueDescriptor<T2>>(),
            symbol3.GetValueDescriptor<IValueDescriptor<T3>>(),
            symbol4.GetValueDescriptor<IValueDescriptor<T4>>()
            );

    public static void SetHandler<T1, T2, T3, T4, T5>(
        this CommandSymbol command,
        Func<T1, T2, T3, T4, T5, Task> handle,
        ISymbolDescriptor<T1> symbol1,
        ISymbolDescriptor<T2> symbol2,
        ISymbolDescriptor<T3> symbol3,
        ISymbolDescriptor<T4> symbol4,
        ISymbolDescriptor<T5> symbol5
        ) => command.Command.SetHandler(
            handle,
            symbol1.GetValueDescriptor<IValueDescriptor<T1>>(),
            symbol2.GetValueDescriptor<IValueDescriptor<T2>>(),
            symbol3.GetValueDescriptor<IValueDescriptor<T3>>(),
            symbol4.GetValueDescriptor<IValueDescriptor<T4>>(),
            symbol5.GetValueDescriptor<IValueDescriptor<T5>>()
            );
}
