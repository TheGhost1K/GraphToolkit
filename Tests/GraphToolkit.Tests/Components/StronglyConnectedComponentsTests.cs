using FluentAssertions;
using GraphToolkit.Components;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Components;

public class StronglyConnectedComponentsTests
{
    [Fact]
    public void Find_TwoCycles_ShouldReturnTwoSccs()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 0)
            .AddEdge(2, 3)
            .AddEdge(3, 4)
            .AddEdge(4, 5)
            .AddEdge(5, 3)
            .Build();

        var sccs = StronglyConnectedComponents.Find(g);

        sccs.Should().HaveCount(2);
        sccs.Should().Contain(c => c.Count == 3 && c.Contains(0) && c.Contains(1) && c.Contains(2));
        sccs.Should().Contain(c => c.Count == 3 && c.Contains(3) && c.Contains(4) && c.Contains(5));
    }

    [Fact]
    public void Find_Dag_ShouldReturnEachVertexAsScc()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .Build();

        var sccs = StronglyConnectedComponents.Find(g);

        sccs.Should().HaveCount(4);
        sccs.Should().OnlyContain(c => c.Count == 1);
    }

    [Fact]
    public void Find_OneBigCycle_ShouldReturnOneScc()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .AddEdge(3, 0)
            .Build();

        var sccs = StronglyConnectedComponents.Find(g);

        sccs.Should().HaveCount(1);
        sccs[0].Should().HaveCount(4);
    }
}