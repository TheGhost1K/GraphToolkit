using FluentAssertions;
using GraphToolkit.Structures;
using Xunit;

namespace GraphToolkit.Tests.Structures;

/// <summary>
/// Тесты RangeUpdate для операции min с присвоением значения.
/// Семантика: applyLazy = присвоить значение всем элементам отрезка.
/// Это не требует знания длины отрезка.
/// </summary>
public class SegmentTreeRangeUpdateTests
{
    [Fact]
    public void RangeUpdate_MinAssign_ShouldWork()
    {
        // min-дерево, applyLazy(node, value) = value (присвоение)
        // composeLazy(old, new) = new (новое присвоение перекрывает старое)
        var st = new SegmentTree<int>(
            new[] { 5, 3, 7, 1, 4 },
            combine: Math.Min,
            identity: int.MaxValue,
            applyLazy: (_, newValue) => newValue,
            composeLazy: (_, newer) => newer);

        st.Query(0, 5).Should().Be(1);

        // Присваиваем 10 всему отрезку [1, 3) — позиции 1, 2
        st.RangeUpdate(1, 3, 10);

        st.Query(1, 3).Should().Be(10);          // min(10, 10) = 10
        st.Query(0, 5).Should().Be(1);           // global min: 1 всё ещё в позиции 3
    }

    [Fact]
    public void RangeUpdate_SumWithLength_ShouldWork()
    {
        // Для суммы нужно хранить (sum, length) в узле, чтобы applyLazy
        // знал, сколько прибавлять. Используем tuple.
        var st = new SegmentTree<(long Sum, int Len)>(
            new[] {
                (1L, 1), (2L, 1), (3L, 1), (4L, 1)
            },
            combine: (a, b) => (a.Sum + b.Sum, a.Len + b.Len),
            identity: (0L, 0),
            applyLazy: (node, delta) => (node.Sum + delta.Sum * node.Len, node.Len),
            composeLazy: (a, b) => (a.Sum + b.Sum, 0));

        st.Query(0, 4).Sum.Should().Be(10);

        // Прибавляем 10 к отрезку [1, 3) — позиции 1, 2 (2 элемента)
        st.RangeUpdate(1, 3, (10L, 0));

        st.Query(0, 4).Sum.Should().Be(30);      // 1 + 12 + 13 + 4 = 30
        st.Query(1, 3).Sum.Should().Be(25);      // 12 + 13 = 25
    }

    [Fact]
    public void RangeUpdate_Multiple_ShouldCompose()
    {
        var st = new SegmentTree<int>(
            new[] { 5, 3, 7, 1, 4 },
            combine: Math.Min,
            identity: int.MaxValue,
            applyLazy: (_, v) => v,
            composeLazy: (_, newer) => newer);

        st.RangeUpdate(0, 5, 100);
        st.RangeUpdate(1, 3, 50);
        st.RangeUpdate(2, 4, 10);

        // Позиция 0: 100
        // Позиция 1: 50
        // Позиция 2: 10
        // Позиция 3: 10
        // Позиция 4: 100
        st.GetAt(0).Should().Be(100);
        st.GetAt(1).Should().Be(50);
        st.GetAt(2).Should().Be(10);
        st.GetAt(3).Should().Be(10);
        st.GetAt(4).Should().Be(100);
    }
}