using FluentAssertions;
using GraphToolkit.Core;
using GraphToolkit.Flow;
using Xunit;

namespace GraphToolkit.Tests.Flow;

public class MinCostFlowTests
{
    private static CostFlowNetwork<string> CreateNetwork()
    {
        var net = new CostFlowNetwork<string>();
        net.AddEdge("S", "A", capacity: 4, cost: 2);
        net.AddEdge("S", "B", capacity: 3, cost: 1);
        net.AddEdge("A", "B", capacity: 1, cost: 1);
        net.AddEdge("A", "T", capacity: 3, cost: 3);
        net.AddEdge("B", "T", capacity: 4, cost: 2);
        return net;
    }

    [Fact]
    public void Spfa_ShouldFindMaxFlowWithMinCost()
    {
        var net = CreateNetwork();
        var (flow, cost) = MinCostFlow.Spfa(net, "S", "T");

        flow.Should().Be(7); // max flow
        cost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void DijkstraWithPotentials_ShouldMatchSpfaCost()
    {
        var net1 = CreateNetwork();
        var net2 = CreateNetwork();

        var (f1, c1) = MinCostFlow.Spfa(net1, "S", "T");
        var (f2, c2) = MinCostFlow.DijkstraWithPotentials(net2, "S", "T");

        f1.Should().Be(f2);
        c1.Should().Be(c2);
    }

    [Fact]
    public void DijkstraWithPotentials_WithMaxFlowLimit_ShouldStopEarly()
    {
        var net = CreateNetwork();
        var (flow, _) = MinCostFlow.DijkstraWithPotentials(net, "S", "T", maxFlow: 3);

        flow.Should().Be(3);
    }

    [Fact]
    public void Spfa_SimpleNetwork_ShouldComputeCorrectCost()
    {
        var net = new CostFlowNetwork<string>();
        net.AddEdge("S", "A", 2, 1);   // cap=2, cost=1
        net.AddEdge("A", "T", 2, 3);   // cap=2, cost=3

        var (flow, cost) = MinCostFlow.Spfa(net, "S", "T");

        flow.Should().Be(2);
        cost.Should().Be(8); // 2 * (1 + 3)
    }
}