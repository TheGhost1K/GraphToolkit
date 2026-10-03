using FluentAssertions;
using GraphToolkit.Parallel;
using GraphToolkit.Traversal;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Parallel;

public class ParallelBfsTests
{
    [Fact]
    public void FindPath_Simple_ReturnsValidShortestPath()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B").AddEdge("A", "C")
            .AddEdge("B", "D").AddEdge("C", "D")
            .AddEdge("D", "E")
            .Build();

        var path = ParallelBfs.FindPath(g, "A", "E");

        path.Should().NotBeNull();
        path!.First().Should().Be("A");
        path.Last().Should().Be("E");

        // Проверяем, что все рёбра пути существуют
        for (int i = 0; i < path.Count - 1; i++)
        {
            g.Neighbors(path[i])
                .Should().Contain(e => e.To == path[i + 1],
                    $"ребро {path[i]} → {path[i + 1]} должно существовать");
        }

        // Кратчайший путь от A до E = 3 ребра (4 вершины)
        path.Should().HaveCount(4);
    }

    [Fact]
    public void FindPath_Unreachable_ReturnsNull()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B")
            .AddVertex("Z")
            .Build();

        ParallelBfs.FindPath(g, "A", "Z").Should().BeNull();
    }

    [Fact]
    public void FindPath_SameVertex_ReturnsSingleElement()
    {
        var g = new GraphBuilder<int>(isDirected: true).AddEdge(1, 2).Build();

        ParallelBfs.FindPath(g, 1, 1).Should().Equal(1);
    }

    [Fact]
    public void Distances_ChainGraph()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1).AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 4)
            .Build();

        var d = ParallelBfs.Distances(g, 0);

        d[0].Should().Be(0);
        d[1].Should().Be(1);
        d[2].Should().Be(2);
        d[3].Should().Be(3);
        d[4].Should().Be(4);
    }

    [Fact]
    public void FindPath_LargeGraph_DoesNotDegrade()
    {
        var builder = new GraphBuilder<int>(isDirected: true);
        for (int i = 0; i < 10_000; i++)
            builder.AddEdge(i, i + 1);

        var g = builder.Build();
        var path = ParallelBfs.FindPath(g, 0, 10_000);

        path.Should().NotBeNull();
        path!.Count.Should().Be(10_001);
    }
}
