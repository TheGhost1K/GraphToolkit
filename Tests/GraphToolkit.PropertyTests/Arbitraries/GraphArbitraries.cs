using FsCheck;
using FsCheck.Fluent;
using GraphToolkit.Core;

namespace GraphToolkit.PropertyTests.Arbitraries;

/// <summary>
/// Генераторы случайных графов для property-based тестов.
/// </summary>
/// <remarks>
/// ВАЖНО: методы НЕ помечены как Arbitrary через атрибут — они
/// вызываются явно через Prop.ForAll(...), чтобы FsCheck не выбирал
/// автоматически неподходящий генератор.
/// </remarks>
public static class GraphArbitraries
{
    /// <summary>Направленный граф без самопетель.</summary>
    public static Arbitrary<Graph<int>> DirectedGraph() =>
        Arb.From(BuildGraphGen(2, 20, isDirected: true));

    /// <summary>Неориентированный граф без самопетель.</summary>
    public static Arbitrary<Graph<int>> UndirectedGraph() =>
        Arb.From(BuildGraphGen(2, 20, isDirected: false));

    /// <summary>Связный неориентированный граф.</summary>
    public static Arbitrary<Graph<int>> ConnectedGraph() =>
        Arb.From(BuildConnectedGen(2, 20));

    /// <summary>DAG с положительными весами.</summary>
    public static Arbitrary<Graph<int>> Dag() =>
        Arb.From(BuildDagGen(2, 20));

    /// <summary>Дерево с положительными весами.</summary>
    public static Arbitrary<Graph<int>> Tree() =>
        Arb.From(BuildTreeGen(2, 30));

    // ============================================================
    //  Генераторы (все через Select / SelectMany — без LINQ from)
    // ============================================================

    private static Gen<Graph<int>> BuildGraphGen(
        int minV, int maxV, bool isDirected)
    {
        return Gen.Choose(minV, maxV).SelectMany(n =>
            Gen.Choose(0, n * 3).SelectMany(count =>
                Gen.ArrayOf(EdgeGen(n), count).Select(edges =>
                    BuildGraph(n, edges, isDirected))));
    }

    private static Gen<Graph<int>> BuildConnectedGen(int minV, int maxV)
    {
        return Gen.Choose(minV, maxV).SelectMany(n =>
            Gen.Choose(0, n * 2).Select(extra => BuildConnected(n, extra)));
    }

    private static Gen<Graph<int>> BuildDagGen(int minV, int maxV)
    {
        return Gen.Choose(minV, maxV).SelectMany(n =>
            Gen.Choose(0, n * 2).SelectMany(count =>
                Gen.ArrayOf(DagEdgeGen(n), count).Select(edges =>
                    BuildGraph(n, edges, isDirected: true))));
    }

    private static Gen<Graph<int>> BuildTreeGen(int minV, int maxV)
    {
        return Gen.Choose(minV, maxV).Select(BuildTree);
    }

    private static Gen<(int From, int To, double Weight)> EdgeGen(int n)
    {
        return Gen.Choose(0, n - 1).SelectMany(from =>
            Gen.Choose(0, n - 1).SelectMany(to =>
                Gen.Choose(1, 1000).Select(w => (from, to, w / 10.0))));
    }

    private static Gen<(int From, int To, double Weight)> DagEdgeGen(int n)
    {
        int maxFrom = Math.Max(0, n - 2);
        return Gen.Choose(0, maxFrom).SelectMany(from =>
            Gen.Choose(from + 1, n - 1).SelectMany(to =>
                Gen.Choose(1, 1000).Select(w => (from, to, w / 10.0))));
    }

    // ============================================================
    //  Сборка графов
    // ============================================================

    private static Graph<int> BuildGraph(
        int n,
        (int From, int To, double Weight)[] edges,
        bool isDirected)
    {
        var g = new Graph<int>(isDirected);
        for (int i = 0; i < n; i++) g.AddVertex(i);

        foreach (var (from, to, w) in edges)
        {
            if (from == to) continue;
            g.AddEdge(from, to, w);
        }
        return g;
    }

    private static Graph<int> BuildConnected(int n, int extraEdges)
    {
        var rnd = new Random(n * 7919 + extraEdges);
        var g = new Graph<int>(isDirected: false);
        for (int i = 0; i < n; i++) g.AddVertex(i);

        // Дедупликация пар (u,v), u < v
        var used = new HashSet<(int, int)>();

        // Связующее дерево
        for (int i = 1; i < n; i++)
        {
            int parent = rnd.Next(i);
            var key = (parent, i);
            used.Add(key);
            g.AddEdge(parent, i, rnd.NextDouble() * 10 + 0.1);
        }

        // Дополнительные рёбра — только уникальные
        int attempts = 0;
        int added = 0;
        while (added < extraEdges && attempts < extraEdges * 10)
        {
            attempts++;
            int u = rnd.Next(n), v = rnd.Next(n);
            if (u == v) continue;

            var key = u < v ? (u, v) : (v, u);
            if (!used.Add(key)) continue;

            g.AddEdge(u, v, rnd.NextDouble() * 10 + 0.1);
            added++;
        }
        return g;
    }

    private static Graph<int> BuildTree(int n)
    {
        if (n < 2)
        {
            var empty = new Graph<int>(isDirected: false);
            if (n == 1) empty.AddVertex(0);
            return empty;
        }

        var rnd = new Random(n * 31 + 17);
        var g = new Graph<int>(isDirected: false);
        for (int i = 0; i < n; i++) g.AddVertex(i);

        // Строго n-1 рёбер, каждое соединяет новый узел со старым
        for (int i = 1; i < n; i++)
            g.AddEdge(rnd.Next(i), i, rnd.NextDouble() * 10 + 0.1);

        return g;
    }
}
