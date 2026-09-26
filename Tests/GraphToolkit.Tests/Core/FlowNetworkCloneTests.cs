using FluentAssertions;
using GraphToolkit.Core;
using GraphToolkit.Flow;
using Xunit;

namespace GraphToolkit.Tests.Core;

public class FlowNetworkCloneTests
{
    [Fact]
    public void Clone_ShouldCopyVerticesAndEdges()
    {
        var net = new FlowNetwork<string>();
        net.AddEdge("S", "A", 10);
        net.AddEdge("A", "T", 5);

        var copy = net.Clone();

        copy.Vertices.Should().BeEquivalentTo(net.Vertices);
        copy.GetCapacity("S", "A").Should().Be(10);
        copy.GetCapacity("A", "T").Should().Be(5);
    }

    [Fact]
    public void Clone_ShouldBeIndependent()
    {
        var net = new FlowNetwork<string>();
        net.AddEdge("S", "A", 10);
        net.AddEdge("A", "T", 5);

        var copy = net.Clone();
        Dinic.Compute(copy, "S", "T");

        // Оригинал не должен измениться
        net.GetCapacity("S", "A").Should().Be(10);
        net.GetCapacity("A", "T").Should().Be(5);
    }

    [Fact]
    public void Clone_AllowsRunningMultipleAlgorithms()
    {
        var net = new FlowNetwork<string>();
        net.AddEdge("S", "A", 16);
        net.AddEdge("S", "B", 13);
        net.AddEdge("A", "B", 10);
        net.AddEdge("A", "C", 12);
        net.AddEdge("B", "D", 14);
        net.AddEdge("C", "B", 9);
        net.AddEdge("C", "T", 20);
        net.AddEdge("D", "C", 7);
        net.AddEdge("D", "T", 4);

        double ff = FordFulkerson.Compute(net.Clone(), "S", "T");
        double ek = EdmondsKarp.Compute(net.Clone(), "S", "T");
        double dn = Dinic.Compute(net.Clone(), "S", "T");

        ff.Should().Be(23);
        ek.Should().Be(23);
        dn.Should().Be(23);

        // Оригинал нетронут
        net.GetCapacity("S", "A").Should().Be(16);
    }

    [Fact]
    public void Clone_AfterMaxFlow_ShouldPreserveResidualState()
    {
        var net = new FlowNetwork<string>();
        net.AddEdge("S", "A", 10);
        net.AddEdge("A", "T", 5);

        Dinic.Compute(net, "S", "T");

        var residual = net.Clone();

        residual.GetCapacity("S", "A").Should().Be(5);   // 10 - 5
        residual.GetCapacity("A", "S").Should().Be(5);   // обратное ребро
        residual.GetCapacity("A", "T").Should().Be(0);   // 5 - 5
        residual.GetCapacity("T", "A").Should().Be(5);
    }
}

public class CostFlowNetworkCloneTests
{
    [Fact]
    public void Clone_ShouldCopyVerticesAndEdges()
    {
        var net = new CostFlowNetwork<string>();
        net.AddEdge("S", "A", 4, 2);
        net.AddEdge("A", "T", 4, 3);

        var copy = net.Clone();

        copy.Vertices.Should().BeEquivalentTo(new[] { "S", "A", "T" });
        copy.IndexOf("S").Should().Be(0);
        copy.IndexOf("A").Should().Be(1);
        copy.IndexOf("T").Should().Be(2);
    }

    [Fact]
    public void Clone_ShouldBeIndependent()
    {
        var net = new CostFlowNetwork<string>();
        net.AddEdge("S", "A", 4, 2);
        net.AddEdge("A", "T", 4, 3);

        var copy = net.Clone();
        var (flow1, cost1) = MinCostFlow.Spfa(copy, "S", "T");

        flow1.Should().Be(4);
        cost1.Should().Be(20);   // 4 * (2 + 3)

        // Оригинал не тронут — можно запустить другой алгоритм
        var (flow2, cost2) = MinCostFlow.DijkstraWithPotentials(net, "S", "T");
        flow2.Should().Be(4);
        cost2.Should().Be(20);
    }

    [Fact]
    public void Clone_AllowsComparingAlgorithms()
    {
        var net = new CostFlowNetwork<string>();
        net.AddEdge("S", "A", 4, 2);
        net.AddEdge("S", "B", 3, 1);
        net.AddEdge("A", "B", 1, 1);
        net.AddEdge("A", "T", 3, 3);
        net.AddEdge("B", "T", 4, 2);

        var (f1, c1) = MinCostFlow.Spfa(net.Clone(), "S", "T");
        var (f2, c2) = MinCostFlow.DijkstraWithPotentials(net.Clone(), "S", "T");

        f1.Should().Be(f2);
        c1.Should().Be(c2);
    }

    [Fact]
    public void Clone_AfterPartialFlow_ShouldPreserveResidualState()
    {
        var net = new CostFlowNetwork<string>();
        net.AddEdge("S", "A", 4, 1);
        net.AddEdge("A", "T", 4, 1);

        // Ограничиваем поток 2 единицами — сеть частично «использована»
        MinCostFlow.Spfa(net, "S", "T", maxFlow: 2);

        var residual = net.Clone();

        // Можно докачать ещё 2 единицы
        var (flow, _) = MinCostFlow.Spfa(residual, "S", "T");
        flow.Should().Be(2);
    }
}