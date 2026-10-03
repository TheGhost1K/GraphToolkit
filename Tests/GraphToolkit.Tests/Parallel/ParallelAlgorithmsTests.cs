using FluentAssertions;
using GraphToolkit.Centrality;
using GraphToolkit.Components;
using GraphToolkit.MinimumSpanningTree;
using GraphToolkit.Parallel;
using GraphToolkit.ShortestPaths;
using GraphToolkit.Traversal;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Parallel;

public class ParallelAlgorithmsTests
{
    // ============================================================
    //  ParallelPageRank
    // ============================================================

    [Fact]
    public void ParallelPageRank_MatchesSequential()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B").AddEdge("B", "C")
            .AddEdge("C", "A").AddEdge("A", "C")
            .Build();

        var seq = PageRank.Compute(g);
        var par = ParallelPageRank.Compute(g);

        foreach (var v in g.Vertices)
            par[v].Should().BeApproximately(seq[v], 1e-6);
    }

    [Fact]
    public void ParallelPageRank_SumToOne()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1).AddEdge(1, 2).AddEdge(2, 0)
            .Build();

        var ranks = ParallelPageRank.Compute(g);
        ranks.Values.Sum().Should().BeApproximately(1.0, 1e-6);
    }

    // ============================================================
    //  ParallelFloydWarshall
    // ============================================================

    [Fact]
    public void ParallelFloydWarshall_MatchesSequential()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 3)
            .AddEdge("B", "C", 4)
            .AddEdge("A", "C", 10)
            .Build();

        var (seqDist, _, vertices) = FloydWarshall.Compute(g);
        var (parDist, _, _) = ParallelFloydWarshall.Compute(g);

        var index = vertices.Select((v, i) => (v, i))
            .ToDictionary(x => x.v, x => x.i);

        for (int i = 0; i < vertices.Count; i++)
            for (int j = 0; j < vertices.Count; j++)
                parDist[i, j].Should().Be(seqDist[i, j]);
    }

    // ============================================================
    //  ParallelBellmanFord
    // ============================================================

    [Fact]
    public void ParallelBellmanFord_MatchesSequential()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 4)
            .AddEdge("A", "C", 2)
            .AddEdge("C", "B", 1)
            .AddEdge("B", "D", 5)
            .Build();

        var (seqDist, _, _) = BellmanFord.Compute(g, "A");
        var (parDist, _, _) = ParallelBellmanFord.Compute(g, "A");

        foreach (var v in g.Vertices)
            parDist[v].Should().BeApproximately(seqDist[v], 1e-6);
    }

    [Fact]
    public void ParallelBellmanFord_DetectsNegativeCycle()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1, 1)
            .AddEdge(1, 2, -1)
            .AddEdge(2, 0, -1)
            .Build();

        var (_, _, hasCycle) = ParallelBellmanFord.Compute(g, 0);
        hasCycle.Should().BeTrue();
    }

    // ============================================================
    //  ParallelComponents
    // ============================================================

    [Fact]
    public void ParallelComponents_MatchesSequential()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(2, 3)
            .AddEdge(4, 5)
            .AddVertex(6)
            .Build();

        var seq = ConnectedComponents.Find(g);
        var par = ParallelComponents.Find(g);

        seq.Count.Should().Be(par.Count);
    }

    // ============================================================
    //  ParallelScc
    // ============================================================

    [Fact]
    public void ParallelScc_MatchesSequential()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1).AddEdge(1, 2).AddEdge(2, 0)
            .AddEdge(2, 3)
            .AddEdge(3, 4).AddEdge(4, 5).AddEdge(5, 3)
            .Build();

        var seq = StronglyConnectedComponents.Find(g);
        var par = ParallelScc.Find(g);

        seq.Count.Should().Be(par.Count);
    }

    // ============================================================
    //  ParallelBoruvka
    // ============================================================

    [Fact]
    public void ParallelBoruvka_SameWeightAsSequential()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 4)
            .AddEdge("A", "C", 3)
            .AddEdge("B", "C", 1)
            .AddEdge("B", "D", 2)
            .AddEdge("C", "D", 5)
            .AddEdge("D", "E", 7)
            .Build();

        var seq = Boruvka.Compute(g);
        var par = ParallelBoruvka.Compute(g);

        seq.Sum(e => e.Weight).Should().Be(par.Sum(e => e.Weight));
    }

    // ============================================================
    //  MultiSourceBfs
    // ============================================================

    [Fact]
    public void MultiSourceBfs_DistancesCorrect()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1).AddEdge(1, 2)
            .AddEdge(0, 3).AddEdge(3, 4)
            .AddEdge(2, 5)
            .Build();

        var (dist, src) = MultiSourceBfs.Compute(g, new[] { 0, 5 });

        dist[0].Should().Be(0);
        dist[1].Should().Be(1);
        dist[5].Should().Be(0);
        dist[4].Should().Be(2);
        src[0].Should().Be(0);
        src[5].Should().Be(5);
    }

    // ============================================================
    //  ParallelDijkstra (delta-stepping)
    // ============================================================

    [Fact]
    public void ParallelDijkstra_MatchesSequential()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1, 4)
            .AddEdge(0, 2, 2)
            .AddEdge(2, 1, 1)
            .AddEdge(1, 3, 5)
            .Build();

        var (seqDist, _) = Dijkstra.Compute(g, 0);
        var (parDist, _) = ParallelDijkstra.Compute(g, 0);

        foreach (var v in g.Vertices)
            parDist[v].Should().BeApproximately(seqDist[v], 1e-6);
    }
}
