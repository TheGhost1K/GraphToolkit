using GraphToolkit.IO;
using GraphToolkit.Utils;

namespace GraphToolkit.Samples.FileIO;

/// <summary>
/// Пример: импорт и экспорт графов в разных форматах.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var outputDir = Path.Combine(AppContext.BaseDirectory, "output");
        Directory.CreateDirectory(outputDir);

        // Создаём граф для экспериментов
        var graph = new GraphBuilder<string>(isDirected: true)
            .AddEdge("A", "B", 4.5)
            .AddEdge("A", "C", 2.0)
            .AddEdge("B", "C", 5.0)
            .AddEdge("B", "D", 10.0)
            .AddEdge("C", "E", 3.0)
            .AddEdge("E", "D", 4.0)
            .Build();

        Console.WriteLine($"Исходный граф: {graph.VertexCount} вершин, " +
                          $"{graph.EdgeCount} рёбер, " +
                          $"directed = {graph.IsDirected}");
        Console.WriteLine();

        // ============================================================
        //  1. Экспорт во все форматы
        // ============================================================

        Section("1. Экспорт");

        var graphmlPath = Path.Combine(outputDir, "graph.graphml");
        var gexfPath = Path.Combine(outputDir, "graph.gexf");
        var jsonPath = Path.Combine(outputDir, "graph.json");
        var csvPath = Path.Combine(outputDir, "graph.csv");

        GraphIO.SaveGraphML(graph, graphmlPath);
        GraphIO.SaveGexf(graph, gexfPath);
        GraphIO.SaveJson(graph, jsonPath);
        GraphIO.SaveCsv(graph, csvPath);

        PrintFileInfo(graphmlPath, "GraphML");
        PrintFileInfo(gexfPath, "GEXF");
        PrintFileInfo(jsonPath, "JSON");
        PrintFileInfo(csvPath, "CSV");

        // ============================================================
        //  2. Загрузка из каждого формата
        // ============================================================

        Section("2. Импорт и проверка round-trip");

        VerifyRoundTrip(graph, GraphIO.LoadGraphML(graphmlPath, s => s),
            "GraphML");
        VerifyRoundTrip(graph, GraphIO.LoadGexf(gexfPath, s => s),
            "GEXF");
        VerifyRoundTrip(graph, GraphIO.LoadJson(jsonPath, s => s),
            "JSON");
        VerifyRoundTrip(graph, GraphIO.LoadCsv(csvPath, s => s, isDirected: true),
            "CSV");

        // ============================================================
        //  3. Разные типы вершин
        // ============================================================

        Section("3. Разные типы вершин");

        // Строковые вершины
        var strings = new GraphBuilder<string>(isDirected: false)
            .AddEdge("Москва", "Санкт-Петербург", 700)
            .AddEdge("Москва", "Казань", 800)
            .AddEdge("Казань", "Екатеринбург", 900)
            .Build();

        var stringPath = Path.Combine(outputDir, "cities.json");
        GraphIO.SaveJson(strings, stringPath);

        var loadedStrings = GraphIO.LoadJson(stringPath, s => s);
        Console.WriteLine($"Строковые вершины: {loadedStrings.VertexCount} городов");

        // Целочисленные вершины
        var ints = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2, 3.5)
            .AddEdge(2, 3, 4.5)
            .Build();

        var intPath = Path.Combine(outputDir, "ints.json");
        GraphIO.SaveJson(ints, intPath);

        var loadedInts = GraphIO.LoadJson(intPath, int.Parse);
        Console.WriteLine($"Целочисленные вершины: " +
                          $"{loadedInts.VertexCount} вершин");
        Console.WriteLine();

        // ============================================================
        //  4. Импорт из DOT
        // ============================================================

        Section("4. Импорт из GraphViz DOT");

        var dotPath = Path.Combine(outputDir, "input.dot");
        File.WriteAllText(dotPath, @"
            digraph Example {
                A -> B [label=""5.0""];
                B -> C [label=""3.0""];
                C -> A [label=""2.0""];
                B -> D [label=""4.0""];
            }
        ");

        var dotGraph = GraphIO.LoadDot(dotPath, s => s);

        Console.WriteLine($"Загружено из DOT: {dotGraph.VertexCount} вершин, " +
                          $"{dotGraph.EdgeCount} рёбер");
        Console.WriteLine($"Directed = {dotGraph.IsDirected}");
        Console.WriteLine();

        foreach (var e in dotGraph.Edges)
            Console.WriteLine($"  {e.From} → {e.To}  ({e.Weight})");
        Console.WriteLine();

        // ============================================================
        //  5. Полный пайплайн: DOT → обработка → GraphML
        // ============================================================

        Section("5. Пайплайн: DOT → анализ → GraphML");

        // Загружаем из DOT
        var original = GraphIO.LoadDot(dotPath, s => s);

        // Обрабатываем алгоритмом (например, находим MST)
        var mst = GraphToolkit.MinimumSpanningTree.Kruskal.Compute(original);

        // Строим граф MST
        var mstGraph = new GraphBuilder<string>(isDirected: false);
        foreach (var v in original.Vertices)
            mstGraph.AddVertex(v);
        foreach (var e in mst)
            mstGraph.AddEdge(e.From, e.To, e.Weight);

        // Сохраняем в GraphML
        var mstPath = Path.Combine(outputDir, "mst.graphml");
        GraphIO.SaveGraphML(mstGraph.Build(), mstPath);

        Console.WriteLine($"MST рёбер: {mst.Count}");
        Console.WriteLine($"Сохранено: {Path.GetFileName(mstPath)}");
        Console.WriteLine();

        // ============================================================
        //  6. Итог
        // ============================================================

        Section("6. Результат");

        Console.WriteLine($"Все файлы сохранены в: {outputDir}");
        Console.WriteLine();
        Console.WriteLine("Список:");
        foreach (var file in Directory.GetFiles(outputDir)
            .OrderBy(f => f))
        {
            var info = new FileInfo(file);
            Console.WriteLine($"  {info.Name,-20} {info.Length,8} байт");
        }
    }

    // ============================================================
    //  Утилиты
    // ============================================================

    private static void PrintFileInfo(string path, string formatName)
    {
        var info = new FileInfo(path);
        Console.WriteLine($"{formatName,-8} → {info.Name,-20} " +
                          $"({info.Length,6} байт)");
    }

    private static void VerifyRoundTrip<T>(
        Core.IGraph<T> original,
        Core.IGraph<T> loaded,
        string formatName) where T : notnull
    {
        bool verticesMatch = original.VertexCount == loaded.VertexCount;
        bool edgesMatch = original.EdgeCount == loaded.EdgeCount;
        bool directedMatch = original.IsDirected == loaded.IsDirected;

        string status = verticesMatch && edgesMatch && directedMatch
            ? "✓"
            : "✗";

        Console.WriteLine($"  {status} {formatName,-8} " +
                          $"V={loaded.VertexCount}, E={loaded.EdgeCount}, " +
                          $"directed={loaded.IsDirected}");
    }

    private static void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 60));
        Console.WriteLine($"  {title}");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine();
    }
}
