using FluentAssertions;
using GraphToolkit.Core;
using GraphToolkit.MinimumSpanningTree;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.MinimumSpanningTree;

public class PrimAndBoruvkaTests
{
    private static Graph<string> CreateGraph() =>
        new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 4)
            .AddEdge("A", "C", 3)
            .AddEdge("B", "C", 1)
            .AddEdge("B", "D", 2)
            .AddEdge("C", "D", 5)
            .AddEdge("D", "E", 7)
            .Build();

    [Fact]
    public void Prim_ShouldReturnMstOfCorrectSize()
    {
        var g = CreateGraph();
        var mst = Prim.Compute(g, "A");

        mst.Should().HaveCount(4);
    }

    [Fact]
    public void Prim_ShouldMatchKruskalWeight()
    {
        var g = CreateGraph();
        var prim = Prim.Compute(g, "A");
        var kruskal = Kruskal.Compute(g);

        prim.Sum(e => e.Weight).Should().Be(kruskal.Sum(e => e.Weight));
    }

    [Fact]
    public void Boruvka_ShouldReturnMstOfCorrectSize()
    {
        var g = CreateGraph();
        var mst = Boruvka.Compute(g);

        mst.Should().HaveCount(4);
    }

    [Fact]
    public void Boruvka_ShouldMatchKruskalWeight()
    {
        var g = CreateGraph();
        var boruvka = Boruvka.Compute(g);
        var kruskal = Kruskal.Compute(g);

        boruvka.Sum(e => e.Weight).Should().Be(kruskal.Sum(e => e.Weight));
    }

    [Fact]
    public void AllThreeMSTAlgorithms_ShouldGiveSameTotalWeight()
    {
        var g = CreateGraph();
        double k = Kruskal.Compute(g).Sum(e => e.Weight);
        double p = Prim.Compute(g, "A").Sum(e => e.Weight);
        double b = Boruvka.Compute(g).Sum(e => e.Weight);

        k.Should().Be(p).And.Be(b);
    }
}