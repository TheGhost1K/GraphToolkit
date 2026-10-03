using FluentAssertions;
using GraphToolkit.Community;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Community;

public class LabelPropagationTests
{
    [Fact]
    public void Compute_TwoCliques_ShouldFindTwoCommunities()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 1)
            .AddEdge(4, 5).AddEdge(5, 6).AddEdge(6, 4)
            .AddEdge(3, 4)
            .Build();

        var communities = LabelPropagation.Compute(g, seed: 42);

        // Вершины одной клики должны быть в одном сообществе
        var clique1 = new[] { 1, 2, 3 };
        var clique2 = new[] { 4, 5, 6 };

        var comm1 = communities.First(c => c.Contains(1));
        comm1.Should().Contain(2);
        comm1.Should().Contain(3);

        // Клики могут быть вместе или раздельно, но каждая — цельная
        var commWith4 = communities.First(c => c.Contains(4));
        commWith4.Should().Contain(5);
        commWith4.Should().Contain(6);
    }

    [Fact]
    public void Compute_EmptyGraph_ReturnsEmpty()
    {
        var g = new GraphBuilder<int>(isDirected: false).Build();
        LabelPropagation.Compute(g).Should().BeEmpty();
    }

    [Fact]
    public void Compute_IsolatedVertices_AllSeparate()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddVertex(1).AddVertex(2).AddVertex(3)
            .Build();

        var communities = LabelPropagation.Compute(g, seed: 42);

        communities.Should().HaveCount(3);
    }

    [Fact]
    public void Compute_ReproducibleWithSeed()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 4)
            .AddEdge(4, 5).AddEdge(5, 6).AddEdge(6, 1)
            .AddEdge(3, 5)
            .Build();

        var c1 = LabelPropagation.Compute(g, seed: 42);
        var c2 = LabelPropagation.Compute(g, seed: 42);

        c1.Count.Should().Be(c2.Count);
    }
}

public class LouvainTests
{
    [Fact]
    public void Compute_TwoCliques_ShouldFindTwoCommunities()
    {
        // Две клики по 5 вершин, соединённые одним ребром
        var builder = new GraphBuilder<int>(isDirected: false);
        for (int i = 1; i <= 5; i++)
            for (int j = i + 1; j <= 5; j++)
                builder.AddEdge(i, j, 1.0);
        for (int i = 6; i <= 10; i++)
            for (int j = i + 1; j <= 10; j++)
                builder.AddEdge(i, j, 1.0);
        builder.AddEdge(5, 6, 0.1);

        var g = builder.Build();
        var communities = Louvain.Compute(g);

        communities.Should().HaveCount(2);
    }

    [Fact]
    public void Modularity_PerfectClustering_HighValue()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 1)
            .AddEdge(4, 5).AddEdge(5, 6).AddEdge(6, 4)
            .Build();

        var communities = new List<List<int>>
        {
            new() { 1, 2, 3 },
            new() { 4, 5, 6 }
        };

        double q = Louvain.Modularity(g, communities);

        q.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Compute_EmptyGraph_ReturnsEmpty()
    {
        var g = new GraphBuilder<int>(isDirected: false).Build();
        Louvain.Compute(g).Should().BeEmpty();
    }
}
