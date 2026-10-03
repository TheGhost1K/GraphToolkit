using GraphToolkit.Core;

namespace GraphToolkit.Benchmarks;

/// <summary>
/// Генераторы случайных графов для бенчмарков.
/// </summary>
internal static class GraphGenerators
{
    /// <summary>Разреженный граф: E ≈ 3V.</summary>
    public static Graph<int> SparseDirected(int v, int seed = 42)
    {
        var rnd = new Random(seed);
        var g = new Graph<int>(isDirected: true);
        for (int i = 0; i < v; i++) g.AddVertex(i);
        for (int i = 0; i < v * 3; i++)
        {
            int u = rnd.Next(v), w = rnd.Next(v);
            if (u != w) g.AddEdge(u, w, rnd.NextDouble() * 100 + 0.1);
        }
        return g;
    }

    /// <summary>Плотный граф: E ≈ V²/4.</summary>
    public static Graph<int> DenseUndirected(int v, int seed = 42)
    {
        var rnd = new Random(seed);
        var g = new Graph<int>(isDirected: false);
        for (int i = 0; i < v; i++) g.AddVertex(i);
        for (int i = 0; i < v; i++)
            for (int j = i + 1; j < v; j++)
                if (rnd.NextDouble() < 0.25)
                    g.AddEdge(i, j, rnd.NextDouble() * 100 + 0.1);
        return g;
    }

    /// <summary>DAG с V вершинами.</summary>
    public static Graph<int> Dag(int v, int seed = 42)
    {
        var rnd = new Random(seed);
        var g = new Graph<int>(isDirected: true);
        for (int i = 0; i < v; i++) g.AddVertex(i);
        for (int i = 0; i < v; i++)
            for (int j = i + 1; j < Math.Min(i + 5, v); j++)
                if (rnd.NextDouble() < 0.5)
                    g.AddEdge(i, j, rnd.NextDouble() * 100 + 0.1);
        return g;
    }

    /// <summary>Случайное дерево с V вершинами.</summary>
    public static Graph<int> Tree(int v, int seed = 42)
    {
        var rnd = new Random(seed);
        var g = new Graph<int>(isDirected: false);
        for (int i = 0; i < v; i++) g.AddVertex(i);
        for (int i = 1; i < v; i++)
            g.AddEdge(rnd.Next(i), i, rnd.NextDouble() * 100 + 0.1);
        return g;
    }

    /// <summary>Путь (worst case для stack overflow).</summary>
    public static Graph<int> Path(int v)
    {
        var g = new Graph<int>(isDirected: true);
        for (int i = 0; i < v; i++) g.AddVertex(i);
        for (int i = 0; i < v - 1; i++)
            g.AddEdge(i, i + 1, 1.0);
        return g;
    }
}
