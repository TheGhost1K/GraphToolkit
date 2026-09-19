using FluentAssertions;
using GraphToolkit.Closure;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Closure;

public class TransitiveClosureTests
{
    [Fact]
    public void Compute_Chain_ShouldMarkAllReachable()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .Build();

        var reach = TransitiveClosure.Compute(g);
        var vertices = g.Vertices.ToList();
        int i0 = vertices.IndexOf(0);
        int i3 = vertices.IndexOf(3);

        reach[i0, i3].Should().BeTrue();
    }

    [Fact]
    public void ComputeDict_ShouldReturnAllReachable()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .Build();

        var dict = TransitiveClosure.ComputeDict(g);

        dict[0].Should().BeEquivalentTo(new[] { 0, 1, 2 });
        dict[1].Should().BeEquivalentTo(new[] { 1, 2 });
        dict[2].Should().BeEquivalentTo(new[] { 2 });
    }

    [Fact]
    public void Reduce_DagWithRedundantEdge_ShouldRemoveIt()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(0, 2)  // избыточное
            .Build();

        var reduced = TransitiveClosure.Reduce(g);

        reduced.EdgeCount.Should().Be(2);
        reduced.Edges.Should().NotContain(e => e.From == 0 && e.To == 2);
    }

    [Fact]
    public void Reduce_CyclicGraph_ShouldThrow()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1)
            .AddEdge(1, 0)
            .Build();

        var act = () => TransitiveClosure.Reduce(g);
        act.Should().Throw<InvalidOperationException>();
    }
}