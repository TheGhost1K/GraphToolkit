using FluentAssertions;
using GraphToolkit.Centrality;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Centrality;

public class CentralitiesTests
{
    [Fact]
    public void Degree_StarGraph_CenterHasHighest()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("C", "A").AddEdge("C", "B").AddEdge("C", "D")
            .Build();

        var c = Centralities.Degree(g);

        c["C"].Should().Be(1.0);   // 3 / 3
        c["A"].Should().BeApproximately(1.0 / 3, 1e-9);
    }

    [Fact]
    public void Closeness_PathGraph_MiddleHighest()
    {
        // A — B — C — D — E, центральная вершина — C
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 1).AddEdge("B", "C", 1)
            .AddEdge("C", "D", 1).AddEdge("D", "E", 1)
            .Build();

        var c = Centralities.Closeness(g);

        c["C"].Should().BeGreaterThan(c["A"]);
        c["C"].Should().BeGreaterThan(c["E"]);
    }

    [Fact]
    public void Betweenness_PathGraph_MiddleHighest()
    {
        // A — B — C — D — E, C лежит на большинстве кратчайших путей
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B").AddEdge("B", "C")
            .AddEdge("C", "D").AddEdge("D", "E")
            .Build();

        var c = Centralities.Betweenness(g);

        c["C"].Should().BeGreaterThan(c["A"]);
        c["C"].Should().BeGreaterThan(c["B"]);
    }

    [Fact]
    public void Eigenvector_PathGraph_MiddleHighest()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1).AddEdge(1, 2)
            .AddEdge(2, 3).AddEdge(3, 4)
            .Build();

        var c = Centralities.Eigenvector(g);

        c[2].Should().BeGreaterThan(c[0]);
        c[2].Should().BeGreaterThan(c[4]);
    }

    [Fact]
    public void Katz_AllNonNegative()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1).AddEdge(1, 2).AddEdge(2, 0)
            .Build();

        var c = Centralities.Katz(g);

        c.Values.Should().OnlyContain(v => v >= 0);
    }
}
