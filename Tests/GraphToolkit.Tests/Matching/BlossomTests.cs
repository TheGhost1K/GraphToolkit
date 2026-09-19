using FluentAssertions;
using GraphToolkit.Matching;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Matching;

public class BlossomTests
{
    [Fact]
    public void Compute_SimplePath_ShouldMatchAll()
    {
        // Путь 0-1-2-3-4: паросочетание {(0,1),(2,3)}
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .AddEdge(3, 4)
            .Build();

        var matching = Blossom.Compute(g);

        matching.Should().HaveCount(2);
    }

    [Fact]
    public void Compute_Triangle_ShouldMatchOnePair()
    {
        // Треугольник: нечётное число вершин, максимальное паросочетание = 1
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 0)
            .Build();

        var matching = Blossom.Compute(g);

        matching.Should().HaveCount(1);
    }

    [Fact]
    public void Compute_PetersenGraphLike_ShouldFindLargeMatching()
    {
        // Граф с нечётными циклами (не двудольный)
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 0)  // треугольник
            .AddEdge(2, 3)
            .AddEdge(3, 4)
            .AddEdge(4, 5)
            .AddEdge(5, 3)  // ещё треугольник
            .Build();

        var matching = Blossom.Compute(g);

        matching.Should().HaveCount(3); // идеальное
    }

    [Fact]
    public void HasPerfectMatching_EvenVertices_ShouldReturnTrue()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .Build();

        Blossom.HasPerfectMatching(g).Should().BeTrue();
    }

    [Fact]
    public void HasPerfectMatching_OddVertices_ShouldReturnFalse()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .Build();

        Blossom.HasPerfectMatching(g).Should().BeFalse();
    }
}