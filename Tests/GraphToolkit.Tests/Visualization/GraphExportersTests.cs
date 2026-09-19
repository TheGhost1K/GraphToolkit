using FluentAssertions;
using GraphToolkit.Utils;
using GraphToolkit.Visualization;
using Xunit;

namespace GraphToolkit.Tests.Visualization;

public class GraphExportersTests
{
    [Fact]
    public void ToDot_DirectedGraph_ShouldContainDigraph()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 3)
            .Build();

        var dot = GraphExporters.ToDot(g);

        dot.Should().Contain("digraph");
        dot.Should().Contain("\"A\"");
        dot.Should().Contain("\"B\"");
        dot.Should().Contain("->");
        dot.Should().Contain("label=\"3\"");
    }

    [Fact]
    public void ToDot_UndirectedGraph_ShouldContainGraph()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2)
            .Build();

        var dot = GraphExporters.ToDot(g);

        dot.Should().Contain("graph");
        dot.Should().NotContain("digraph");
        dot.Should().Contain("--");
    }

    [Fact]
    public void ToMermaid_DirectedGraph_ShouldContainGraphLR()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(1, 2)
            .Build();

        var mermaid = GraphExporters.ToMermaid(g);

        mermaid.Should().Contain("graph LR");
        mermaid.Should().Contain("-->");
    }

    [Fact]
    public void ToMermaid_UndirectedGraph_ShouldContainGraphTD()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2)
            .Build();

        var mermaid = GraphExporters.ToMermaid(g);

        mermaid.Should().Contain("graph TD");
        mermaid.Should().Contain("---");
    }

    [Fact]
    public void ToAdjacencyMatrix_ShouldIncludeAllVertices()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 5)
            .AddEdge("A", "C", 3)
            .Build();

        var matrix = GraphExporters.ToAdjacencyMatrix(g);

        matrix.Should().Contain("A");
        matrix.Should().Contain("B");
        matrix.Should().Contain("C");
        matrix.Should().Contain("5");
        matrix.Should().Contain("3");
    }

    [Fact]
    public void ToAdjacencyList_ShouldShowNeighbors()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 5)
            .AddEdge("A", "C")
            .Build();

        var list = GraphExporters.ToAdjacencyList(g);

        list.Should().Contain("A:");
        list.Should().Contain("B(5)");
        list.Should().Contain("C");
    }
}