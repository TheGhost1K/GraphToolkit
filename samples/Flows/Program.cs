using GraphToolkit.Core;
using GraphToolkit.Flow;

namespace GraphToolkit.Samples.Flows;

/// <summary>
/// Пример: максимальный поток и поток минимальной стоимости
/// в сети доставки.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Задача о максимальном потоке ===\n");

        // Модель: склад (S) → перевалочные базы (A, B) → магазины (T).
        var net = new FlowNetwork<string>();
        net.AddEdge("S", "A", 10);
        net.AddEdge("S", "B", 8);
        net.AddEdge("A", "B", 2);
        net.AddEdge("A", "C", 7);
        net.AddEdge("A", "T", 4);
        net.AddEdge("B", "C", 5);
        net.AddEdge("B", "T", 6);
        net.AddEdge("C", "T", 9);

        // Сравнение трёх алгоритмов — каждый клонирует сеть.
        double ff = FordFulkerson.Compute(net.Clone(), "S", "T");
        double ek = EdmondsKarp.Compute(net.Clone(), "S", "T");
        double dn = Dinic.Compute(net.Clone(), "S", "T");

        Console.WriteLine($"Форд-Фалкерсон: {ff}");
        Console.WriteLine($"Эдмондс-Карп:   {ek}");
        Console.WriteLine($"Диниц:          {dn}");
        Console.WriteLine();

        // Минимальный разрез
        var sourceSide = Dinic.MinCut(net.Clone(), "S", "T");
        Console.WriteLine("Минимальный разрез (сторона источника):");
        Console.WriteLine("  {" + string.Join(", ", sourceSide) + "}");
        Console.WriteLine();

        Console.WriteLine("=== Задача о потоке минимальной стоимости ===\n");

        // Та же топология, но с ценами доставки.
        var costNet = new CostFlowNetwork<string>();
        costNet.AddEdge("S", "A", 10, 1);
        costNet.AddEdge("S", "B", 8, 2);
        costNet.AddEdge("A", "B", 2, 1);
        costNet.AddEdge("A", "C", 7, 3);
        costNet.AddEdge("A", "T", 4, 5);
        costNet.AddEdge("B", "C", 5, 2);
        costNet.AddEdge("B", "T", 6, 4);
        costNet.AddEdge("C", "T", 9, 1);

        var (flow, cost) = MinCostFlow.DijkstraWithPotentials(costNet, "S", "T");

        Console.WriteLine($"Максимальный поток: {flow}");
        Console.WriteLine($"Минимальная стоимость: {cost}");
        Console.WriteLine($"Средняя стоимость единицы: {cost / flow:F2}");
    }
}
