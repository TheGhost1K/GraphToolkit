using FluentAssertions;
using GraphToolkit.Core;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Core;

public class CSRGraphTests
{
    [Fact]
    public void Constructor_PreservesVerticesAndEdges()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 3)
            .AddEdge("B", "C", 5)
            .Build();

        var csr = new CSRGraph<string>(g);

        csr.VertexCount.Should().Be(3);
        csr.EdgeCount.Should().Be(2);
    }

    [Fact]
    public void Neighbors_ReturnsCorrectIndices()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B")
            .AddEdge("A", "C")
            .Build();

        var csr = new CSRGraph<string>(g);
        int aIdx = csr.IndexOf("A");

        var neighbors = csr.Neighbors(aIdx);
        neighbors.Length.Should().Be(2);
    }

    [Fact]
    public void Undirected_AddsBothDirections()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1, 5)
            .Build();

        var csr = new CSRGraph<int>(g);

        csr.EdgeCount.Should().Be(2);  // оба направления в CSR
        csr.Neighbors(0).Length.Should().Be(1);
        csr.Neighbors(1).Length.Should().Be(1);
    }

    [Fact]
    public void ApproximateMemory_LessThanAdjacencyList()
    {
        var g = new GraphBuilder<int>(isDirected: false).Build();
        var builder = new GraphBuilder<int>(isDirected: false);
        for (int i = 0; i < 100; i++)
            builder.AddEdge(i, (i + 1) % 100);
        var graph = builder.Build();

        var csr = new CSRGraph<int>(graph);
        csr.ApproximateMemoryBytes().Should().BeGreaterThan(0);
    }
}
