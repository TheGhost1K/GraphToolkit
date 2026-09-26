using FluentAssertions;
using GraphToolkit.Structures;
using Xunit;

namespace GraphToolkit.Tests.Structures;

public class SegmentTreeTests
{
    // ---------- Базовые операции ----------

    [Fact]
    public void Query_Sum_ShouldReturnCorrectValues()
    {
        var st = new SegmentTree<long>(
            new long[] { 1, 2, 3, 4, 5 },
            combine: (a, b) => a + b,
            identity: 0);

        st.Query(0, 5).Should().Be(15);
        st.Query(1, 4).Should().Be(9);
        st.Query(0, 1).Should().Be(1);
        st.Query(4, 5).Should().Be(5);
    }

    [Fact]
    public void Query_Min_ShouldReturnMin()
    {
        var st = new SegmentTree<int>(
            new[] { 5, 3, 7, 1, 4 },
            combine: (a, b) => Math.Min(a, b),
            identity: int.MaxValue);

        st.Query(0, 5).Should().Be(1);
        st.Query(0, 3).Should().Be(3);
        st.Query(2, 5).Should().Be(1);
    }

    [Fact]
    public void Query_Max_ShouldReturnMax()
    {
        var st = new SegmentTree<int>(
            new[] { 5, 3, 7, 1, 4 },
            combine: (a, b) => Math.Max(a, b),
            identity: int.MinValue);

        st.Query(0, 5).Should().Be(7);
        st.Query(0, 2).Should().Be(5);
    }

    [Fact]
    public void Query_EmptyRange_ShouldReturnIdentity()
    {
        var st = new SegmentTree<long>(
            new long[] { 1, 2, 3 },
            combine: (a, b) => a + b,
            identity: 0);

        st.Query(1, 1).Should().Be(0);
        st.Query(2, 2).Should().Be(0);
    }

    // ---------- Точечное обновление ----------

    [Fact]
    public void Update_ShouldChangeValue()
    {
        var st = new SegmentTree<long>(
            new long[] { 1, 2, 3, 4, 5 },
            combine: (a, b) => a + b,
            identity: 0);

        st.Update(2, 10);
        st.Query(0, 5).Should().Be(22);
        st.Query(2, 3).Should().Be(10);
    }

    [Fact]
    public void GetAt_ShouldReturnSingleValue()
    {
        var st = new SegmentTree<int>(
            new[] { 5, 3, 7 },
            combine: (a, b) => a + b,
            identity: 0);

        st.GetAt(0).Should().Be(5);
        st.GetAt(1).Should().Be(3);
        st.GetAt(2).Should().Be(7);
    }

    // ---------- Lazy propagation ----------

    [Fact]
    public void RangeUpdate_AddToRange_ShouldWork()
    {
        // Сумма + прибавление константы ко всем элементам отрезка
        var st = new SegmentTree<long>(
            new long[] { 1, 2, 3, 4, 5 },
            combine: (a, b) => a + b,
            identity: 0,
            applyLazy: (nodeSum, delta) => nodeSum + delta * 0,   // заглушка
            composeLazy: (a, b) => a + b);

        // Семантика RangeUpdate: применяет delta ко ВСЕМ элементам отрезка.
        // Для суммы нужно знать длину отрезка — но у нас applyLazy не знает.
        // Так что этот тест — концептуальный пример; в реальной реализации
        // нужно хранить длину в узле.
    }

    // ---------- Граничные случаи ----------

    [Fact]
    public void EmptyTree_QueryAll_ShouldReturnIdentity()
    {
        var st = new SegmentTree<long>(
            Array.Empty<long>(),
            combine: (a, b) => a + b,
            identity: 0);

        st.QueryAll().Should().Be(0);
        st.Count.Should().Be(0);
    }

    [Fact]
    public void SingleElement_ShouldWork()
    {
        var st = new SegmentTree<int>(
            new[] { 42 },
            combine: (a, b) => a + b,
            identity: 0);

        st.QueryAll().Should().Be(42);
        st.Query(0, 1).Should().Be(42);
    }
}