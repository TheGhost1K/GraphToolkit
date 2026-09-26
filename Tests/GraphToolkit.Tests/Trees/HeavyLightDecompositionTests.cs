using FluentAssertions;
using GraphToolkit.Trees;
using GraphToolkit.Utils;
using Xunit;
using GraphToolkit.Core;

namespace GraphToolkit.Tests.Trees;

public class HeavyLightDecompositionTests
{
    private static Graph<int> SimpleTree() =>
        new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(1, 3)
            .AddEdge(2, 4).AddEdge(2, 5)
            .AddEdge(3, 6)
            .Build();

    [Fact]
    public void Position_ShouldBeUnique()
    {
        var hld = new HeavyLightDecomposition<int>(SimpleTree(), root: 1);

        var positions = new[] { 1, 2, 3, 4, 5, 6 }
            .Select(hld.Position)
            .ToList();

        positions.Should().OnlyHaveUniqueItems();
        positions.Should().BeEquivalentTo(Enumerable.Range(0, 6));
    }

    [Fact]
    public void VertexAt_ShouldInvertPosition()
    {
        var hld = new HeavyLightDecomposition<int>(SimpleTree(), root: 1);

        foreach (var v in new[] { 1, 2, 3, 4, 5, 6 })
            hld.VertexAt(hld.Position(v)).Should().Be(v);
    }

    [Fact]
    public void Lca_Siblings_ShouldBeParent()
    {
        var hld = new HeavyLightDecomposition<int>(SimpleTree(), root: 1);

        hld.Lca(4, 5).Should().Be(2);
        hld.Lca(2, 3).Should().Be(1);
        hld.Lca(4, 6).Should().Be(1);
    }

    [Fact]
    public void Lca_SameVertex_ShouldReturnIt()
    {
        var hld = new HeavyLightDecomposition<int>(SimpleTree(), root: 1);

        hld.Lca(4, 4).Should().Be(4);
    }

    [Fact]
    public void Lca_AncestorDescendant_ShouldReturnAncestor()
    {
        var hld = new HeavyLightDecomposition<int>(SimpleTree(), root: 1);

        hld.Lca(1, 4).Should().Be(1);
        hld.Lca(2, 5).Should().Be(2);
    }

    [Fact]
    public void PathVertices_Simple_ShouldReturnAllOnPath()
    {
        var hld = new HeavyLightDecomposition<int>(SimpleTree(), root: 1);

        var path = hld.PathVertices(4, 6);

        path.Should().Contain(new[] { 4, 2, 1, 3, 6 });
        path.Should().HaveCount(5);
    }

    [Fact]
    public void PathVertices_SiblingsPath()
    {
        var hld = new HeavyLightDecomposition<int>(SimpleTree(), root: 1);

        var path = hld.PathVertices(4, 5);

        path.Should().Contain(new[] { 4, 2, 5 });
    }

    [Fact]
    public void PathSegments_ShouldCoverPath()
    {
        var hld = new HeavyLightDecomposition<int>(SimpleTree(), root: 1);

        var segments = hld.PathSegments(4, 6).ToList();

        // Собираем все позиции, которые покрывают отрезки
        var covered = new HashSet<int>();
        foreach (var (l, r) in segments)
            for (int i = l; i <= r; i++)
                covered.Add(i);

        // Каждая вершина пути должна попасть в объединение отрезков
        covered.Should().Contain(hld.Position(4));
        covered.Should().Contain(hld.Position(2));
        covered.Should().Contain(hld.Position(1));
        covered.Should().Contain(hld.Position(3));
        covered.Should().Contain(hld.Position(6));
    }

    [Fact]
    public void PathVertices_Chain_ShouldBeLinear()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 4).AddEdge(4, 5)
            .Build();

        var hld = new HeavyLightDecomposition<int>(g, root: 1);

        hld.PathVertices(1, 5).Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void Lca_LargeTree_ShouldBeCorrect()
    {
        // Бинарная куча: i/2 — родитель i
        var builder = new GraphBuilder<int>(isDirected: false);
        for (int i = 2; i <= 1000; i++)
            builder.AddEdge(i / 2, i);

        var hld = new HeavyLightDecomposition<int>(builder.Build(), root: 1);

        // 999 → 499 → 249 → 124 → 62
        // 1000 → 500 → 250 → 125 → 62
        // LCA(999, 1000) = 62
        hld.Lca(999, 1000).Should().Be(62);

        // LCA(999, 500): 999 → 499 → ... → 62; 500 → 250 → 125 → 62
        // LCA = 62
        hld.Lca(999, 500).Should().Be(62);

        // LCA(999, 499) = 499 (499 — родитель 999)
        hld.Lca(999, 499).Should().Be(499);

        // LCA(998, 999) = 499 (оба ребёнка 499)
        hld.Lca(998, 999).Should().Be(499);
    }
}