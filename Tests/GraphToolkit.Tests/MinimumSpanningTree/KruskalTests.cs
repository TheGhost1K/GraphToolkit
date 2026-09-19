using FluentAssertions;
using GraphToolkit.MinimumSpanningTree;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.MinimumSpanningTree;

public class KruskalTests
{
    [Fact]
    public void Compute_ShouldReturnMstWithVMinusOneEdges()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 4)
            .AddEdge("A", "C", 3)
            .AddEdge("B", "C", 1)
            .AddEdge("B", "D", 2)
            .AddEdge("C", "D", 5)
            .AddEdge("D", "E", 7)
            .Build();

        var mst = Kruskal.Compute(g);

        mst.Should().HaveCount(4); // V - 1 = 5 - 1
    }

    [Fact]
    public void Compute_ShouldMinimizeTotalWeight()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 4)
            .AddEdge("A", "C", 3)
            .AddEdge("B", "C", 1)
            .AddEdge("B", "D", 2)
            .AddEdge("C", "D", 5)
            .AddEdge("D", "E", 7)
            .Build();

        var mst = Kruskal.Compute(g);
        double total = mst.Sum(e => e.Weight);

        // Оптимум: BC(1) + BD(2) + AC(3) + DE(7) = 13
        total.Should().Be(13);
    }

    [Fact]
    public void Compute_SingleVertex_ShouldReturnEmpty()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddVertex(1)
            .Build();

        Kruskal.Compute(g).Should().BeEmpty();
    }

    [Fact]
    public void Compute_SingleEdge_ShouldReturnIt()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2, 5)
            .Build();

        var mst = Kruskal.Compute(g);

        mst.Should().HaveCount(1);
        mst[0].Weight.Should().Be(5);
    }
}