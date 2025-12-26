using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class EnumStateTests
{
    public enum State
    {
        None,
        Started,
        InProgress,
        Completed
    }

    [Fact]
    public void EnumState_BasicOperations_WorksAsExpected()
    {
        var enumState = new EnumState<State>();

        // Initial state should be default (None)
        enumState.Value.Be(State.None);

        // Set state to Started
        enumState.Set(State.Started);
        enumState.Value.Be(State.Started);

        // Move state from Started to InProgress
        var previousState = enumState.Move(State.Started, State.InProgress);
        previousState.Be(State.Started);
        enumState.Value.Be(State.InProgress);

        // Try to move state from Completed to None (should fail)
        var moveResult = enumState.TryMove(State.Completed, State.None);
        moveResult.Be(false);
        enumState.Value.Be(State.InProgress);

        // Try to move state from InProgress to Completed (should succeed)
        moveResult = enumState.TryMove(State.InProgress, State.Completed);
        moveResult.Be(true);
        enumState.Value.Be(State.Completed);
    }

    [Fact]
    public void EnumState_Move_ReturnsActualPreviousState()
    {
        var enumState = new EnumState<State>();
        enumState.Set(State.Started);

        // Move with wrong fromState should fail and return actual current state
        var previousState = enumState.Move(State.InProgress, State.Completed);
        previousState.Be(State.Started); // Should return Started, not InProgress
        enumState.Value.Be(State.Started); // State should not have changed
    }

    [Fact]
    public void EnumState_MultipleSetOperations_UpdatesCorrectly()
    {
        var enumState = new EnumState<State>();

        enumState.Set(State.Started);
        enumState.Value.Be(State.Started);

        enumState.Set(State.InProgress);
        enumState.Value.Be(State.InProgress);

        enumState.Set(State.None);
        enumState.Value.Be(State.None);

        enumState.Set(State.Completed);
        enumState.Value.Be(State.Completed);
    }

    [Fact]
    public void EnumState_TryMove_MultipleFailed_StateRemainsSame()
    {
        var enumState = new EnumState<State>();
        enumState.Set(State.InProgress);

        // Multiple failed attempts with wrong fromState
        enumState.TryMove(State.None, State.Completed).Be(false);
        enumState.Value.Be(State.InProgress);

        enumState.TryMove(State.Completed, State.Started).Be(false);
        enumState.Value.Be(State.InProgress);

        // Correct fromState should succeed
        enumState.TryMove(State.InProgress, State.Completed).Be(true);
        enumState.Value.Be(State.Completed);
    }

    [Fact]
    public void EnumState_AllEnumValues_CanBeSet()
    {
        var enumState = new EnumState<State>();

        // Test all enum values
        foreach (State state in Enum.GetValues<State>())
        {
            enumState.Set(state);
            enumState.Value.Be(state);
        }
    }

    [Fact]
    public void EnumState_TryMove_ConsecutiveSuccesses()
    {
        var enumState = new EnumState<State>();

        enumState.TryMove(State.None, State.Started).Be(true);
        enumState.Value.Be(State.Started);

        enumState.TryMove(State.Started, State.InProgress).Be(true);
        enumState.Value.Be(State.InProgress);

        enumState.TryMove(State.InProgress, State.Completed).Be(true);
        enumState.Value.Be(State.Completed);
    }

    [Fact]
    public void EnumState_Move_WithCorrectFromState_ChangesState()
    {
        var enumState = new EnumState<State>();
        enumState.Set(State.Started);

        var previous = enumState.Move(State.Started, State.InProgress);
        previous.Be(State.Started);
        enumState.Value.Be(State.InProgress);

        // Can move back
        previous = enumState.Move(State.InProgress, State.Started);
        previous.Be(State.InProgress);
        enumState.Value.Be(State.Started);
    }

    [Fact]
    public void EnumState_DifferentEnumType_WorksCorrectly()
    {
        var enumState = new EnumState<DayOfWeek>();

        enumState.Value.Be(DayOfWeek.Sunday); // Default value
        enumState.Set(DayOfWeek.Monday);
        enumState.Value.Be(DayOfWeek.Monday);

        enumState.TryMove(DayOfWeek.Monday, DayOfWeek.Tuesday).Be(true);
        enumState.Value.Be(DayOfWeek.Tuesday);
    }

    [Fact]
    public void EnumState_IdempotentMove_SameState()
    {
        var enumState = new EnumState<State>();
        enumState.Set(State.InProgress);
        var prev = enumState.Move(State.InProgress, State.InProgress);
        prev.Be(State.InProgress);
        enumState.Value.Be(State.InProgress);
    }

    [Fact]
    public void EnumState_IdempotentTryMove_SameState()
    {
        var enumState = new EnumState<State>();
        enumState.Set(State.Completed);
        enumState.TryMove(State.Completed, State.Completed).Be(true);
        enumState.Value.Be(State.Completed);
    }

    [Fact]
    public void EnumState_IdempotentTryMove_WrongFromState_Fails()
    {
        var enumState = new EnumState<State>();
        enumState.Set(State.InProgress);
        enumState.TryMove(State.Started, State.InProgress).Be(false);
        enumState.Value.Be(State.InProgress);
    }

    [Fact]
    public async Task EnumState_ConcurrentTransitions()
    {
        var enumState = new EnumState<State>();
        int successCount = 0;

        var tasks = Enumerable.Range(0, 100).Select(_ => Task.Run(() =>
        {
            if (enumState.TryMove(State.None, State.Started)) Interlocked.Increment(ref successCount);
            if (enumState.TryMove(State.Started, State.InProgress)) Interlocked.Increment(ref successCount);
            if (enumState.TryMove(State.InProgress, State.Completed)) Interlocked.Increment(ref successCount);
        }));

        await Task.WhenAll(tasks);

        // At most one full successful progression
        enumState.Value.Be(State.Completed);
        successCount.Be(3);
    }

    [Fact]
    public void EnumState_ConstructorWithInitialValue_SetsState()
    {
        var enumState = new EnumState<State>(State.InProgress);
        enumState.Value.Be(State.InProgress);
    }

    [Fact]
    public void EnumState_ConstructorWithInitialValue_AllValues()
    {
        foreach (State state in Enum.GetValues<State>())
        {
            var enumState = new EnumState<State>(state);
            enumState.Value.Be(state);
        }
    }

    // Note: Testing the static constructor exception requires an enum with a non-int backing type.
    // This would need a byte/short/long-based enum to trigger the NotSupportedException.
    public enum ByteState : byte { A, B }

    [Fact]
    public void EnumState_NonInt32Enum_ThrowsNotSupportedException()
    {
        // The static constructor throws when the enum's underlying type is not 32-bit
        var exception = Assert.Throws<TypeInitializationException>(() => new EnumState<ByteState>());
        Assert.IsType<NotSupportedException>(exception.InnerException);
    }

    [Fact]
    public void EnumState_IfValue_ReturnsCorrectResult()
    {
        var enumState = new EnumState<State>(State.InProgress);

        enumState.IfValue(State.InProgress).Be(true);
        enumState.IfValue(State.None).Be(false);
        enumState.IfValue(State.Started).Be(false);
        enumState.IfValue(State.Completed).Be(false);

        enumState.Set(State.Completed);
        enumState.IfValue(State.Completed).Be(true);
        enumState.IfValue(State.InProgress).Be(false);
    }

    [Fact]
    public async Task EnumState_IfValue_ConcurrentReads()
    {
        var enumState = new EnumState<State>(State.Started);

        var tasks = Enumerable.Range(0, 100).Select(_ => Task.Run(() =>
        {
            State current = enumState.Value;
            if (!enumState.IfValue(current))
            {
                // State changed between reads - this is expected under concurrency
                // but IfValue should still return a consistent atomic read
            }

            // IfValue should be consistent with itself
            bool check1 = enumState.IfValue(State.Started);
            bool check2 = enumState.IfValue(State.Started);
            // Note: These could differ if another thread changes state between calls
        }));

        await Task.WhenAll(tasks);
    }
}
