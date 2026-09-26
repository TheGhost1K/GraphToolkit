using FluentAssertions;
using GraphToolkit.Core;
using GraphToolkit.ShortestPaths;
using Xunit;

namespace GraphToolkit.Tests.Core;

public class AdjacencyMatrixGraphTests
{
    [Fact]
    public void AddEdge_ShouldIncreaseEdgeCount()
    {
        var g = new AdjacencyMatrixGraph<string>(isDirected: true);
        g.AddEdge("A", "B", 5);
        g.AddEdge("B", "C", 3);

        g.VertexCount.Should().Be(3);
        g.EdgeCount.Should().Be(2);
    }

    [Fact]
    public void AddEdge_SameEdgeTwice_ShouldNotDuplicateCount()
    {
        var g = new AdjacencyMatrixGraph<string>(isDirected: true);
        g.AddEdge("A", "B", 5);
        g.AddEdge("A", "B", 7);   // перезапись

        g.EdgeCount.Should().Be(1);
        g.GetWeight("A", "B").Should().Be(7);
    }

    [Fact]
    public void Undirected_AddEdge_ShouldReflectBothDirections()
    {
        var g = new AdjacencyMatrixGraph<int>(isDirected: false);
        g.AddEdge(1, 2, 5);

        g.GetWeight(1, 2).Should().Be(5);
        g.GetWeight(2, 1).Should().Be(5);
        g.EdgeCount.Should().Be(1);   // считаем одно ребро
    }

    [Fact]
    public void HasEdge_NonexistentEdge_ShouldReturnFalse()
    {
        var g = new AdjacencyMatrixGraph<string>(isDirected: true);
        g.AddEdge("A", "B");

        g.HasEdge("A", "B").Should().BeTrue();
        g.HasEdge("B", "A").Should().BeFalse();  // directed — обратного ребра нет
        g.HasEdge("A", "Z").Should().BeFalse();
        g.HasEdge("Z", "A").Should().BeFalse();
    }

    [Fact]
    public void HasEdge_UndirectedGraph_ReciprocalIsTrue()
    {
        var g = new AdjacencyMatrixGraph<string>(isDirected: false);
        g.AddEdge("A", "B");

        g.HasEdge("A", "B").Should().BeTrue();
        g.HasEdge("B", "A").Should().BeTrue();   // неориентированный — симметрия
    }

    [Fact]
    public void HasEdge_DirectedGraph_ReciprocalIsFalse()
    {
        var g = new AdjacencyMatrixGraph<string>(isDirected: true);
        g.AddEdge("A", "B");

        g.HasEdge("A", "B").Should().BeTrue();
        g.HasEdge("B", "A").Should().BeFalse();  // направленное — нет обратного
    }

    [Fact]
    public void HasEdge_UnknownVertex_ShouldReturnFalse()
    {
        var g = new AdjacencyMatrixGraph<string>(isDirected: true);
        g.AddEdge("A", "B");

        g.HasEdge("A", "Z").Should().BeFalse();
        g.HasEdge("Z", "A").Should().BeFalse();
        g.HasEdge("Z", "Y").Should().BeFalse();
    }

    [Fact]
    public void HasVertex_ShouldWork()
    {
        var g = new AdjacencyMatrixGraph<string>();
        g.AddEdge("A", "B");

        g.HasVertex("A").Should().BeTrue();
        g.HasVertex("Z").Should().BeFalse();
    }

    [Fact]
    public void RemoveEdge_ShouldDecreaseCount()
    {
        var g = new AdjacencyMatrixGraph<int>();
        g.AddEdge(1, 2);
        g.AddEdge(2, 3);
        g.RemoveEdge(1, 2);

        g.EdgeCount.Should().Be(1);
        g.HasEdge(1, 2).Should().BeFalse();
    }

    [Fact]
    public void Grow_ShouldPreserveData()
    {
        var g = new AdjacencyMatrixGraph<int>(initialCapacity: 4);
        for (int i = 0; i < 20; i++)
            g.AddEdge(i, i + 1, i + 1);

        g.VertexCount.Should().Be(21);
        g.EdgeCount.Should().Be(20);
        g.GetWeight(0, 1).Should().Be(1);
        g.GetWeight(19, 20).Should().Be(20);
    }

    [Fact]
    public void Neighbors_ShouldReturnCorrectEdges()
    {
        var g = new AdjacencyMatrixGraph<string>(isDirected: true);
        g.AddEdge("A", "B", 1);
        g.AddEdge("A", "C", 2);

        var neighbors = g.Neighbors("A").ToList();

        neighbors.Should().HaveCount(2);
        neighbors.Should().Contain(e => e.To == "B" && e.Weight == 1);
        neighbors.Should().Contain(e => e.To == "C" && e.Weight == 2);
    }

    [Fact]
    public void Dijkstra_OnMatrixGraph_ShouldWork()
    {
        var g = new AdjacencyMatrixGraph<string>(isDirected: true);
        g.AddEdge("A", "B", 4);
        g.AddEdge("A", "C", 2);
        g.AddEdge("C", "B", 1);
        g.AddEdge("B", "D", 5);

        var path = Dijkstra.FindPath(g, "A", "D");

        path.Should().Equal("A", "C", "B", "D");
    }

    [Fact]
    public void Edges_DirectedGraph_ShouldReturnAllEdges()
    {
        var g = new AdjacencyMatrixGraph<string>(isDirected: true);
        g.AddEdge("A", "B");
        g.AddEdge("B", "A");    // в ориентированном это два разных ребра

        g.Edges.Should().HaveCount(2);
    }

    [Fact]
    public void Edges_UndirectedGraph_ShouldNotDuplicate()
    {
        var g = new AdjacencyMatrixGraph<string>(isDirected: false);
        g.AddEdge("A", "B");

        g.Edges.Should().HaveCount(1);
    }
}