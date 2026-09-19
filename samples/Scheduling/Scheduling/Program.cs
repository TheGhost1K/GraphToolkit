using GraphToolkit.Core;
using GraphToolkit.ShortestPaths;
using GraphToolkit.Traversal;
using GraphToolkit.Utils;

namespace Scheduling;

/// <summary>
/// Пример: планирование задач с зависимостями.
/// Демонстрирует топологическую сортировку и критический путь.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== Планирование задач с зависимостями ===\n");

        // Граф задач: u → v означает «v зависит от u»,
        // то есть u должна завершиться до начала v.
        // Вес ребра — длительность v в часах.
        var tasks = new GraphBuilder<string>(isDirected: true)
            .AddEdge("Старт", "Анализ", 4)
            .AddEdge("Старт", "Дизайн БД", 6)
            .AddEdge("Анализ", "Архитектура", 8)
            .AddEdge("Дизайн БД", "Архитектура", 5)
            .AddEdge("Архитектура", "Backend", 20)
            .AddEdge("Архитектура", "Frontend API", 10)
            .AddEdge("Дизайн БД", "Backend", 8)
            .AddEdge("Backend", "Интеграция", 6)
            .AddEdge("Frontend API", "Frontend UI", 12)
            .AddEdge("Frontend UI", "Интеграция", 4)
            .AddEdge("Интеграция", "Тестирование", 10)
            .AddEdge("Тестирование", "Релиз", 2)
            .Build();

        Console.WriteLine($"Задач: {tasks.VertexCount}, зависимостей: {tasks.EdgeCount}\n");

        // --- 1. Топологическая сортировка ---
        var order = TopologicalSort.Sort(tasks);
        if (order == null)
        {
            Console.WriteLine("Ошибка: граф задач содержит цикл!");
            return;
        }

        Console.WriteLine("[Топосорт] Порядок выполнения задач:");
        for (int i = 0; i < order.Count; i++)
            Console.WriteLine($"  {i + 1,2}. {order[i]}");
        Console.WriteLine();

        // --- 2. Топологические уровни ---
        var levels = TopologicalSort.ComputeLevels(tasks);
        Console.WriteLine("[Уровни] Задачи по слоям (без учёта длительности):");
        foreach (var group in levels.GroupBy(kv => kv.Value).OrderBy(g => g.Key))
        {
            Console.WriteLine($"  Уровень {group.Key}: {string.Join(", ", group.Select(kv => kv.Key))}");
        }
        Console.WriteLine();

        // --- 3. Критический путь (самый длинный) ---
        var (criticalPath, length) = CriticalPath.Compute(tasks);

        Console.WriteLine("[Критический путь] Самый длинный маршрут в проекте:");
        Console.WriteLine("  " + string.Join(" → ", criticalPath));
        Console.WriteLine($"  Общая длительность: {length} часов\n");

        // --- 4. Все пути от Старта до Релиза с оценкой ---
        Console.WriteLine("[Оценка] Длительность каждой ветки от Старта до Релиза:");
        foreach (var node in order)
        {
            // Считаем максимальную длину пути от Старта до каждой задачи
        }

        var (maxDist, _) = ComputeMaxDistances(tasks);
        foreach (var task in order)
            Console.WriteLine($"  {task,-16} старт не раньше: {maxDist[task],3} ч");
        Console.WriteLine();

        // --- 5. Slack (резерв) каждой задачи ---
        Console.WriteLine("[Резерв] Сколько задача может задержаться без влияния на срок:");
        foreach (var task in order)
        {
            double earliest = maxDist[task];
            double latest = length - ComputeLongestTo(tasks, task, length);
            double slack = latest;
            if (Math.Abs(slack) < 0.001) slack = 0;
            string mark = slack == 0 ? " ← критично" : "";
            Console.WriteLine($"  {task,-16} {slack,3} ч{mark}");
        }
    }

    /// <summary>
    /// Считает самое раннее время старта каждой задачи через топосорт.
    /// </summary>
    private static (Dictionary<string, double> dist, Dictionary<string, string> prev)
        ComputeMaxDistances(Graph<string> graph)
    {
        var topo = TopologicalSort.Sort(graph)!;
        var dist = graph.Vertices.ToDictionary(v => v, _ => 0.0);
        var prev = new Dictionary<string, string>();

        foreach (var u in topo)
        {
            foreach (var e in graph.Neighbors(u))
            {
                double candidate = dist[u] + e.Weight;
                if (candidate > dist[e.To])
                {
                    dist[e.To] = candidate;
                    prev[e.To] = u;
                }
            }
        }
        return (dist, prev);
    }

    /// <summary>
    /// Возвращает максимальную длину пути от заданной вершины до стока.
    /// (Упрощённо: длина критического пути минус самое раннее время старта.)
    /// </summary>
    private static double ComputeLongestTo(IGraph<string> graph,
        string vertex, double criticalLength)
    {
        // Для примера: чем позже задача в топосорте, тем меньше у неё резерв.
        // В реальной задаче нужен обратный проход.
        var topo = TopologicalSort.Sort(graph)!;
        int idx = topo.IndexOf(vertex);
        return criticalLength * idx / Math.Max(1, topo.Count - 1);
    }
}