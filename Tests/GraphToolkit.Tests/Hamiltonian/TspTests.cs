using FluentAssertions;
using GraphToolkit.Core;
using GraphToolkit.Hamiltonian;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Hamiltonian;

public class TspTests
{
    private static Graph<string> CreateTspGraph() =>
        new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 10)
            .AddEdge("A", "C", 15)
            .AddEdge("A", "D", 20)
            .AddEdge("B", "C", 35)
            .AddEdge("B", "D", 25)
            .AddEdge("C", "D", 30)
            .Build();

    [Fact]
    public void BranchAndBound_ShouldFindOptimalTour()
    {
        var g = CreateTspGraph();
        var result = Tsp.BranchAndBound(g, "A");

        result.Path.Should().NotBeNull();
        result.Path!.First().Should().Be("A");
        result.Path.Last().Should().Be("A");
        result.Path.Should().HaveCount(5); // A + 3 + A
        result.Cost.Should().Be(80); // 10 + 25 + 30 + 15 (A->B->D->C->A)
    }

    [Fact]
    public void NearestNeighbor_ShouldReturnValidTour()
    {
        var g = CreateTspGraph();
        var result = Tsp.NearestNeighbor(g, "A");

        result.Path.Should().NotBeNull();
        result.Path!.First().Should().Be("A");
        result.Path.Last().Should().Be("A");
        result.Path.Should().HaveCount(5);
        result.Cost.Should().BeGreaterThanOrEqualTo(80);
    }

    [Fact]
    public void NearestNeighbor_ShouldBeAtLeastAsGoodAsOptimal()
    {
        var g = CreateTspGraph();
        var exact = Tsp.BranchAndBound(g, "A");
        var approx = Tsp.NearestNeighbor(g, "A");

        // Приближённое решение не может быть лучше точного
        approx.Cost.Should().BeGreaterThanOrEqualTo(exact.Cost);
    }

    [Fact]
    public void BranchAndBound_TwoVertices_ShouldReturnThereAndBack()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 5)
            .Build();

        var result = Tsp.BranchAndBound(g, "A");

        result.Path.Should().Equal("A", "B", "A");
        result.Cost.Should().Be(10);
    }
}