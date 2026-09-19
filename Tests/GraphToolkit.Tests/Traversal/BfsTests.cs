using FluentAssertions;
using GraphToolkit.Core;
using GraphToolkit.Traversal;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Traversal;

public class BfsTests
{
    private static Graph<string> CreateTestGraph()
    {
        return new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B")
            .AddEdge("A", "C")
            .AddEdge("B", "D")
            .AddEdge("C", "D")
            .AddEdge("D", "E")
            .AddEdge("E", "F")
            .Build();
    }

    [Fact]
    public void FindPath_DirectEdge_ShouldReturnTwoVertices()
    {
        var g = CreateTestGraph();
        var path = Bfs.FindPath(g, "A", "B");

        path.Should().Equal("A", "B");
    }

    [Fact]
    public void FindPath_ShouldReturnShortestByEdges()
    {
        var g = CreateTestGraph();
        var path = Bfs.FindPath(g, "A", "D");

        // Через B или C — оба по 2 ребра
        path.Should().HaveCount(3);
        path![0].Should().Be("A");
        path[^1].Should().Be("D");
    }

    [Fact]
    public void FindPath_Unreachable_ShouldReturnNull()
    {
        var g = new Graph<string>(isDirected: true);
        g.AddEdge("A", "B");
        g.AddVertex("Z");

        Bfs.FindPath(g, "A", "Z").Should().BeNull();
    }

    [Fact]
    public void FindPath_SameVertex_ShouldReturnSingleElement()
    {
        var g = CreateTestGraph();
        var path = Bfs.FindPath(g, "A", "A");
        path.Should().Equal("A");
    }

    [Fact]
    public void FindPath_LongerPath_ShouldFindIt()
    {
        var g = CreateTestGraph();
        var path = Bfs.FindPath(g, "A", "F");

        path.Should().NotBeNull();
        path!.First().Should().Be("A");
        path.Last().Should().Be("F");
    }

    [Fact]
    public void FindPath_Undirected_ShouldWork()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B")
            .AddEdge("B", "C")
            .Build();

        Bfs.FindPath(g, "C", "A").Should().Equal("C", "B", "A");
    }
}