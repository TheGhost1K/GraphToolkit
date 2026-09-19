using FluentAssertions;
using GraphToolkit.Eulerian;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Eulerian;

public class EulerianPathTests
{
    // ---------- HasCycle ----------

    [Fact]
    public void HasCycle_SimpleCycle_ShouldReturnTrue()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 4).AddEdge(4, 1)
            .Build();

        EulerianPath.HasCycle(g).Should().BeTrue();
    }

    [Fact]
    public void HasCycle_OddDegrees_ShouldReturnFalse()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(2, 3)
            .Build();

        EulerianPath.HasCycle(g).Should().BeFalse();
    }

    [Fact]
    public void HasCycle_Directed_Balanced_ShouldReturnTrue()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 1)
            .Build();

        EulerianPath.HasCycle(g).Should().BeTrue();
    }

    [Fact]
    public void HasCycle_NoEdges_ShouldReturnFalse()
    {
        var g = new GraphBuilder<int>(isDirected: false).Build();
        EulerianPath.HasCycle(g).Should().BeFalse();
    }

    // ---------- HasPath ----------

    [Fact]
    public void HasPath_TwoOddDegrees_ShouldReturnTrueWithEndpoints()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 1).AddEdge(1, 4)
            .Build();

        var result = EulerianPath.HasPath(g);

        result.Exists.Should().BeTrue();
        result.HasEndpoints.Should().BeTrue();
    }

    [Fact]
    public void HasPath_Cycle_ShouldReturnTrueWithoutEndpoints()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 1)
            .Build();

        var result = EulerianPath.HasPath(g);

        result.Exists.Should().BeTrue();
        result.HasEndpoints.Should().BeFalse();
    }

    [Fact]
    public void HasPath_MoreThanTwoOddDegrees_ShouldReturnFalse()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(3, 4)
            .Build();

        var result = EulerianPath.HasPath(g);

        result.Exists.Should().BeFalse();
        result.HasEndpoints.Should().BeFalse();
    }

    [Fact]
    public void HasPath_Directed_TwoOddDegrees_ShouldReturnCorrectEndpoints()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 1).AddEdge(1, 4)
            .Build();

        var result = EulerianPath.HasPath(g);

        result.Exists.Should().BeTrue();
        result.HasEndpoints.Should().BeTrue();
        result.Start.Should().Be(1);
        result.End.Should().Be(4);
    }

    [Fact]
    public void HasPath_String_ShouldWorkWithReferenceType()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B").AddEdge("B", "C").AddEdge("C", "A").AddEdge("A", "D")
            .Build();

        var result = EulerianPath.HasPath(g);

        result.Exists.Should().BeTrue();
        result.HasEndpoints.Should().BeTrue();
    }

    [Fact]
    public void HasPath_NoEdges_ShouldReturnFalse()
    {
        var g = new GraphBuilder<int>(isDirected: false).Build();

        var result = EulerianPath.HasPath(g);

        result.Exists.Should().BeFalse();
        result.HasEndpoints.Should().BeFalse();
    }

    // ---------- Find ----------

    [Fact]
    public void Find_SimpleCycle_ShouldReturnClosedPath()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 1)
            .Build();

        var path = EulerianPath.Find(g);

        path.Should().NotBeNull();
        path!.Should().HaveCount(4);
        path.First().Should().Be(path.Last());
    }

    [Fact]
    public void Find_Chain_ShouldReturnAllVertices()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 4)
            .Build();

        var path = EulerianPath.Find(g);

        path.Should().NotBeNull();
        path!.Should().HaveCount(4);
    }

    [Fact]
    public void Find_NoEulerianPath_ShouldReturnNull()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(3, 4)
            .Build();

        EulerianPath.Find(g).Should().BeNull();
    }

    [Fact]
    public void Find_DirectedCycle_ShouldReturnClosedPath()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 1)
            .Build();

        var path = EulerianPath.Find(g);

        path.Should().NotBeNull();
        path!.Should().HaveCount(4);
        path.First().Should().Be(path.Last());
    }

    [Fact]
    public void Find_EmptyGraph_ShouldReturnEmptyList()
    {
        var g = new GraphBuilder<int>(isDirected: false).Build();

        var path = EulerianPath.Find(g);

        path.Should().NotBeNull();
        path!.Should().BeEmpty();
    }
}