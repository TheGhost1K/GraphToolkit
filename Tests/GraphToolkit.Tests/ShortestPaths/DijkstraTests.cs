using FluentAssertions;
using GraphToolkit.Core;
using GraphToolkit.ShortestPaths;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.ShortestPaths;

public class DijkstraTests
{
    private static Graph<string> CreateGraph()
    {
        return new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 4)
            .AddEdge("A", "C", 2)
            .AddEdge("B", "C", 5)
            .AddEdge("B", "D", 10)
            .AddEdge("C", "E", 3)
            .AddEdge("E", "D", 4)
            .Build();
    }

    [Fact]
    public void Compute_ShouldReturnCorrectDistances()
    {
        var g = CreateGraph();
        var (dist, _) = Dijkstra.Compute(g, "A");

        dist["A"].Should().Be(0);
        dist["B"].Should().Be(4);
        dist["C"].Should().Be(2);
        dist["E"].Should().Be(5);
        dist["D"].Should().Be(9);
    }

    [Fact]
    public void FindPath_ShouldReturnShortestPath()
    {
        var g = CreateGraph();
        var path = Dijkstra.FindPath(g, "A", "D");

        path.Should().Equal("A", "C", "E", "D");
    }

    [Fact]
    public void FindPath_Unreachable_ShouldReturnNull()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B")
            .AddVertex("Z")
            .Build();

        Dijkstra.FindPath(g, "A", "Z").Should().BeNull();
    }

    [Fact]
    public void FindPath_DirectEdge_ShouldReturnDirectPath()
    {
        var g = CreateGraph();
        var path = Dijkstra.FindPath(g, "A", "B");

        path.Should().Equal("A", "B");
    }

    [Fact]
    public void Compute_SameVertex_ShouldBeZero()
    {
        var g = CreateGraph();
        var (dist, _) = Dijkstra.Compute(g, "A");

        dist["A"].Should().Be(0);
    }

    [Fact]
    public void Compute_Undirected_ShouldWork()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 1)
            .AddEdge("B", "C", 2)
            .Build();

        var (dist, _) = Dijkstra.Compute(g, "A");

        dist["C"].Should().Be(3);
    }
}