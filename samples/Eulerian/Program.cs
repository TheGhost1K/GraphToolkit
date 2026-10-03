using GraphToolkit.Eulerian;
using GraphToolkit.Utils;

namespace GraphToolkit.Samples.Eulerian;

/// <summary>
/// Пример: эйлеровы пути и циклы — обход всех дорог города
/// без повторения.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Задача о почтальоне (эйлеров цикл) ===\n");

        // Граф дорог: можно ли пройти по каждой дороге ровно один раз
        // и вернуться в начало?
        var city = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B")
            .AddEdge("B", "C")
            .AddEdge("C", "D")
            .AddEdge("D", "A")
            .AddEdge("A", "C")
            .Build();

        Console.WriteLine($"Всего дорог: {city.EdgeCount}");
        Console.WriteLine($"Эйлеров цикл существует: {EulerianPath.HasCycle(city)}");

        var cycle = EulerianPath.Find(city);
        if (cycle is not null)
            Console.WriteLine("Маршрут: " + string.Join(" → ", cycle));
        Console.WriteLine();

        Console.WriteLine("=== Эйлеров путь с разными концами ===\n");

        // Сеть, где начало и конец не совпадают.
        var trail = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B")
            .AddEdge("B", "C")
            .AddEdge("C", "D")
            .AddEdge("D", "B")
            .AddEdge("B", "E")
            .Build();

        var check = EulerianPath.HasPath(trail);
        Console.WriteLine($"Эйлеров путь существует: {check.Exists}");
        if (check.Exists)
            Console.WriteLine($"Начало: {check.Start}, конец: {check.End}");

        var path = EulerianPath.Find(trail);
        Console.WriteLine("Маршрут: " + string.Join(" → ", path!));
    }
}
