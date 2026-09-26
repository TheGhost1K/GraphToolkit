using FluentAssertions;
using GraphToolkit.Cut;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Cut;

public class GomoryHuTests
{
    [Fact]
    public void Compute_SimpleGraph_ShouldReturnTree()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 1)
            .AddEdge("B", "C", 2)
            .AddEdge("A", "C", 3)
            .Build();

        var result = GomoryHu.Compute(g);

        result.Edges.Should().NotBeEmpty();
        result.Edges.Should().HaveCount(2);   // V - 1
    }

    [Fact]
    public void MinCut_TwoLeaves_ShouldFindBottleneck()
    {
        // A - B - C, все рёбра веса 1.
        // Min cut между A и C = 1 (любое из рёбер)
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 1)
            .AddEdge("B", "C", 1)
            .Build();

        var result = GomoryHu.Compute(g);

        result.MinCut("A", "C").Should().Be(1);
    }

    [Fact]
    public void MinCut_ParallelPaths_ShouldSumWeight()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 1)
            .AddEdge("B", "C", 1)
            .AddEdge("A", "D", 1)
            .AddEdge("D", "C", 1)
            .Build();

        var result = GomoryHu.Compute(g);

        // Min cut между A и C = 2 (два независимых пути)
        result.MinCut("A", "C").Should().Be(2);
    }

    [Fact]
    public void MinCut_Triangle_ShouldFindMinEdge()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 5)
            .AddEdge("B", "C", 3)
            .AddEdge("A", "C", 1)
            .Build();

        var result = GomoryHu.Compute(g);

        // Разбиения для (A,B): {A}|{B,C} = 5+1 = 6, {A,B}|... нет C стороне...
        // Минимум = 6
        result.MinCut("A", "B").Should().Be(6);

        // Разбиения для (A,C): {A}|{B,C} = 5+1 = 6, {A,B}|{C} = 1+3 = 4
        result.MinCut("A", "C").Should().Be(4);

        // Разбиения для (B,C): {B}|{A,C} = 5+3 = 8, {B,A}|{C} = 3+1 = 4
        result.MinCut("B", "C").Should().Be(4);
    }

    [Fact]
    public void Compute_DirectedGraph_ShouldThrow()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(1, 2)
            .Build();

        var act = () => GomoryHu.Compute(g);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Compute_SingleVertex_ShouldReturnEmptyTree()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddVertex(1)
            .Build();

        var result = GomoryHu.Compute(g);

        result.Edges.Should().BeEmpty();
    }

    [Fact]
    public void Compute_DisconnectedGraph_ShouldReturnZeroForSeparateComponents()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2, 5)
            .AddVertex(3)
            .Build();

        var result = GomoryHu.Compute(g);

        result.MinCut(1, 3).Should().Be(0);
    }
}