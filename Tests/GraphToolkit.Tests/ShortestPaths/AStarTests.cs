using FluentAssertions;
using GraphToolkit.ShortestPaths;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.ShortestPaths;

public class AStarTests
{
    [Fact]
    public void FindPath_ZeroHeuristic_ShouldBehaveLikeDijkstra()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 1)
            .AddEdge("B", "C", 1)
            .AddEdge("A", "C", 3)
            .Build();

        var path = AStar.FindPath(g, "A", "C", (_, _) => 0);

        path.Should().Equal("A", "B", "C");
    }

    [Fact]
    public void FindPath_AdmissibleHeuristic_ShouldFindOptimal()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 1)
            .AddEdge("B", "C", 1)
            .AddEdge("A", "C", 5)
            .Build();

        // Допустимая эвристика — недооценивает
        var path = AStar.FindPath(g, "A", "C", (_, _) => 0.5);

        path.Should().Equal("A", "B", "C");
    }

    [Fact]
    public void FindPath_Unreachable_ShouldReturnNull()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1)
            .AddVertex(99)
            .Build();

        AStar.FindPath(g, 0, 99, (_, _) => 0).Should().BeNull();
    }
}