using GraphToolkit.Hamiltonian;
using GraphToolkit.Utils;

namespace GraphToolkit.Samples.Hamiltonian;

/// <summary>
/// Пример: гамильтонов цикл и задача коммивояжёра (TSP).
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Гамильтонов цикл ===\n");

        // Небольшой граф, где есть гамильтонов цикл.
        var g = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B")
            .AddEdge("B", "C")
            .AddEdge("C", "D")
            .AddEdge("D", "A")
            .AddEdge("A", "C")
            .Build();

        var cycle = HamiltonianCycle.FindCycle(g);
        Console.WriteLine(cycle is not null
            ? "Цикл: " + string.Join(" → ", cycle)
            : "Цикл не найден");
        Console.WriteLine();

        Console.WriteLine("=== TSP: точное решение (ветви и границы) ===\n");

        // Города с расстояниями.
        var cities = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B", 10)
            .AddEdge("A", "C", 15)
            .AddEdge("A", "D", 20)
            .AddEdge("B", "C", 35)
            .AddEdge("B", "D", 25)
            .AddEdge("C", "D", 30)
            .Build();

        var exact = Tsp.BranchAndBound(cities, "A");
        Console.WriteLine($"Оптимальный маршрут: {string.Join(" → ", exact.Path!)}");
        Console.WriteLine($"Стоимость: {exact.Cost}");
        Console.WriteLine();

        Console.WriteLine("=== TSP: приближённое решение ===\n");

        var approx = Tsp.NearestNeighbor(cities, "A");
        Console.WriteLine($"Маршрут: {string.Join(" → ", approx.Path!)}");
        Console.WriteLine($"Стоимость: {approx.Cost}");
        Console.WriteLine();

        // Сравнение
        double ratio = approx.Cost / exact.Cost;
        Console.WriteLine($"Отношение приближённого к точному: {ratio:F3}");
    }
}
