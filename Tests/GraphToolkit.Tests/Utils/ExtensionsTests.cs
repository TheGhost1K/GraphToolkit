using FluentAssertions;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Utils;

public class ExtensionsTests
{
    [Fact]
    public void Degree_ShouldCountOutgoingEdges()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1)
            .AddEdge(0, 2)
            .AddEdge(0, 3)
            .Build();

        g.Degree(0).Should().Be(3);
        g.Degree(1).Should().Be(0);
    }

    [Fact]
    public void InDegree_ShouldCountIncomingEdges()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 3)
            .AddEdge(1, 3)
            .AddEdge(2, 3)
            .Build();

        g.InDegree(3).Should().Be(3);
    }

    [Fact]
    public void ContainsVertex_Existing_ShouldReturnTrue()
    {
        var g = new GraphBuilder<string>()
            .AddVertex("A")
            .Build();

        g.ContainsVertex("A").Should().BeTrue();
        g.ContainsVertex("B").Should().BeFalse();
    }

    [Fact]
    public void ContainsEdge_ShouldCheckAdjacency()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(1, 2)
            .Build();

        g.ContainsEdge(1, 2).Should().BeTrue();
        g.ContainsEdge(2, 1).Should().BeFalse();
    }

    [Fact]
    public void IsTree_SimpleChain_ShouldReturnTrue()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .Build();

        g.IsTree().Should().BeTrue();
    }

    [Fact]
    public void IsTree_Cycle_ShouldReturnFalse()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 0)
            .Build();

        g.IsTree().Should().BeFalse();
    }

    [Fact]
    public void IsTree_Disconnected_ShouldReturnFalse()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddVertex(2)
            .Build();

        g.IsTree().Should().BeFalse();
    }

    [Fact]
    public void OrderByDegreeDescending_ShouldSortCorrectly()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1)
            .AddEdge(0, 2)
            .AddEdge(0, 3)
            .AddEdge(1, 2)
            .Build();

        var ordered = g.OrderByDegreeDescending().ToList();

        ordered[0].Should().Be(0); // степень 3
    }
}