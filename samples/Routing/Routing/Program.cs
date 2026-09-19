using GraphToolkit.ShortestPaths;
using GraphToolkit.Utils;
using GraphToolkit.Visualization;

namespace Routing;

/// <summary>
/// Пример: маршрутизация в дорожной сети города.
/// Демонстрирует Дейкстру и A* с эвристикой на основе координат.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== Маршрутизация в дорожной сети ===\n");

        // Создаём дорожную сеть (взвешенный неориентированный граф).
        // Вес ребра = время в пути (минуты).
        var city = new GraphBuilder<string>(isDirected: false)
            .AddEdge("Центр", "Вокзал", 12)
            .AddEdge("Центр", "Университет", 8)
            .AddEdge("Центр", "Парк", 6)
            .AddEdge("Вокзал", "Аэропорт", 35)
            .AddEdge("Вокзал", "Рынок", 10)
            .AddEdge("Университет", "Рынок", 15)
            .AddEdge("Университет", "Больница", 11)
            .AddEdge("Парк", "Больница", 9)
            .AddEdge("Рынок", "Аэропорт", 28)
            .AddEdge("Больница", "Аэропорт", 32)
            .AddEdge("Рынок", "Парк", 18)
            .Build();

        Console.WriteLine($"Городов: {city.VertexCount}, дорог: {city.EdgeCount}\n");

        // --- 1. Дейкстра: кратчайший путь от Центра до Аэропорта ---
        var path = Dijkstra.FindPath(city, "Центр", "Аэропорт");
        var (distances, _) = Dijkstra.Compute(city, "Центр");

        Console.WriteLine("[Дейкстра] Кратчайший путь Центр → Аэропорт:");
        Console.WriteLine("  " + string.Join(" → ", path!));
        Console.WriteLine($"  Время в пути: {distances["Аэропорт"]} мин\n");

        // --- 2. Все расстояния от Центра ---
        Console.WriteLine("[Дейкстра] Время от Центра до всех узлов:");
        foreach (var (node, d) in distances.OrderBy(kv => kv.Value))
            Console.WriteLine($"  {node,-12} {d,3} мин");
        Console.WriteLine();

        // --- 3. A* с эвристикой ---
        // Эвристика: условное расстояние по прямой между городами.
        var coords = new Dictionary<string, (double X, double Y)>
        {
            ["Центр"] = (0, 0),
            ["Вокзал"] = (2, 1),
            ["Университет"] = (1, 2),
            ["Парк"] = (-1, 1),
            ["Рынок"] = (1, 0),
            ["Больница"] = (-1, 2),
            ["Аэропорт"] = (3, 3)
        };

        double Heuristic(string a, string b)
        {
            var (ax, ay) = coords[a];
            var (bx, by) = coords[b];
            return Math.Sqrt((ax - bx) * (ax - bx) + (ay - by) * (ay - by));
        }

        var aStarPath = AStar.FindPath(city, "Центр", "Аэропорт", Heuristic);

        Console.WriteLine("[A*] Кратчайший путь Центр → Аэропорт с эвристикой:");
        Console.WriteLine("  " + string.Join(" → ", aStarPath!));
        Console.WriteLine();

        // --- 4. Проверка: пути совпадают ---
        bool same = path!.SequenceEqual(aStarPath!);
        Console.WriteLine($"Результаты Дейкстры и A* совпадают: {same}\n");

        // --- 5. Визуализация (Mermaid) ---
        Console.WriteLine("[Визуализация] Mermaid-диаграмма:");
        var mermaid = GraphExporters.ToMermaid(city);
        Console.WriteLine(mermaid);

        // --- 6. Флойд-Уоршелл: расстояния между всеми парами ---
        Console.WriteLine("[Флойд-Уоршелл] Матрица расстояний:");
        var matrix = GraphExporters.ToAdjacencyMatrix(city);
        Console.WriteLine(matrix);
    }
}