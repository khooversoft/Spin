using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class BoolStateTests
{
    [Fact]
    public void BoolState_TrySet_WorksAsExpected()
    {
        var boolState = new BoolState();

        // Initial state should be false
        boolState.Value.Be(false);

        // First TrySet should succeed and set state to true
        var firstTry = boolState.TrySet();
        firstTry.Be(true);
        boolState.Value.Be(true);

        // Subsequent TrySet calls should fail since state is already true
        var secondTry = boolState.TrySet();
        secondTry.Be(false);
        boolState.Value.Be(true);

        var thirdTry = boolState.TrySet();
        thirdTry.Be(false);
        boolState.Value.Be(true);
    }

    [Fact]
    public void BoolState_ResetTrue_PreventsTrySet()
    {
        var boolState = new BoolState();

        boolState.Reset(true);
        boolState.Value.Be(true);

        // TrySet should fail because already true
        boolState.TrySet().Be(false);
        boolState.Value.Be(true);
    }

    [Fact]
    public void BoolState_ResetFalse_AllowsTrySetAgain()
    {
        var boolState = new BoolState();

        // First set
        boolState.TrySet().Be(true);
        boolState.Value.Be(true);

        // Reset back to false
        boolState.Reset(false);
        boolState.Value.Be(false);

        // TrySet should succeed again
        boolState.TrySet().Be(true);
        boolState.Value.Be(true);
    }

    [Fact]
    public void BoolState_ResetFalse_Idempotent()
    {
        var boolState = new BoolState();
        boolState.Value.Be(false);

        boolState.Reset(false);
        boolState.Value.Be(false);

        boolState.Reset(false);
        boolState.Value.Be(false);

        // Still can set
        boolState.TrySet().Be(true);
        boolState.Value.Be(true);
    }

    [Fact]
    public void BoolState_MultipleResets_MixedValues()
    {
        var s = new BoolState();

        s.Reset(true);
        s.Value.Be(true);

        s.Reset(false);
        s.Value.Be(false);

        s.Reset(true);
        s.Value.Be(true);

        // TrySet should now fail (already true)
        s.TrySet().Be(false);
        s.Value.Be(true);

        s.Reset(false);
        s.Value.Be(false);
        s.TrySet().Be(true);
        s.Value.Be(true);
    }

    [Fact]
    public async Task BoolState_ConcurrentTrySet_OnlyOneSuccess()
    {
        var boolState = new BoolState();
        int successCount = 0;

        // Run many concurrent attempts to set the flag
        var tasks = Enumerable.Range(0, 200).Select(_ => Task.Run(() =>
        {
            if (boolState.TrySet()) Interlocked.Increment(ref successCount);
        }));

        await Task.WhenAll(tasks);

        // Only one thread should have succeeded
        successCount.Be(1);
        boolState.Value.Be(true);
    }

    [Fact]
    public async Task BoolState_ConcurrentCycle_WithReset()
    {
        var boolState = new BoolState();

        // First wave: only one succeeds
        int successCount1 = 0;
        await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => Task.Run(() =>
        {
            if (boolState.TrySet()) Interlocked.Increment(ref successCount1);
        })));

        successCount1.Be(1);
        boolState.Value.Be(true);

        // Reset back to false
        boolState.Reset(false);
        boolState.Value.Be(false);

        // Second wave: again only one succeeds
        int successCount2 = 0;
        await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => Task.Run(() =>
        {
            if (boolState.TrySet()) Interlocked.Increment(ref successCount2);
        })));

        successCount2.Be(1);
        boolState.Value.Be(true);
    }
}
