using FluentAssertions;
using GraphToolkit.Coloring;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Coloring;

public class GraphColoringTests
{
    [Fact]
    public void Greedy_Triangle_ShouldUse3Colors()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 0)
            .Build();

        var colors = GraphColoring.Greedy(g);

        colors.Values.Distinct().Should().HaveCount(3);
    }

    [Fact]
    public void Greedy_Bipartite_ShouldUse2Colors()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .Build();

        var colors = GraphColoring.Greedy(g);

        colors.Values.Distinct().Should().HaveCount(2);
    }

    [Fact]
    public void Exact_Triangle_ShouldReturn3()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 0)
            .Build();

        var (_, chi) = GraphColoring.Exact(g);

        chi.Should().Be(3);
    }

    [Fact]
    public void Exact_Bipartite_ShouldReturn2()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .AddEdge(3, 0)
            .Build();

        var (_, chi) = GraphColoring.Exact(g);

        chi.Should().Be(2);
    }

    [Fact]
    public void Exact_K4_ShouldReturn4()
    {
        var g = new GraphBuilder<int>(isDirected: false);
        for (int i = 0; i < 4; i++)
            for (int j = i + 1; j < 4; j++)
                g.AddEdge(i, j);

        var (_, chi) = GraphColoring.Exact(g.Build());

        chi.Should().Be(4);
    }

    [Fact]
    public void Exact_ShouldNotExceedGreedyResult()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .AddEdge(3, 0)
            .AddEdge(0, 2)
            .Build();

        var greedyColors = GraphColoring.Greedy(g);
        var (_, chi) = GraphColoring.Exact(g);

        chi.Should().BeLessThanOrEqualTo(greedyColors.Values.Max() + 1);
    }

    [Fact]
    public void IsBipartite_EvenCycle_ShouldReturnTrue()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .AddEdge(3, 0)
            .Build();

        GraphColoring.IsBipartite(g, out var coloring).Should().BeTrue();
        coloring.Values.Distinct().Should().HaveCount(2);
    }

    [Fact]
    public void IsBipartite_OddCycle_ShouldReturnFalse()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 0)
            .Build();

        GraphColoring.IsBipartite(g, out _).Should().BeFalse();
    }
}