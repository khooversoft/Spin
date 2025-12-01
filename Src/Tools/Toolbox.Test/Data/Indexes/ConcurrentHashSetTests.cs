using Toolbox.Data;
using Toolbox.Tools;

namespace Toolbox.Test.Data.Indexes;

public class ConcurrentHashSetTests
{
    [Fact]
    public void Empty()
    {
        var set = new ConcurrentHashSet<string>();
        set.Count.Be(0);
        set.IsEmpty.BeTrue();
    }

    [Fact]
    public void TryAdd_NewItem_ReturnsTrue()
    {
        var set = new ConcurrentHashSet<string>();

        bool result = set.TryAdd("item1");

        result.BeTrue();
        set.Count.Be(1);
        set.IsEmpty.BeFalse();
        set.Contains("item1").BeTrue();
    }

    [Fact]
    public void TryAdd_DuplicateItem_ReturnsFalse()
    {
        var set = new ConcurrentHashSet<string>();
        set.TryAdd("item1");

        bool result = set.TryAdd("item1");

        result.BeFalse();
        set.Count.Be(1);
    }

    [Fact]
    public void TryAdd_MultipleItems_AllAdded()
    {
        var set = new ConcurrentHashSet<string>();

        set.TryAdd("item1").BeTrue();
        set.TryAdd("item2").BeTrue();
        set.TryAdd("item3").BeTrue();

        set.Count.Be(3);
        set.Contains("item1").BeTrue();
        set.Contains("item2").BeTrue();
        set.Contains("item3").BeTrue();
    }

    [Fact]
    public void TryRemove_ExistingItem_ReturnsTrue()
    {
        var set = new ConcurrentHashSet<string>();
        set.TryAdd("item1");

        bool result = set.TryRemove("item1");

        result.BeTrue();
        set.Count.Be(0);
        set.IsEmpty.BeTrue();
        set.Contains("item1").BeFalse();
    }

    [Fact]
    public void TryRemove_NonExistingItem_ReturnsFalse()
    {
        var set = new ConcurrentHashSet<string>();

        bool result = set.TryRemove("nonexistent");

        result.BeFalse();
        set.Count.Be(0);
    }

    [Fact]
    public void TryRemove_FromPopulatedSet_RemovesOnlySpecifiedItem()
    {
        var set = new ConcurrentHashSet<string>();
        set.TryAdd("item1");
        set.TryAdd("item2");
        set.TryAdd("item3");

        set.TryRemove("item2").BeTrue();

        set.Count.Be(2);
        set.Contains("item1").BeTrue();
        set.Contains("item2").BeFalse();
        set.Contains("item3").BeTrue();
    }

    [Fact]
    public void Contains_ExistingItem_ReturnsTrue()
    {
        var set = new ConcurrentHashSet<string>();
        set.TryAdd("item1");

        set.Contains("item1").BeTrue();
    }

    [Fact]
    public void Contains_NonExistingItem_ReturnsFalse()
    {
        var set = new ConcurrentHashSet<string>();
        set.TryAdd("item1");

        set.Contains("item2").BeFalse();
    }

    [Fact]
    public void Clear_EmptySet_RemainsEmpty()
    {
        var set = new ConcurrentHashSet<string>();

        set.Clear();

        set.Count.Be(0);
        set.IsEmpty.BeTrue();
    }

    [Fact]
    public void Clear_PopulatedSet_BecomesEmpty()
    {
        var set = new ConcurrentHashSet<string>();
        set.TryAdd("item1");
        set.TryAdd("item2");
        set.TryAdd("item3");

        set.Clear();

        set.Count.Be(0);
        set.IsEmpty.BeTrue();
        set.Contains("item1").BeFalse();
        set.Contains("item2").BeFalse();
        set.Contains("item3").BeFalse();
    }

    [Fact]
    public void IsEmpty_WhenEmpty_ReturnsTrue()
    {
        var set = new ConcurrentHashSet<string>();

        set.IsEmpty.BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenPopulated_ReturnsFalse()
    {
        var set = new ConcurrentHashSet<string>();
        set.TryAdd("item1");

        set.IsEmpty.BeFalse();
    }

    [Fact]
    public void Count_AfterAddAndRemove_IsCorrect()
    {
        var set = new ConcurrentHashSet<string>();

        set.Count.Be(0);

        set.TryAdd("item1");
        set.Count.Be(1);

        set.TryAdd("item2");
        set.Count.Be(2);

        set.TryRemove("item1");
        set.Count.Be(1);

        set.TryRemove("item2");
        set.Count.Be(0);
    }

    [Fact]
    public void ToImmutableArray_EmptySet_ReturnsEmptyArray()
    {
        var set = new ConcurrentHashSet<string>();

        var array = set.ToImmutableArray();

        array.Length.Be(0);
    }

    [Fact]
    public void ToImmutableArray_PopulatedSet_ContainsAllItems()
    {
        var set = new ConcurrentHashSet<string>();
        set.TryAdd("item1");
        set.TryAdd("item2");
        set.TryAdd("item3");

        var array = set.ToImmutableArray();

        array.Length.Be(3);
        array.Contains("item1").BeTrue();
        array.Contains("item2").BeTrue();
        array.Contains("item3").BeTrue();
    }

    [Fact]
    public void GetEnumerator_EmptySet_NoItems()
    {
        var set = new ConcurrentHashSet<string>();

        var items = set.ToList();

        items.Count.Be(0);
    }

    [Fact]
    public void GetEnumerator_PopulatedSet_IteratesAllItems()
    {
        var set = new ConcurrentHashSet<string>();
        set.TryAdd("item1");
        set.TryAdd("item2");
        set.TryAdd("item3");

        var items = set.ToList();

        items.Count.Be(3);
        items.Contains("item1").BeTrue();
        items.Contains("item2").BeTrue();
        items.Contains("item3").BeTrue();
    }

    [Fact]
    public void LinqOperations_WorkCorrectly()
    {
        var set = new ConcurrentHashSet<string>();
        set.TryAdd("apple");
        set.TryAdd("banana");
        set.TryAdd("apricot");

        var filtered = set.Where(x => x.StartsWith("a")).ToList();

        filtered.Count.Be(2);
        filtered.Contains("apple").BeTrue();
        filtered.Contains("apricot").BeTrue();
    }

    [Fact]
    public void Constructor_WithCustomComparer_UsesComparer()
    {
        var set = new ConcurrentHashSet<string>(StringComparer.OrdinalIgnoreCase);
        set.TryAdd("Item1");

        set.Contains("ITEM1").BeTrue();
        set.Contains("item1").BeTrue();
        set.TryAdd("item1").BeFalse(); // Should fail as it's considered duplicate
        set.Count.Be(1);
    }

    [Fact]
    public void Constructor_WithDefaultComparer_CaseSensitive()
    {
        var set = new ConcurrentHashSet<string>();
        set.TryAdd("Item1");

        set.Contains("ITEM1").BeFalse();
        set.Contains("Item1").BeTrue();
        set.TryAdd("ITEM1").BeTrue(); // Should succeed as it's different
        set.Count.Be(2);
    }

    [Fact]
    public async Task ConcurrentAdd_MultipleThreads_AllItemsAdded()
    {
        var set = new ConcurrentHashSet<int>();
        const int threadCount = 10;
        const int itemsPerThread = 100;

        var tasks = Enumerable.Range(0, threadCount)
            .Select(async threadId =>
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < itemsPerThread; i++)
                    {
                        int value = threadId * itemsPerThread + i;
                        set.TryAdd(value);
                    }
                });
            });

        await Task.WhenAll(tasks);

        set.Count.Be(threadCount * itemsPerThread);
    }

    [Fact]
    public async Task ConcurrentRemove_MultipleThreads_ThreadSafe()
    {
        var set = new ConcurrentHashSet<int>();
        const int itemCount = 1000;

        // Populate the set
        for (int i = 0; i < itemCount; i++)
        {
            set.TryAdd(i);
        }

        // Remove items concurrently
        var tasks = Enumerable.Range(0, itemCount)
            .Select(i => Task.Run(() => set.TryRemove(i)));

        await Task.WhenAll(tasks);

        set.Count.Be(0);
        set.IsEmpty.BeTrue();
    }

    [Fact]
    public async Task ConcurrentMixedOperations_ThreadSafe()
    {
        var set = new ConcurrentHashSet<int>();
        const int operationCount = 100;

        var addTasks = Enumerable.Range(0, operationCount)
            .Select(i => Task.Run(() => set.TryAdd(i)));

        var containsTasks = Enumerable.Range(0, operationCount)
            .Select(i => Task.Run(() => set.Contains(i)));

        var removeTasks = Enumerable.Range(0, operationCount / 2)
            .Select(i => Task.Run(() => set.TryRemove(i)));

        await Task.WhenAll(addTasks.Concat(containsTasks).Concat(removeTasks));

        // After all operations, we should have items that weren't removed
        set.Count.Be(operationCount / 2);

        for (int i = operationCount / 2; i < operationCount; i++)
        {
            set.Contains(i).BeTrue();
        }
    }

    [Fact]
    public async Task ConcurrentAddAndEnumerate_NoExceptions()
    {
        var set = new ConcurrentHashSet<int>();
        bool enumerationCompleted = false;

        var addTask = Task.Run(async () =>
        {
            for (int i = 0; i < 1000; i++)
            {
                set.TryAdd(i);
                await Task.Delay(1);
            }
        });

        var enumerateTask = Task.Run(async () =>
        {
            await Task.Delay(10);
            for (int i = 0; i < 50; i++)
            {
                var _ = set.ToList(); // Force enumeration
                await Task.Delay(10);
            }
            enumerationCompleted = true;
        });

        await Task.WhenAll(addTask, enumerateTask);

        enumerationCompleted.BeTrue();
    }

    [Fact]
    public void IntegerSet_WorksCorrectly()
    {
        var set = new ConcurrentHashSet<int>();

        set.TryAdd(1).BeTrue();
        set.TryAdd(2).BeTrue();
        set.TryAdd(1).BeFalse();

        set.Count.Be(2);
        set.Contains(1).BeTrue();
        set.Contains(2).BeTrue();
        set.Contains(3).BeFalse();
    }

    [Fact]
    public void CustomTypeSet_WorksCorrectly()
    {
        var set = new ConcurrentHashSet<TestRecord>();
        var item1 = new TestRecord(1, "Test1");
        var item2 = new TestRecord(2, "Test2");
        var item1Duplicate = new TestRecord(1, "Test1");

        set.TryAdd(item1).BeTrue();
        set.TryAdd(item2).BeTrue();
        set.TryAdd(item1Duplicate).BeFalse(); // Record equality

        set.Count.Be(2);
        set.Contains(item1).BeTrue();
        set.Contains(item1Duplicate).BeTrue(); // Record equality
    }

    [Fact]
    public void InterfaceType_WithReferenceEquality_WorksCorrectly()
    {
        var set = new ConcurrentHashSet<ITestEntity>();
        var entity1 = new TestEntity(1, "Entity1");
        var entity2 = new TestEntity(2, "Entity2");
        var entity1Reference = entity1; // Same reference

        set.TryAdd(entity1).BeTrue();
        set.TryAdd(entity2).BeTrue();
        set.TryAdd(entity1Reference).BeFalse(); // Same reference, should fail

        set.Count.Be(2);
        set.Contains(entity1).BeTrue();
        set.Contains(entity2).BeTrue();
        set.Contains(entity1Reference).BeTrue(); // Same reference
    }

    [Fact]
    public void InterfaceType_WithDifferentInstances_AddsMultiple()
    {
        var set = new ConcurrentHashSet<ITestEntity>();
        var entity1 = new TestEntity(1, "Entity1");
        var entity2 = new TestEntity(1, "Entity1"); // Different instance, same values

        set.TryAdd(entity1).BeTrue();
        set.TryAdd(entity2).BeTrue(); // Different reference, should succeed

        set.Count.Be(2);
        set.Contains(entity1).BeTrue();
        set.Contains(entity2).BeTrue();
    }

    [Fact]
    public void InterfaceType_Remove_WorksCorrectly()
    {
        var set = new ConcurrentHashSet<ITestEntity>();
        var entity1 = new TestEntity(1, "Entity1");
        var entity2 = new TestEntity(2, "Entity2");

        set.TryAdd(entity1);
        set.TryAdd(entity2);

        set.TryRemove(entity1).BeTrue();

        set.Count.Be(1);
        set.Contains(entity1).BeFalse();
        set.Contains(entity2).BeTrue();
    }

    [Fact]
    public void InterfaceType_Enumeration_WorksCorrectly()
    {
        var set = new ConcurrentHashSet<ITestEntity>();
        var entity1 = new TestEntity(1, "Entity1");
        var entity2 = new TestEntity(2, "Entity2");
        var entity3 = new TestEntity(3, "Entity3");

        set.TryAdd(entity1);
        set.TryAdd(entity2);
        set.TryAdd(entity3);

        var items = set.ToList();

        items.Count.Be(3);
        items.Contains(entity1).BeTrue();
        items.Contains(entity2).BeTrue();
        items.Contains(entity3).BeTrue();
    }

    [Fact]
    public void InterfaceType_LinqQueries_WorkCorrectly()
    {
        var set = new ConcurrentHashSet<ITestEntity>();
        var entity1 = new TestEntity(1, "Entity1");
        var entity2 = new TestEntity(2, "Entity2");
        var entity3 = new TestEntity(10, "Entity10");

        set.TryAdd(entity1);
        set.TryAdd(entity2);
        set.TryAdd(entity3);

        var filtered = set.Where(x => x.Id >= 2).ToList();

        filtered.Count.Be(2);
        filtered.Contains(entity2).BeTrue();
        filtered.Contains(entity3).BeTrue();
    }

    [Fact]
    public void InterfaceType_ToImmutableArray_WorksCorrectly()
    {
        var set = new ConcurrentHashSet<ITestEntity>();
        var entity1 = new TestEntity(1, "Entity1");
        var entity2 = new TestEntity(2, "Entity2");

        set.TryAdd(entity1);
        set.TryAdd(entity2);

        var array = set.ToImmutableArray();

        array.Length.Be(2);
        array.Contains(entity1).BeTrue();
        array.Contains(entity2).BeTrue();
    }

    [Fact]
    public void InterfaceType_Clear_WorksCorrectly()
    {
        var set = new ConcurrentHashSet<ITestEntity>();
        var entity1 = new TestEntity(1, "Entity1");
        var entity2 = new TestEntity(2, "Entity2");

        set.TryAdd(entity1);
        set.TryAdd(entity2);

        set.Clear();

        set.Count.Be(0);
        set.IsEmpty.BeTrue();
        set.Contains(entity1).BeFalse();
        set.Contains(entity2).BeFalse();
    }

    [Fact]
    public async Task InterfaceType_ConcurrentOperations_ThreadSafe()
    {
        var set = new ConcurrentHashSet<ITestEntity>();
        const int entityCount = 100;
        var entities = Enumerable.Range(0, entityCount)
            .Select(i => (ITestEntity)new TestEntity(i, $"Entity{i}"))
            .ToList();

        var addTasks = entities.Select(e => Task.Run(() => set.TryAdd(e)));

        await Task.WhenAll(addTasks);

        set.Count.Be(entityCount);

        foreach (var entity in entities)
        {
            set.Contains(entity).BeTrue();
        }
    }

    private interface ITestEntity
    {
        int Id { get; }
        string Name { get; }
    }

    private class TestEntity : ITestEntity
    {
        public TestEntity(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string Name { get; }
    }

    private record TestRecord(int Id, string Name);
}
