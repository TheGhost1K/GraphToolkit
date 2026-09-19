using FluentAssertions;
using GraphToolkit.Core;
using Xunit;

namespace GraphToolkit.Tests.Core;

public class GraphTests
{
    [Fact]
    public void AddVertex_ShouldIncreaseVertexCount()
    {
        var g = new Graph<string>();
        g.AddVertex("A");
        g.AddVertex("B");

        g.VertexCount.Should().Be(2);
    }

    [Fact]
    public void AddVertex_ShouldNotDuplicate()
    {
        var g = new Graph<string>();
        g.AddVertex("A");
        g.AddVertex("A");

        g.VertexCount.Should().Be(1);
    }

    [Fact]
    public void AddEdge_Directed_ShouldAddOneEdge()
    {
        var g = new Graph<string>(isDirected: true);
        g.AddEdge("A", "B", 5);

        g.VertexCount.Should().Be(2);
        g.EdgeCount.Should().Be(1);
        g.Neighbors("A").Should().ContainSingle(e => e.To == "B" && e.Weight == 5);
        g.Neighbors("B").Should().BeEmpty();
    }

    [Fact]
    public void AddEdge_Undirected_ShouldAddBothDirections()
    {
        var g = new Graph<string>(isDirected: false);
        g.AddEdge("A", "B", 5);

        g.VertexCount.Should().Be(2);
        g.EdgeCount.Should().Be(1); // в Edges хранится 1 (прямое)
        g.Neighbors("A").Should().ContainSingle(e => e.To == "B");
        g.Neighbors("B").Should().ContainSingle(e => e.To == "A");
    }

    [Fact]
    public void RemoveEdge_Directed_ShouldRemoveOnlyOneDirection()
    {
        var g = new Graph<string>(isDirected: true);
        g.AddEdge("A", "B");
        g.AddEdge("B", "A");
        g.RemoveEdge("A", "B");

        g.Neighbors("A").Should().BeEmpty();
        g.Neighbors("B").Should().ContainSingle(e => e.To == "A");
    }

    [Fact]
    public void RemoveEdge_Undirected_ShouldRemoveBothDirections()
    {
        var g = new Graph<string>(isDirected: false);
        g.AddEdge("A", "B");
        g.RemoveEdge("A", "B");

        g.EdgeCount.Should().Be(0);
        g.Neighbors("A").Should().BeEmpty();
        g.Neighbors("B").Should().BeEmpty();
    }

    [Fact]
    public void Transpose_ShouldReverseAllEdges()
    {
        var g = new Graph<string>(isDirected: true);
        g.AddEdge("A", "B", 3);
        g.AddEdge("B", "C", 4);

        var t = g.Transpose();

        t.Neighbors("A").Should().BeEmpty();
        t.Neighbors("B").Should().ContainSingle(e => e.To == "A" && e.Weight == 3);
        t.Neighbors("C").Should().ContainSingle(e => e.To == "B" && e.Weight == 4);
    }

    [Fact]
    public void Neighbors_NonexistentVertex_ShouldReturnEmpty()
    {
        var g = new Graph<string>();
        g.Neighbors("missing").Should().BeEmpty();
    }
}