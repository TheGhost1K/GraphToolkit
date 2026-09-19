using FluentAssertions;
using GraphToolkit.ShortestPaths;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.ShortestPaths;

public class BellmanFordTests
{
    [Fact]
    public void Compute_PositiveWeights_ShouldMatchDijkstra()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 4)
            .AddEdge("A", "C", 2)
            .AddEdge("B", "C", 5)
            .AddEdge("C", "D", 3)
            .Build();

        var (dist, _, hasCycle) = BellmanFord.Compute(g, "A");

        hasCycle.Should().BeFalse();
        dist["A"].Should().Be(0);
        dist["B"].Should().Be(4);
        dist["C"].Should().Be(2);
        dist["D"].Should().Be(5);
    }

    [Fact]
    public void Compute_NegativeEdges_ShouldWork()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 4)
            .AddEdge("A", "C", 2)
            .AddEdge("B", "C", -3)
            .AddEdge("C", "D", 1)
            .Build();

        var (dist, _, _) = BellmanFord.Compute(g, "A");

        dist["C"].Should().Be(1); // A -> B -> C: 4 + (-3) = 1
        dist["D"].Should().Be(2);
    }

    [Fact]
    public void Compute_NegativeCycle_ShouldDetect()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1, 1)
            .AddEdge(1, 2, -1)
            .AddEdge(2, 0, -1)
            .Build();

        var (_, _, hasCycle) = BellmanFord.Compute(g, 0);

        hasCycle.Should().BeTrue();
    }

    [Fact]
    public void Compute_Unreachable_ShouldBeInfinity()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B")
            .AddVertex("Z")
            .Build();

        var (dist, _, _) = BellmanFord.Compute(g, "A");

        dist["Z"].Should().Be(double.PositiveInfinity);
    }
}