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

    public enum OffsetState : long
    {
        Undefined0 = 0, // or deliberately omit to test behavior
        First = 1,
        Second = 2,
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
    public void EnumState_LongUnderlyingEnum_BasicFlow()
    {
        var enumState = new EnumState<OffsetState>();
        enumState.Value.Be(OffsetState.Undefined0);
        enumState.Set(OffsetState.First);
        enumState.Value.Be(OffsetState.First);
        enumState.TryMove(OffsetState.First, OffsetState.Second).Be(true);
        enumState.Value.Be(OffsetState.Second);
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
}
