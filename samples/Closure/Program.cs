using GraphToolkit.Closure;
using GraphToolkit.Utils;

namespace GraphToolkit.Samples.Closure;

/// <summary>
/// Пример: транзитивное замыкание и сокращение графа зависимостей.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Транзитивное замыкание ===\n");

        // Граф зависимостей модулей приложения.
        var deps = new GraphBuilder<string>(isDirected: true)
            .AddEdge("UI", "Controller")
            .AddEdge("Controller", "Service")
            .AddEdge("Service", "Repository")
            .AddEdge("Repository", "Database")
            .Build();

        var closure = TransitiveClosure.ComputeDict(deps);

        Console.WriteLine("Кто от кого зависит транзитивно:");
        foreach (var (module, reachable) in closure.OrderBy(kv => kv.Key))
        {
            var others = reachable.Where(r => r != module).ToList();
            if (others.Count > 0)
                Console.WriteLine($"  {module,-12} → {string.Join(", ", others)}");
        }
        Console.WriteLine();

        Console.WriteLine("=== Транзитивное сокращение ===\n");

        // Граф с избыточными зависимостями.
        var redundant = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B")
            .AddEdge("B", "C")
            .AddEdge("A", "C")   // избыточное: A → B → C
            .AddEdge("C", "D")
            .AddEdge("A", "D")   // избыточное: A → C → D
            .Build();

        Console.WriteLine("Исходный граф:");
        foreach (var e in redundant.Edges)
            Console.WriteLine($"  {e.From} → {e.To}");
        Console.WriteLine($"  Всего: {redundant.EdgeCount} зависимостей");
        Console.WriteLine();

        var reduced = TransitiveClosure.Reduce(redundant);

        Console.WriteLine("После транзитивного сокращения:");
        foreach (var e in reduced.Edges)
            Console.WriteLine($"  {e.From} → {e.To}");
        Console.WriteLine($"  Всего: {reduced.EdgeCount} зависимостей");
        Console.WriteLine();

        Console.WriteLine($"Экономия: {redundant.EdgeCount - reduced.EdgeCount} избыточных связей");
    }
}
