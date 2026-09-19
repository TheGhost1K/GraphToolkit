using FluentAssertions;
using GraphToolkit.ShortestPaths;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.ShortestPaths;

public class CriticalPathTests
{
    [Fact]
    public void Compute_ShouldFindLongestPath()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 3)
            .AddEdge("A", "C", 6)
            .AddEdge("B", "D", 4)
            .AddEdge("C", "D", 8)
            .AddEdge("D", "E", 2)
            .Build();

        var (path, length) = CriticalPath.Compute(g);

        path.Should().Equal("A", "C", "D", "E");
        length.Should().Be(16); // 6 + 8 + 2
    }

    [Fact]
    public void Compute_WithCycle_ShouldThrow()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(0, 1)
            .AddEdge(1, 0)
            .Build();

        var act = () => CriticalPath.Compute(g);
        act.Should().Throw<InvalidOperationException>();
    }
}