using FluentAssertions;
using GraphToolkit.Core;
using GraphToolkit.Flow;
using Xunit;

namespace GraphToolkit.Tests.Flow;

public class DinicTests
{
    [Fact]
    public void Compute_ClassicNetwork_ShouldReturn23()
    {
        // Классический пример из CLRS
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

        double flow = Dinic.Compute(net, "S", "T");

        flow.Should().Be(23);
    }

    [Fact]
    public void Compute_SimpleNetwork_ShouldReturnCorrectFlow()
    {
        var net = new FlowNetwork<string>();
        net.AddEdge("S", "A", 10);
        net.AddEdge("A", "T", 5);

        Dinic.Compute(net, "S", "T").Should().Be(5);
    }

    [Fact]
    public void Compute_NoPath_ShouldReturnZero()
    {
        var net = new FlowNetwork<int>();
        net.AddEdge(0, 1, 10);
        net.AddVertex(99);

        Dinic.Compute(net, 0, 99).Should().Be(0);
    }

    [Fact]
    public void MinCut_ShouldReturnSourceSide()
    {
        var net = new FlowNetwork<string>();
        net.AddEdge("S", "A", 10);
        net.AddEdge("A", "T", 5);

        var cut = Dinic.MinCut(net, "S", "T");

        cut.Should().Contain("S");
        cut.Should().Contain("A");
        cut.Should().NotContain("T");
    }

    [Fact]
    public void Compute_ParallelEdges_ShouldSumCapacity()
    {
        var net = new FlowNetwork<string>();
        net.AddEdge("S", "A", 5);
        net.AddEdge("S", "A", 3);
        net.AddEdge("A", "T", 10);

        Dinic.Compute(net, "S", "T").Should().Be(8);
    }
}