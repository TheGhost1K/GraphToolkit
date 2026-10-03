using FluentAssertions;
using GraphToolkit.Core;
using GraphToolkit.Flow;
using Xunit;

namespace GraphToolkit.Tests.Flow;

public class PushRelabelTests
{
    [Fact]
    public void PushRelabel_ClassicNetwork_Returns23()
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

        PushRelabel.Compute(net, "S", "T").Should().Be(23);
    }

    [Fact]
    public void PushRelabel_MatchesDinic()
    {
        var net1 = new FlowNetwork<int>();
        var net2 = new FlowNetwork<int>();
        for (int i = 0; i < 2; i++)
        {
            var net = i == 0 ? net1 : net2;
            net.AddEdge(0, 1, 10);
            net.AddEdge(0, 2, 10);
            net.AddEdge(1, 3, 10);
            net.AddEdge(2, 3, 10);
        }

        double pr = PushRelabel.Compute(net1, 0, 3);
        double dn = Dinic.Compute(net2, 0, 3);

        pr.Should().Be(dn);
    }
}
