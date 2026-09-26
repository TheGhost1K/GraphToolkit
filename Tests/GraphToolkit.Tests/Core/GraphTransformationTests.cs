using FluentAssertions;
using GraphToolkit.Core;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Core;

public class GraphTransformationTests
{
    // ---------- Clone ----------

    [Fact]
    public void Clone_ShouldCopyVerticesAndEdges()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 3)
            .AddEdge("B", "C", 5)
            .Build();

        var copy = g.Clone();

        copy.VertexCount.Should().Be(3);
        copy.EdgeCount.Should().Be(2);
        copy.IsDirected.Should().BeTrue();
    }

    [Fact]
    public void Clone_ShouldBeIndependent()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 3)
            .Build();

        var copy = g.Clone();
        copy.AddEdge("X", "Y");

        g.VertexCount.Should().Be(2);   // оригинал не изменился
        copy.VertexCount.Should().Be(4);
    }

    [Fact]
    public void Clone_ShouldPreserveWeights()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2, 7.5)
            .AddEdge(2, 3, 3.14)
            .Build();

        var copy = g.Clone();

        copy.Neighbors(1).First().Weight.Should().Be(7.5);
        copy.Neighbors(2).First(e => e.To == 3).Weight.Should().Be(3.14);
    }

    // ---------- ToUndirected ----------

    [Fact]
    public void ToUndirected_Directed_ShouldBecomeUndirected()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 3)
            .AddEdge("B", "C", 5)
            .Build();

        var undirected = g.ToUndirected();

        undirected.IsDirected.Should().BeFalse();
        undirected.VertexCount.Should().Be(3);
        undirected.EdgeCount.Should().Be(2);
    }

    [Fact]
    public void ToUndirected_ShouldDeduplicateReciprocalEdges()
    {
        var g = new Graph<string>(isDirected: true);
        g.AddEdge("A", "B", 1);
        g.AddEdge("B", "A", 5);   // обратное ребро с другим весом

        var undirected = g.ToUndirected();

        undirected.EdgeCount.Should().Be(1);   // только одно ребро
    }

    [Fact]
    public void ToUndirected_AlreadyUndirected_ShouldReturnClone()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .Build();

        var copy = g.ToUndirected();

        copy.IsDirected.Should().BeFalse();
        copy.VertexCount.Should().Be(3);
        copy.EdgeCount.Should().Be(2);
    }

    // ---------- ToDirected ----------

    [Fact]
    public void ToDirected_Undirected_ShouldBecomeDirected()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 3)
            .AddEdge("B", "C", 5)
            .Build();

        var directed = g.ToDirected();

        directed.IsDirected.Should().BeTrue();
        directed.VertexCount.Should().Be(3);
        directed.EdgeCount.Should().Be(4);   // каждое ребро → 2 направленных

        // Проверяем оба направления
        directed.Neighbors("A").Should().ContainSingle(e => e.To == "B");
        directed.Neighbors("B").Should().Contain(e => e.To == "A");
        directed.Neighbors("B").Should().Contain(e => e.To == "C");
        directed.Neighbors("C").Should().Contain(e => e.To == "B");
    }

    [Fact]
    public void ToDirected_AlreadyDirected_ShouldReturnClone()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(1, 2)
            .Build();

        var copy = g.ToDirected();

        copy.IsDirected.Should().BeTrue();
        copy.EdgeCount.Should().Be(1);
    }

    // ---------- Transpose (существующий) ----------

    [Fact]
    public void Transpose_ShouldReverseAllEdges()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 3)
            .AddEdge("B", "C", 5)
            .Build();

        var t = g.Transpose();

        t.Neighbors("A").Should().BeEmpty();
        t.Neighbors("B").Should().ContainSingle(e => e.To == "A" && e.Weight == 3);
        t.Neighbors("C").Should().ContainSingle(e => e.To == "B" && e.Weight == 5);
    }
}