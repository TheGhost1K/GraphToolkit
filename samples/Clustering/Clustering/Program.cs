using GraphToolkit.Components;
using GraphToolkit.Core;
using GraphToolkit.MinimumSpanningTree;
using GraphToolkit.Utils;
using GraphToolkit.Visualization;

namespace Clustering;

/// <summary>
/// Пример: кластеризация через MST.
/// Демонстрирует MST и удаление k-1 самых тяжёлых рёбер.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Кластеризация данных через MST ===\n");

        // Точки данных в 2D-пространстве.
        // Вес ребра = расстояние между точками.
        var points = new Dictionary<string, (double X, double Y)>
        {
            ["A"] = (0, 0),
            ["B"] = (1, 1),
            ["C"] = (0.5, 0.3),
            ["D"] = (10, 10),
            ["E"] = (11, 9),
            ["F"] = (10.5, 11),
            ["G"] = (20, 5),
            ["H"] = (21, 6),
            ["I"] = (19.5, 4.5)
        };

        // Строим полный граф (каждая точка соединена с каждой).
        var builder = new GraphBuilder<string>(isDirected: false);
        var names = points.Keys.ToList();

        for (int i = 0; i < names.Count; i++)
            for (int j = i + 1; j < names.Count; j++)
            {
                var (x1, y1) = points[names[i]];
                var (x2, y2) = points[names[j]];
                double dist = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
                builder.AddEdge(names[i], names[j], dist);
            }

        var graph = builder.Build();
        Console.WriteLine($"Точек: {graph.VertexCount}, рёбер: {graph.EdgeCount}\n");

        // --- 1. Строим MST ---
        var mst = Kruskal.Compute(graph);
        double total = mst.Sum(e => e.Weight);

        Console.WriteLine("[MST] Минимальное остовное дерево:");
        foreach (var edge in mst.OrderBy(e => e.Weight))
            Console.WriteLine($"  {edge.From,-2} — {edge.To,-2} {edge.Weight,6:F2}");
        Console.WriteLine($"  Общий вес: {total:F2}\n");

        // --- 2. Кластеризация: удаляем k-1 самых тяжёлых рёбер ---
        Console.WriteLine("[Кластеризация] Разбиение на кластеры:\n");

        for (int k = 2; k <= 4; k++)
        {
            var clusters = Clusterize(graph, k);
            Console.WriteLine($"  k = {k}:");
            foreach (var cluster in clusters)
                Console.WriteLine($"    {{{string.Join(", ", cluster.OrderBy(x => x))}}}");
            Console.WriteLine();
        }

        // --- 3. Уровень "естественной" кластеризации по разрывам ---
        Console.WriteLine("[Разрывы] Самые тяжёлые рёбра MST (кандидаты на разрезание):");
        var sortedEdges = mst.OrderByDescending(e => e.Weight).ToList();
        for (int i = 0; i < Math.Min(4, sortedEdges.Count); i++)
        {
            var e = sortedEdges[i];
            Console.WriteLine($"  #{i + 1}: {e.From,-2} — {e.To,-2} = {e.Weight,6:F2}" +
                              $" {(i == 0 ? "← самый большой разрыв" : "")}");
        }
        Console.WriteLine();

        // --- 4. Визуализация ---
        Console.WriteLine("[Визуализация] Mermaid-диаграмма MST:");
        var mstGraph = new Graph<string>(isDirected: false);
        foreach (var v in graph.Vertices) mstGraph.AddVertex(v);
        foreach (var e in mst) mstGraph.AddEdge(e.From, e.To, e.Weight);
        Console.WriteLine(GraphExporters.ToMermaid(mstGraph));
    }

    /// <summary>
    /// Кластеризация: удаляем k-1 самых тяжёлых рёбер MST
    /// и ищем связные компоненты оставшегося графа.
    /// </summary>
    private static List<List<string>> Clusterize(Graph<string> graph, int k)
    {
        var mst = Kruskal.Compute(graph);
        var toRemove = mst.OrderByDescending(e => e.Weight).Take(k - 1).ToHashSet();

        var reduced = new Graph<string>(isDirected: false);
        foreach (var v in graph.Vertices) reduced.AddVertex(v);
        foreach (var e in mst)
            if (!toRemove.Contains(e))
                reduced.AddEdge(e.From, e.To, e.Weight);

        return ConnectedComponents.Find(reduced);
    }
}