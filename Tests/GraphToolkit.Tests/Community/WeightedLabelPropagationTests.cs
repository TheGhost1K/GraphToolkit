using FluentAssertions;
using GraphToolkit.Community;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Community;

public class WeightedLabelPropagationTests
{
    [Fact]
    public void Compute_TwoCliques_FindsTwoCommunities()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2, 5).AddEdge(2, 3, 5).AddEdge(3, 1, 5)
            .AddEdge(4, 5, 5).AddEdge(5, 6, 5).AddEdge(6, 4, 5)
            .AddEdge(3, 4, 0.1)
            .Build();

        var communities = WeightedLabelPropagation.Compute(g, seed: 42);

        communities.Should().HaveCount(2);
    }

    [Fact]
    public void Compute_EmptyGraph_ReturnsEmpty()
    {
        var g = new GraphBuilder<int>(isDirected: false).Build();
        WeightedLabelPropagation.Compute(g).Should().BeEmpty();
    }

    [Fact]
    public void Compute_ReproducibleWithSeed()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2, 1).AddEdge(2, 3, 1).AddEdge(3, 1, 1)
            .Build();

        var c1 = WeightedLabelPropagation.Compute(g, seed: 42);
        var c2 = WeightedLabelPropagation.Compute(g, seed: 42);

        c1.Count.Should().Be(c2.Count);
    }
}
