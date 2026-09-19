using FluentAssertions;
using GraphToolkit.Traversal;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Traversal;

public class TopologicalSortTests
{
    [Fact]
    public void Sort_Dag_ShouldReturnValidOrder()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1)
            .AddEdge(0, 2)
            .AddEdge(1, 3)
            .AddEdge(2, 3)
            .Build();

        var order = TopologicalSort.Sort(g);

        order.Should().NotBeNull();
        order!.Should().HaveCount(4);

        // Все рёбра удовлетворяют порядку
        foreach (var e in g.Edges)
            order.IndexOf(e.From).Should().BeLessThan(order.IndexOf(e.To));
    }

    [Fact]
    public void Sort_WithCycle_ShouldReturnNull()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 0)
            .Build();

        TopologicalSort.Sort(g).Should().BeNull();
    }

    [Fact]
    public void Sort_EmptyGraph_ShouldReturnEmptyList()
    {
        var g = new GraphBuilder<int>(isDirected: true).Build();
        TopologicalSort.Sort(g).Should().BeEmpty();
    }

    [Fact]
    public void ComputeLevels_ShouldAssignCorrectLevels()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1)
            .AddEdge(0, 2)
            .AddEdge(1, 3)
            .AddEdge(2, 3)
            .AddEdge(3, 4)
            .Build();

        var levels = TopologicalSort.ComputeLevels(g);

        levels[0].Should().Be(0);
        levels[1].Should().Be(1);
        levels[2].Should().Be(1);
        levels[3].Should().Be(2);
        levels[4].Should().Be(3);
    }

    [Fact]
    public void ComputeLevels_WithCycle_ShouldThrow()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1)
            .AddEdge(1, 0)
            .Build();

        var act = () => TopologicalSort.ComputeLevels(g);
        act.Should().Throw<InvalidOperationException>();
    }
}