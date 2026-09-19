using FluentAssertions;
using GraphToolkit.Cut;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Cut;

public class StoerWagnerTests
{
    [Fact]
    public void Compute_SimpleGraph_ShouldFindMinCut()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 1)
            .AddEdge("B", "C", 2)
            .AddEdge("A", "C", 3)
            .AddEdge("C", "D", 4)
            .Build();

        var result = StoerWagner.Compute(g);

        // Минимальный разрез — ребро AB (вес 1) или изолировать D
        result.MinCut.Should().BeGreaterThan(0);
        result.PartitionA.Should().NotBeEmpty();
        result.PartitionB.Should().NotBeEmpty();
    }

    [Fact]
    public void Compute_DisconnectedGraph_ShouldReturnZero()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2, 5)
            .AddVertex(3)
            .Build();

        var result = StoerWagner.Compute(g);

        result.MinCut.Should().Be(0);
    }

    [Fact]
    public void Compute_TwoClusters_ShouldFindBottleneck()
    {
        // Два треугольника, соединённых одним ребром веса 1
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2, 10)
            .AddEdge(2, 3, 10)
            .AddEdge(3, 1, 10)
            .AddEdge(3, 4, 1)   // мост
            .AddEdge(4, 5, 10)
            .AddEdge(5, 6, 10)
            .AddEdge(6, 4, 10)
            .Build();

        var result = StoerWagner.Compute(g);

        result.MinCut.Should().Be(1);
    }
}