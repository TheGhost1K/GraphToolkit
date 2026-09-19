using GraphToolkit.Core;
using GraphToolkit.Flow;

namespace Transportation;

/// <summary>
/// Пример: транспортная задача (склады → магазины).
/// Демонстрирует max-flow и min-cost flow.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Транспортная задача: склады → магазины ===\n");

        // Склады: S1, S2, S3 с запасами.
        // Магазины: M1, M2, M3 с потребностями.
        // Стоимость доставки единицы товара указана в рублях.

        // --- Часть 1. Максимальный поток ---
        Console.WriteLine("[Max Flow] Максимальный объём поставок:\n");

        var net = new FlowNetwork<string>();

        // Источник → склады (capacity = запас)
        net.AddEdge("SOURCE", "Склад 1", 100);
        net.AddEdge("SOURCE", "Склад 2", 80);
        net.AddEdge("SOURCE", "Склад 3", 60);

        // Склады → магазины (capacity = пропускная способность маршрута)
        net.AddEdge("Склад 1", "Магазин 1", 50);
        net.AddEdge("Склад 1", "Магазин 2", 40);
        net.AddEdge("Склад 1", "Магазин 3", 30);

        net.AddEdge("Склад 2", "Магазин 1", 40);
        net.AddEdge("Склад 2", "Магазин 2", 50);
        net.AddEdge("Склад 2", "Магазин 3", 20);

        net.AddEdge("Склад 3", "Магазин 1", 30);
        net.AddEdge("Склад 3", "Магазин 2", 20);
        net.AddEdge("Склад 3", "Магазин 3", 40);

        // Магазины → сток (capacity = потребность)
        net.AddEdge("Магазин 1", "SINK", 90);
        net.AddEdge("Магазин 2", "SINK", 80);
        net.AddEdge("Магазин 3", "SINK", 70);

        double maxFlow = Dinic.Compute(net, "SOURCE", "SINK");
        Console.WriteLine($"  Общий максимальный поток: {maxFlow} единиц");
        Console.WriteLine();

        // --- Часть 2. Min-cost max-flow ---
        Console.WriteLine("[Min-Cost Flow] Минимальная стоимость доставки:\n");

        var costNet = new CostFlowNetwork<string>();

        costNet.AddEdge("SOURCE", "Склад 1", 100, 0);
        costNet.AddEdge("SOURCE", "Склад 2", 80, 0);
        costNet.AddEdge("SOURCE", "Склад 3", 60, 0);

        // Стоимость доставки: склад → магазин
        costNet.AddEdge("Склад 1", "Магазин 1", 50, 3);
        costNet.AddEdge("Склад 1", "Магазин 2", 40, 5);
        costNet.AddEdge("Склад 1", "Магазин 3", 30, 7);

        costNet.AddEdge("Склад 2", "Магазин 1", 40, 4);
        costNet.AddEdge("Склад 2", "Магазин 2", 50, 2);
        costNet.AddEdge("Склад 2", "Магазин 3", 20, 6);

        costNet.AddEdge("Склад 3", "Магазин 1", 30, 6);
        costNet.AddEdge("Склад 3", "Магазин 2", 20, 5);
        costNet.AddEdge("Склад 3", "Магазин 3", 40, 3);

        costNet.AddEdge("Магазин 1", "SINK", 90, 0);
        costNet.AddEdge("Магазин 2", "SINK", 80, 0);
        costNet.AddEdge("Магазин 3", "SINK", 70, 0);

        var (flow, cost) = MinCostFlow.DijkstraWithPotentials(costNet, "SOURCE", "SINK");

        Console.WriteLine($"  Поток:     {flow} единиц");
        Console.WriteLine($"  Стоимость: {cost} руб.");
        Console.WriteLine($"  Средняя:   {(flow > 0 ? cost / flow : 0):F2} руб/ед.");
        Console.WriteLine();

        // --- Часть 3. Сравнение с SPFA ---
        Console.WriteLine("[Сравнение] Проверка другим алгоритмом (SPFA):\n");

        var costNet2 = new CostFlowNetwork<string>();
        costNet2.AddEdge("SOURCE", "Склад 1", 100, 0);
        costNet2.AddEdge("SOURCE", "Склад 2", 80, 0);
        costNet2.AddEdge("SOURCE", "Склад 3", 60, 0);
        costNet2.AddEdge("Склад 1", "Магазин 1", 50, 3);
        costNet2.AddEdge("Склад 1", "Магазин 2", 40, 5);
        costNet2.AddEdge("Склад 1", "Магазин 3", 30, 7);
        costNet2.AddEdge("Склад 2", "Магазин 1", 40, 4);
        costNet2.AddEdge("Склад 2", "Магазин 2", 50, 2);
        costNet2.AddEdge("Склад 2", "Магазин 3", 20, 6);
        costNet2.AddEdge("Склад 3", "Магазин 1", 30, 6);
        costNet2.AddEdge("Склад 3", "Магазин 2", 20, 5);
        costNet2.AddEdge("Склад 3", "Магазин 3", 40, 3);
        costNet2.AddEdge("Магазин 1", "SINK", 90, 0);
        costNet2.AddEdge("Магазин 2", "SINK", 80, 0);
        costNet2.AddEdge("Магазин 3", "SINK", 70, 0);

        var (flow2, cost2) = MinCostFlow.Spfa(costNet2, "SOURCE", "SINK");

        Console.WriteLine($"  Поток:  {flow2}, стоимость: {cost2}");
        Console.WriteLine($"  Совпадает с Дейкстрой+потенциалами: {flow == flow2 && Math.Abs(cost - cost2) < 0.001}");
    }
}