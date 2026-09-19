using FluentAssertions;
using GraphToolkit.Components;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Components;

public class BridgesAndArticulationTests
{
    [Fact]
    public void Find_TriangleWithTail_ShouldDetectBridgeAndArticulation()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B")
            .AddEdge("B", "C")
            .AddEdge("C", "A")   // треугольник — не мосты
            .AddEdge("C", "D")   // мост
            .AddEdge("D", "E")
            .AddEdge("D", "F")
            .Build();

        var result = BridgesAndArticulation.Find(g);

        result.Bridges.Should().HaveCount(3);
        result.Bridges.Should().Contain(("C", "D"));
        result.Bridges.Should().Contain(("D", "E"));
        result.Bridges.Should().Contain(("D", "F"));

        result.ArticulationPoints.Should().Contain("C");
        result.ArticulationPoints.Should().Contain("D");
    }

    [Fact]
    public void Find_Cycle_ShouldHaveNoBridges()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .AddEdge(3, 4)
            .AddEdge(4, 1)
            .Build();

        var result = BridgesAndArticulation.Find(g);

        result.Bridges.Should().BeEmpty();
        result.ArticulationPoints.Should().BeEmpty();
    }

    [Fact]
    public void Find_Path_AllEdgesAreBridges()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .AddEdge(3, 4)
            .Build();

        var result = BridgesAndArticulation.Find(g);

        result.Bridges.Should().HaveCount(3);
        result.ArticulationPoints.Should().Contain(new[] { 2, 3 });
    }
}