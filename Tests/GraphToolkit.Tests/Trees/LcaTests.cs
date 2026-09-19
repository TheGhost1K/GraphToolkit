using FluentAssertions;
using GraphToolkit.Core;
using GraphToolkit.Trees;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Trees;

public class LcaTests
{
    private static Graph<string> CreateTree() =>
        new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B")
            .AddEdge("A", "C")
            .AddEdge("B", "D")
            .AddEdge("B", "E")
            .AddEdge("C", "F")
            .AddEdge("F", "G")
            .Build();

    [Fact]
    public void Query_Siblings_ShouldReturnParent()
    {
        var lca = new Lca<string>(CreateTree(), root: "A");

        lca.Query("D", "E").Should().Be("B");
        lca.Query("F", "C").Should().Be("C");
    }

    [Fact]
    public void Query_DifferentSubtrees_ShouldReturnRoot()
    {
        var lca = new Lca<string>(CreateTree(), root: "A");

        lca.Query("D", "G").Should().Be("A");
        lca.Query("E", "F").Should().Be("A");
    }

    [Fact]
    public void Query_AncestorDescendant_ShouldReturnAncestor()
    {
        var lca = new Lca<string>(CreateTree(), root: "A");

        lca.Query("A", "G").Should().Be("A");
        lca.Query("C", "G").Should().Be("C");
    }

    [Fact]
    public void Query_SameVertex_ShouldReturnIt()
    {
        var lca = new Lca<string>(CreateTree(), root: "A");

        lca.Query("D", "D").Should().Be("D");
    }
}