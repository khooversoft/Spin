namespace Toolbox.Tools;

/// <summary>
/// Provides a disposable scope that executes a finalization action when disposed, with optional cancellation support.
/// Uses thread-safe operations to ensure finalize or cancel actions execute exactly once.
/// </summary>
/// <remarks>
/// <para>
/// This class implements a pattern for executing cleanup or finalization logic at the end of a scope.
/// When disposed, the finalization action is invoked unless <see cref="Cancel"/> was called first.
/// </para>
/// <para>
/// Thread Safety: All operations are thread-safe using <see cref="Interlocked"/> to ensure actions execute exactly once.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// using (var scope = new FinalizeScope(() => Console.WriteLine("Finalized")))
/// {
///     // Do work
///     // If not cancelled, "Finalized" will be printed when scope is disposed
/// }
/// </code>
/// </para>
/// </remarks>
public sealed class FinalizeScope : IDisposable
{
    private Action? _finalizeAction;
    private Action? _cancelAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="FinalizeScope"/> class with a finalization action.
    /// </summary>
    /// <param name="finalizeAction">The action to execute when the scope is disposed. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="finalizeAction"/> is null.</exception>
    public FinalizeScope(Action finalizeAction) => _finalizeAction = finalizeAction.NotNull();

    /// <summary>
    /// Initializes a new instance of the <see cref="FinalizeScope"/> class with both finalization and cancellation actions.
    /// </summary>
    /// <param name="finalizeAction">The action to execute when the scope is disposed normally. Cannot be null.</param>
    /// <param name="cancelAction">The action to execute when <see cref="Cancel"/> is called. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="finalizeAction"/> or <paramref name="cancelAction"/> is null.</exception>
    public FinalizeScope(Action finalizeAction, Action cancelAction)
    {
        _finalizeAction = finalizeAction.NotNull();
        _cancelAction = cancelAction.NotNull();
    }

    /// <summary>
    /// Cancels the finalization action and executes the cancellation action if provided.
    /// </summary>
    /// <remarks>
    /// This method is thread-safe. Both finalization and cancellation actions are cleared atomically,
    /// and only the cancellation action (if provided) is invoked. Subsequent calls to <see cref="Cancel"/> 
    /// or <see cref="Dispose"/> will have no effect.
    /// </remarks>
    public void Cancel()
    {
        _ = Interlocked.Exchange(ref _finalizeAction, null);
        Interlocked.Exchange(ref _cancelAction, null)?.Invoke();
    }

    /// <summary>
    /// Disposes the scope and executes the finalization action if not cancelled.
    /// </summary>
    /// <remarks>
    /// This method is thread-safe. Both cancellation and finalization actions are cleared atomically,
    /// and only the finalization action is invoked. If <see cref="Cancel"/> was called previously,
    /// no action is executed.
    /// </remarks>
    public void Dispose()
    {
        _ = Interlocked.Exchange(ref _cancelAction, null);
        Interlocked.Exchange(ref _finalizeAction, null)?.Invoke();
    }
}

/// <summary>
/// Provides a disposable scope that executes a finalization action with a captured value when disposed, 
/// with optional cancellation support and implicit conversion to the underlying value.
/// Uses thread-safe operations to ensure finalize or cancel actions execute exactly once.
/// </summary>
/// <typeparam name="T">The type of value to capture and pass to the finalization/cancellation actions.</typeparam>
/// <remarks>
/// <para>
/// This class extends the finalization scope pattern by capturing a value that is passed to the 
/// finalization or cancellation action. The scope can be implicitly converted to the underlying value,
/// allowing it to be used transparently in expressions.
/// </para>
/// <para>
/// Thread Safety: All operations are thread-safe using <see cref="Interlocked"/> to ensure actions execute exactly once.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// using var scope = new FinalizeScope&lt;FileStream&gt;(
///     stream,
///     s => s.Flush(),
///     s => s.Close()
/// );
/// 
/// // Use scope implicitly as the stream
/// scope.Write(data, 0, data.Length);
/// </code>
/// </para>
/// </remarks>
public sealed class FinalizeScope<T> : IDisposable
{
    private readonly T _value;
    private Action<T>? _finalizeAction;
    private Action<T>? _cancelAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="FinalizeScope{T}"/> class with a value and finalization action.
    /// </summary>
    /// <param name="value">The value to capture and pass to the finalization action.</param>
    /// <param name="finalizeAction">The action to execute with the captured value when the scope is disposed. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="finalizeAction"/> is null.</exception>
    public FinalizeScope(T value, Action<T> finalizeAction)
    {
        _value = value;
        _finalizeAction = finalizeAction.NotNull();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FinalizeScope{T}"/> class with a value, finalization action, and cancellation action.
    /// </summary>
    /// <param name="value">The value to capture and pass to the actions.</param>
    /// <param name="finalizeAction">The action to execute with the captured value when the scope is disposed normally. Cannot be null.</param>
    /// <param name="cancelAction">The action to execute with the captured value when <see cref="Cancel"/> is called. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="finalizeAction"/> or <paramref name="cancelAction"/> is null.</exception>
    public FinalizeScope(T value, Action<T> finalizeAction, Action<T> cancelAction)
    {
        _value = value;
        _finalizeAction = finalizeAction.NotNull();
        _cancelAction = cancelAction.NotNull();
    }

    /// <summary>
    /// Cancels the finalization action and executes the cancellation action with the captured value if provided.
    /// </summary>
    /// <remarks>
    /// This method is thread-safe. Both finalization and cancellation actions are cleared atomically,
    /// and only the cancellation action (if provided) is invoked with the captured value. 
    /// Subsequent calls to <see cref="Cancel"/> or <see cref="Dispose"/> will have no effect.
    /// </remarks>
    public void Cancel()
    {
        _ = Interlocked.Exchange(ref _finalizeAction, null);
        Interlocked.Exchange(ref _cancelAction, null)?.Invoke(_value);
    }

    /// <summary>
    /// Disposes the scope and executes the finalization action with the captured value if not cancelled.
    /// </summary>
    /// <remarks>
    /// This method is thread-safe. Both cancellation and finalization actions are cleared atomically,
    /// and only the finalization action is invoked with the captured value. If <see cref="Cancel"/> 
    /// was called previously, no action is executed.
    /// </remarks>
    public void Dispose()
    {
        _ = Interlocked.Exchange(ref _cancelAction, null);
        Interlocked.Exchange(ref _finalizeAction, null)?.Invoke(_value);
    }

    /// <summary>
    /// Implicitly converts the <see cref="FinalizeScope{T}"/> to its underlying value.
    /// </summary>
    /// <param name="scope">The scope to convert.</param>
    /// <returns>The captured value of type <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// This operator allows the scope to be used transparently wherever the underlying value type is expected,
    /// enabling patterns like: <c>using var file = new FinalizeScope&lt;FileStream&gt;(...); file.Write(...);</c>
    /// </remarks>
    public static implicit operator T(FinalizeScope<T> scope) => scope._value;
}
