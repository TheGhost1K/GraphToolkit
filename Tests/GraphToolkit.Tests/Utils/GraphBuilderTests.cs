using FluentAssertions;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Utils;

public class GraphBuilderTests
{
    [Fact]
    public void Build_WithEdges_ShouldCreateCorrectGraph()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 3)
            .AddEdge("B", "C", 5)
            .Build();

        g.VertexCount.Should().Be(3);
        g.EdgeCount.Should().Be(2);
        g.IsDirected.Should().BeTrue();
    }

    [Fact]
    public void Build_WithAddEdges_ShouldCreateAll()
    {
        var edges = new[]
        {
            ("A", "B", 1.0),
            ("B", "C", 2.0),
            ("C", "D", 3.0)
        };

        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdges(edges)
            .Build();

        g.EdgeCount.Should().Be(3);
    }

    [Fact]
    public void AddVertex_ShouldNotAddEdge()
    {
        var g = new GraphBuilder<int>()
            .AddVertex(1)
            .AddVertex(2)
            .Build();

        g.VertexCount.Should().Be(2);
        g.EdgeCount.Should().Be(0);
    }

    [Fact]
    public void Build_Empty_ShouldReturnEmptyGraph()
    {
        var g = new GraphBuilder<int>().Build();

        g.VertexCount.Should().Be(0);
        g.EdgeCount.Should().Be(0);
    }
}