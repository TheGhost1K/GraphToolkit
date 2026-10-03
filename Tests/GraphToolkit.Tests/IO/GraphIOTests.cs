using FluentAssertions;
using GraphToolkit.IO;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.IO;

public class GraphIOTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    private string TempFile(string ext)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gt-test-{Guid.NewGuid()}.{ext}");
        _tempFiles.Add(path);
        return path;
    }

    public void Dispose()
    {
        foreach (var f in _tempFiles)
            if (File.Exists(f)) File.Delete(f);
    }

    // ---------- GraphML ----------

    [Fact]
    public void GraphML_RoundTrip_PreservesStructure()
    {
        var g = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 4.5)
            .AddEdge("B", "C", 2.0)
            .Build();

        var path = TempFile("graphml");
        GraphIO.SaveGraphML(g, path);
        var loaded = GraphIO.LoadGraphML(path, s => s);

        loaded.IsDirected.Should().BeTrue();
        loaded.VertexCount.Should().Be(3);
        loaded.EdgeCount.Should().Be(2);
    }

    // ---------- GEXF ----------

    [Fact]
    public void Gexf_RoundTrip_PreservesStructure()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 1.5)
            .AddEdge("B", "C", 2.5)
            .Build();

        var path = TempFile("gexf");
        GraphIO.SaveGexf(g, path);
        var loaded = GraphIO.LoadGexf(path, s => s);

        loaded.IsDirected.Should().BeFalse();
        loaded.VertexCount.Should().Be(3);
        loaded.EdgeCount.Should().Be(2);
    }

    // ---------- JSON ----------

    [Fact]
    public void Json_RoundTrip_PreservesWeights()
    {
        var g = new GraphBuilder<int>(isDirected: true)
            .AddEdge(1, 2, 7.5)
            .AddEdge(2, 3, 3.14)
            .Build();

        var path = TempFile("json");
        GraphIO.SaveJson(g, path);
        var loaded = GraphIO.LoadJson(path, int.Parse);

        loaded.IsDirected.Should().BeTrue();
        loaded.EdgeCount.Should().Be(2);
        loaded.Neighbors(1).First().Weight.Should().Be(7.5);
    }

    // ---------- CSV ----------

    [Fact]
    public void Csv_RoundTrip()
    {
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 1)
            .AddEdge("B", "C", 2)
            .Build();

        var path = TempFile("csv");
        GraphIO.SaveCsv(g, path);
        var loaded = GraphIO.LoadCsv(path, s => s);

        loaded.VertexCount.Should().Be(3);
        loaded.EdgeCount.Should().Be(2);
    }

    // ---------- DOT (import) ----------

    [Fact]
    public void Dot_LoadSimpleGraph()
    {
        var path = TempFile("dot");
        File.WriteAllText(path, @"
            digraph G {
                A -> B [label=""5""];
                B -> C [label=""3""];
            }
        ");

        var g = GraphIO.LoadDot(path, s => s);

        g.IsDirected.Should().BeTrue();
        g.EdgeCount.Should().Be(2);
        g.Neighbors("A").First().Weight.Should().Be(5);
    }
}
