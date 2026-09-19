using FluentAssertions;
using GraphToolkit.Core;
using GraphToolkit.Traversal;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Traversal;

public class DfsTests
{
    [Fact]
    public void FindPath_SimpleChain_ShouldFindIt()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B")
            .AddEdge("B", "C")
            .AddEdge("C", "D")
            .Build();

        var path = Dfs.FindPath(g, "A", "D");

        path.Should().Equal("A", "B", "C", "D");
    }

    [Fact]
    public void FindPath_Unreachable_ShouldReturnNull()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B")
            .AddVertex("Z")
            .Build();

        Dfs.FindPath(g, "A", "Z").Should().BeNull();
    }

    [Fact]
    public void FindPath_SameVertex_ShouldReturnSingleElement()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(1, 2)
            .Build();

        Dfs.FindPath(g, 1, 1).Should().Equal(1);
    }

    [Fact]
    public void FindPath_LargeChain_ShouldNotStackOverflow()
    {
        // Проверяет, что итеративная реализация не падает
        var g = new Graph<int>(isDirected: true);
        for (int i = 0; i < 100_000; i++) g.AddEdge(i, i + 1);

        var path = Dfs.FindPath(g, 0, 100_000);

        path.Should().NotBeNull();
        path!.Count.Should().Be(100_001);
    }
}