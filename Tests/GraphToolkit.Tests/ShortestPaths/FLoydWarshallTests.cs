using FluentAssertions;
using GraphToolkit.ShortestPaths;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.ShortestPaths;

public class FloydWarshallTests
{
    [Fact]
    public void Compute_ShouldReturnAllPairsDistances()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 3)
            .AddEdge("B", "C", 4)
            .AddEdge("A", "C", 10)
            .Build();

        var (dist, _, vertices) = FloydWarshall.Compute(g);

        var idx = new Dictionary<string, int>();
        for (int i = 0; i < vertices.Count; i++)
            idx[vertices[i]] = i;

        dist[idx["A"], idx["B"]].Should().Be(3);
        dist[idx["A"], idx["C"]].Should().Be(7);
        dist[idx["B"], idx["C"]].Should().Be(4);
    }

    [Fact]
    public void FindPath_ShouldReturnShortestPath()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 3)
            .AddEdge("B", "C", 4)
            .AddEdge("A", "C", 10)
            .Build();

        var path = FloydWarshall.FindPath(g, "A", "C", out double distance);

        path.Should().Equal("A", "B", "C");
        distance.Should().Be(7);
    }

    [Fact]
    public void Compute_SelfDistance_ShouldBeZero()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(1, 2, 5)
            .Build();

        var (dist, _, vertices) = FloydWarshall.Compute(g);

        int i = -1;
        for (int k = 0; k < vertices.Count; k++)
            if (EqualityComparer<int>.Default.Equals(vertices[k], 1))
            {
                i = k;
                break;
            }

        i.Should().BeGreaterThanOrEqualTo(0);
        dist[i, i].Should().Be(0);
    }

    [Fact]
    public void Compute_UndirectedGraph_ShouldBeSymmetric()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 5)
            .AddEdge("B", "C", 3)
            .Build();

        var (dist, _, vertices) = FloydWarshall.Compute(g);

        var idx = new Dictionary<string, int>();
        for (int i = 0; i < vertices.Count; i++)
            idx[vertices[i]] = i;

        dist[idx["A"], idx["C"]].Should().Be(8);
        dist[idx["C"], idx["A"]].Should().Be(8);
    }
}