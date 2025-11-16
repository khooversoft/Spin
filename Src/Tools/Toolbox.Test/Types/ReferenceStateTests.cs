using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ReferenceStateTests
{
    public record ReferenceRecord
    {
        public string Name { get; init; } = string.Empty;
    }

    [Fact]
    public void ReferenceState_BasicOperations_WorksAsExpected()
    {
        var initialRecord = new ReferenceRecord { Name = "Initial" };
        var newRecord = new ReferenceRecord { Name = "New" };
        var anotherRecord = new ReferenceRecord { Name = "Another" };

        var referenceState = new ReferenceState<ReferenceRecord>(initialRecord);
        // Initial state should be the initial record
        referenceState.Value.Be(initialRecord);

        // Set state to new record
        referenceState.Set(newRecord);
        referenceState.Value.Be(newRecord);

        // Move state from new record to another record
        var previousState = referenceState.Move(newRecord, anotherRecord);
        previousState.Be(newRecord);
        referenceState.Value.Be(anotherRecord);

        // Try to move using wrong from (should fail)
        var moveResult = referenceState.TryMove(initialRecord, initialRecord);
        moveResult.Be(false);
        referenceState.Value.Be(anotherRecord);

        // Try to move state from another record to initial record (should succeed)
        moveResult = referenceState.TryMove(anotherRecord, initialRecord);
        moveResult.Be(true);
        referenceState.Value.Be(initialRecord);
    }

    [Fact]
    public void DefaultConstructor_InitialValueIsNull()
    {
        var state = new ReferenceState<ReferenceRecord>();
        state.Value.BeNull();
    }

    [Fact]
    public void Set_FromNull_Works()
    {
        var state = new ReferenceState<ReferenceRecord>();
        var value = new ReferenceRecord { Name = "A" };
        state.Set(value);
        state.Value.Be(value);
    }

    [Fact]
    public void Move_FailsWhenFromStateMismatch_ValueUnchanged_ReturnsCurrent()
    {
        var initial = new ReferenceRecord { Name = "Initial" };
        var wrongFrom = new ReferenceRecord { Name = "Wrong" };
        var newValue = new ReferenceRecord { Name = "New" };

        var state = new ReferenceState<ReferenceRecord>(initial);

        // fromState does not match current; should not change
        var returned = state.Move(wrongFrom, newValue);

        returned.Be(initial);          // CompareExchange returns current when no swap
        state.Value.Be(initial);       // Value unchanged
    }

    [Fact]
    public void Move_Idempotent_WhenStateEqualsFromState()
    {
        var item = new ReferenceRecord { Name = "Same" };
        var state = new ReferenceState<ReferenceRecord>(item);

        // Move to same instance (state == fromState) => sets same value, returns previous (same)
        var returned = state.Move(item, item);

        returned.Be(item);
        state.Value.Be(item);
    }

    [Fact]
    public void TryMove_SucceedsWhenFromMatches()
    {
        var start = new ReferenceRecord { Name = "Start" };
        var to = new ReferenceRecord { Name = "To" };

        var state = new ReferenceState<ReferenceRecord>(start);

        var result = state.TryMove(start, to);
        result.Be(true);
        state.Value.Be(to);
    }

    [Fact]
    public void TryMove_FailsWhenFromMismatch()
    {
        var start = new ReferenceRecord { Name = "Start" };
        var other = new ReferenceRecord { Name = "Other" };
        var to = new ReferenceRecord { Name = "To" };

        var state = new ReferenceState<ReferenceRecord>(start);

        var result = state.TryMove(other, to);
        result.Be(false);
        state.Value.Be(start);
    }

    [Fact]
    public void MovesUseEquityIdentity_NotValueReferenceEquality()
    {
        var a1 = new ReferenceRecord { Name = "A" };
        var a2 = new ReferenceRecord { Name = "A" }; // Different instance, same name
        var target = new ReferenceRecord { Name = "Target" };

        var state = new ReferenceState<ReferenceRecord>(a1);

        // Using fromState = a2 (value-equal to a1) returns true, but no swap occurs
        var moveResult = state.TryMove(a2, target);
        moveResult.Be(true);
        state.Value.Be(a1);

        // Using fromState = a1 should succeed and swap
        moveResult = state.TryMove(a1, target);
        moveResult.Be(true);
        state.Value.Be(target);
    }

    [Fact]
    public void MovesUseEquityIdentity_NotValueEquality()
    {
        var a1 = new ReferenceRecord { Name = "A" };
        var a2 = new ReferenceRecord { Name = "B" }; // Different instance, different name
        var target = new ReferenceRecord { Name = "Target" };

        var state = new ReferenceState<ReferenceRecord>(a1);

        // Using fromState = a2 should fail (not equal to current)
        var moveResult = state.TryMove(a2, target);
        moveResult.Be(false);
        state.Value.Be(a1);

        // Using fromState = a1 should succeed
        moveResult = state.TryMove(a1, target);
        moveResult.Be(true);
        state.Value.Be(target);
    }

    [Fact]
    public void TryMove_FromNull_CurrentBehavior_ReportSuccess()
    {
        // Documenting current behavior: when initial value is null,
        var newValue = new ReferenceRecord { Name = "New" };
        var state = new ReferenceState<ReferenceRecord>(); // Value == null

        var result = state.TryMove(null, newValue);
        result.Be(true);
        state.Value.Be(newValue);
    }
}
