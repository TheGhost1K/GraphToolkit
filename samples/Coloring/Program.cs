using GraphToolkit.Coloring;
using GraphToolkit.Utils;

namespace GraphToolkit.Samples.Coloring;

/// <summary>
/// Пример: раскраска графа — составление расписания экзаменов.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Раскраска графа ===\n");

        // Граф конфликтов: студенты, у которых общие курсы,
        // не могут сдавать экзамены в один день.
        var conflicts = new GraphBuilder<string>(isDirected: false)
            .AddEdge("Математика", "Физика")
            .AddEdge("Математика", "Информатика")
            .AddEdge("Физика", "Химия")
            .AddEdge("Информатика", "Химия")
            .AddEdge("Химия", "Биология")
            .Build();

        Console.WriteLine("Граф конфликтов:");
        foreach (var e in conflicts.Edges)
            Console.WriteLine($"  {e.From} — {e.To}");
        Console.WriteLine();

        // Жадная раскраска (быстро, но не оптимально).
        var greedy = GraphColoring.Greedy(conflicts);
        int greedyColors = greedy.Values.Max() + 1;
        Console.WriteLine($"Жадная раскраска: {greedyColors} дней");
        foreach (var (course, color) in greedy.OrderBy(kv => kv.Value))
            Console.WriteLine($"  {course,-14} день {color + 1}");
        Console.WriteLine();

        // Точная раскраска (медленнее, но минимум).
        var (exact, chi) = GraphColoring.Exact(conflicts);
        Console.WriteLine($"Оптимальная раскраска: χ(G) = {chi}");
        foreach (var (course, color) in exact.OrderBy(kv => kv.Value))
            Console.WriteLine($"  {course,-14} день {color + 1}");
        Console.WriteLine();

        // Проверка двудольности
        var bipartite = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .AddEdge(3, 4)
            .AddEdge(4, 1)
            .Build();

        Console.WriteLine("=== Проверка двудольности ===");
        bool isBip = GraphColoring.IsBipartite(bipartite, out var parts);
        Console.WriteLine($"Граф двудольный: {isBip}");
        if (isBip)
        {
            Console.WriteLine("Части:");
            Console.WriteLine("  A: " + string.Join(", ",
                parts.Where(kv => kv.Value == 0).Select(kv => kv.Key)));
            Console.WriteLine("  B: " + string.Join(", ",
                parts.Where(kv => kv.Value == 1).Select(kv => kv.Key)));
        }
    }
}
