using FluentAssertions;
using GraphToolkit.Components;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Components;

public class ConnectedComponentsTests
{
    [Fact]
    public void Find_TwoComponents_ShouldReturnTwo()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .AddEdge(4, 5)
            .AddVertex(6)
            .Build();

        var comps = ConnectedComponents.Find(g);

        comps.Should().HaveCount(3);
    }

    [Fact]
    public void Find_SingleComponent_ShouldReturnOne()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .AddEdge(3, 1)
            .Build();

        var comps = ConnectedComponents.Find(g);

        comps.Should().HaveCount(1);
        comps[0].Should().HaveCount(3);
    }

    [Fact]
    public void Find_EmptyGraph_ShouldReturnEmpty()
    {
        var g = new GraphBuilder<int>(isDirected: false).Build();
        ConnectedComponents.Find(g).Should().BeEmpty();
    }

    [Fact]
    public void Find_IsolatedVertices_ShouldBeSeparateComponents()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddVertex(1)
            .AddVertex(2)
            .AddVertex(3)
            .Build();

        var comps = ConnectedComponents.Find(g);

        comps.Should().HaveCount(3);
        comps.Should().OnlyContain(c => c.Count == 1);
    }
}