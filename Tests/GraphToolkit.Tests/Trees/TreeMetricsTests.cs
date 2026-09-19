using FluentAssertions;
using GraphToolkit.Trees;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Trees;

public class TreeMetricsTests
{
    [Fact]
    public void Diameter_SimpleChain_ShouldBeWholeChain()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1, 1)
            .AddEdge(1, 2, 1)
            .AddEdge(2, 3, 1)
            .AddEdge(3, 4, 1)
            .Build();

        var result = TreeMetrics.Diameter(g);

        result.Distance.Should().Be(4);
        result.Path.Should().HaveCount(5);
    }

    [Fact]
    public void Diameter_StarGraph_ShouldBeTwo()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1, 1)
            .AddEdge(0, 2, 1)
            .AddEdge(0, 3, 1)
            .Build();

        var result = TreeMetrics.Diameter(g);

        result.Distance.Should().Be(2);
    }

    [Fact]
    public void Centroid_SimplePath_ShouldBeMiddle()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .AddEdge(3, 4)
            .Build();

        var centroid = TreeMetrics.Centroid(g);

        // Центроид пути 0-1-2-3-4 — вершина 2 (или 1/3 при чётном)
        centroid.Should().BeOneOf(1, 2, 3);
    }

    [Fact]
    public void Centroid_Star_ShouldBeCenter()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(0, 2)
            .AddEdge(0, 3)
            .AddEdge(0, 4)
            .Build();

        var centroid = TreeMetrics.Centroid(g);

        centroid.Should().Be(0);
    }
}