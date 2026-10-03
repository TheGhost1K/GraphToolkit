using FluentAssertions;
using GraphToolkit.Centrality;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Centrality;

public class PageRankTests
{
    [Fact]
    public void Compute_SimpleGraph_SumToOne()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B")
            .AddEdge("B", "C")
            .AddEdge("C", "A")
            .Build();

        var ranks = PageRank.Compute(g);

        ranks.Values.Sum().Should().BeApproximately(1.0, 1e-6);
    }

    [Fact]
    public void Compute_Hub_ShouldHaveHigherRank()
    {
        // A, B, C → D (D — хаб, на который все ссылаются)
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "D")
            .AddEdge("B", "D")
            .AddEdge("C", "D")
            .AddEdge("D", "A")
            .Build();

        var ranks = PageRank.Compute(g);

        ranks["D"].Should().BeGreaterThan(ranks["A"]);
        ranks["D"].Should().BeGreaterThan(ranks["B"]);
        ranks["D"].Should().BeGreaterThan(ranks["C"]);
    }

    [Fact]
    public void Compute_EmptyGraph_ReturnsEmpty()
    {
        var g = new GraphBuilder<string>(isDirected: true).Build();
        PageRank.Compute(g).Should().BeEmpty();
    }

    [Fact]
    public void Compute_InvalidDamping_Throws()
    {
        var g = new GraphBuilder<int>(isDirected: true).AddEdge(1, 2).Build();

        var act = () => PageRank.Compute(g, damping: 1.5);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Compute_SymmetricGraph_AllEqual()
    {
        // A ↔ B
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B")
            .AddEdge("B", "A")
            .Build();

        var ranks = PageRank.Compute(g);

        ranks["A"].Should().BeApproximately(ranks["B"], 1e-9);
    }
}
