using FluentAssertions;
using GraphToolkit.Trees;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Trees;

public class CentroidDecompositionTests
{
    [Fact]
    public void Constructor_EmptyGraph_ShouldThrow()
    {
        var g = new GraphBuilder<int>().Build();
        var act = () => new CentroidDecomposition<int>(g);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_NonTree_ShouldThrow()
    {
        // Цикл — не дерево
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .AddEdge(3, 1)
            .Build();

        var act = () => new CentroidDecomposition<int>(g);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Root_ShouldBeCentroidOfTree()
    {
        // Путь 1-2-3-4-5, центроид = 3
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 4).AddEdge(4, 5)
            .Build();

        var cd = new CentroidDecomposition<int>(g);

        cd.Root.Should().Be(3);
    }

    [Fact]
    public void Depth_ShouldBeLogarithmic()
    {
        // Путь на 1000 вершин — глубина дерева центроидов должна быть ~log2(1000) ≈ 10
        var g = new GraphBuilder<int>(isDirected: false).Build();
        var builder = new GraphBuilder<int>(isDirected: false);
        for (int i = 1; i < 1000; i++)
            builder.AddEdge(i, i + 1);

        var cd = new CentroidDecomposition<int>(builder.Build());

        // Глубина не должна превышать 2*log2(V)
        int maxDepth = cd.Traverse().Max(cd.Depth);
        maxDepth.Should().BeLessThan(20);
    }

    [Fact]
    public void Distance_SimplePath_ShouldBeCorrect()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2, 1)
            .AddEdge(2, 3, 1)
            .AddEdge(3, 4, 1)
            .Build();

        var cd = new CentroidDecomposition<int>(g);

        cd.Distance(1, 4).Should().Be(3);
        cd.Distance(1, 2).Should().Be(1);
        cd.Distance(2, 3).Should().Be(1);
        cd.Distance(1, 1).Should().Be(0);
    }

    [Fact]
    public void Distance_WeightedTree_ShouldBeCorrect()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 5)
            .AddEdge("B", "C", 3)
            .AddEdge("C", "D", 2)
            .Build();

        var cd = new CentroidDecomposition<string>(g);

        cd.Distance("A", "D").Should().Be(10);
        cd.Distance("B", "D").Should().Be(5);
    }

    [Fact]
    public void Distance_BalancedTree_ShouldBeCorrect()
    {
        //       1
        //      / \
        //     2   3
        //    / \
        //   4   5
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2, 1).AddEdge(1, 3, 1)
            .AddEdge(2, 4, 1).AddEdge(2, 5, 1)
            .Build();

        var cd = new CentroidDecomposition<int>(g);

        cd.Distance(4, 5).Should().Be(2);
        cd.Distance(4, 3).Should().Be(3);
        cd.Distance(1, 5).Should().Be(2);
    }

    [Fact]
    public void Parent_OfRoot_ShouldBeDefault()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(2, 3)
            .Build();

        var cd = new CentroidDecomposition<int>(g);

        (cd.Parent(cd.Root) is 0).Should().BeTrue();
    }

    [Fact]
    public void Traverse_ShouldVisitAllVertices()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 4)
            .AddEdge(4, 5).AddEdge(5, 6).AddEdge(6, 7)
            .Build();

        var cd = new CentroidDecomposition<int>(g);

        cd.Traverse().Should().HaveCount(7);
        cd.Traverse().Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Children_OfRoot_ShouldCoverSubcomponents()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 4)
            .Build();

        var cd = new CentroidDecomposition<int>(g);

        var children = cd.Children(cd.Root).ToList();
        children.Should().NotBeEmpty();
    }
}