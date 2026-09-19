using FluentAssertions;
using GraphToolkit.Core;
using GraphToolkit.Flow;
using Xunit;

namespace GraphToolkit.Tests.Flow;

public class EdmondsKarpAndFordFulkersonTests
{
    private static FlowNetwork<string> CreateNetwork()
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
        return net;
    }

    [Fact]
    public void EdmondsKarp_ShouldMatchDinicResult()
    {
        var net1 = CreateNetwork();
        var net2 = CreateNetwork();

        EdmondsKarp.Compute(net1, "S", "T")
            .Should().Be(Dinic.Compute(net2, "S", "T"));
    }

    [Fact]
    public void FordFulkerson_ShouldMatchDinicResult()
    {
        var net1 = CreateNetwork();
        var net2 = CreateNetwork();

        FordFulkerson.Compute(net1, "S", "T")
            .Should().Be(Dinic.Compute(net2, "S", "T"));
    }

    [Fact]
    public void AllThreeMaxFlowAlgorithms_ShouldAgree()
    {
        var n1 = CreateNetwork();
        var n2 = CreateNetwork();
        var n3 = CreateNetwork();

        double ff = FordFulkerson.Compute(n1, "S", "T");
        double ek = EdmondsKarp.Compute(n2, "S", "T");
        double dn = Dinic.Compute(n3, "S", "T");

        ff.Should().Be(23);
        ek.Should().Be(ff);
        dn.Should().Be(ff);
    }
}