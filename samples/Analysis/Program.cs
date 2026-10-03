using GraphToolkit.Centrality;
using GraphToolkit.Community;
using GraphToolkit.Core;
using GraphToolkit.IO;
using GraphToolkit.Utils;
using GraphToolkit.Visualization;

namespace GraphToolkit.Samples.Analysis;

/// <summary>
/// Пример: анализ социальной сети через центральности, PageRank
/// и обнаружение сообществ.
/// </summary>
/// <remarks>
/// <para>
/// Демонстрирует полный пайплайн анализа:
/// </para>
/// <list type="number">
///   <item>Загрузка графа из JSON.</item>
///   <item>Общая информация о графе.</item>
///   <item>PageRank — важность пользователей.</item>
///   <item>Degree, Closeness, Betweenness — разные аспекты центральности.</item>
///   <item>Louvain — обнаружение сообществ.</item>
///   <item>Сравнение сообществ с PageRank.</item>
///   <item>Визуализация в Mermaid с раскраской по сообществам.</item>
/// </list>
/// </remarks>
public static class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var dataPath = Path.Combine(
            AppContext.BaseDirectory, "data", "social-network.json");

        if (!File.Exists(dataPath))
        {
            Console.Error.WriteLine($"Файл не найден: {dataPath}");
            Console.Error.WriteLine("Убедитесь, что data/social-network.json " +
                                    "копируется в выходную папку.");
            return;
        }

        // ============================================================
        //  1. Загрузка графа
        // ============================================================

        Section("1. Загрузка графа");

        var graph = GraphIO.LoadJson(dataPath, s => s);

        Console.WriteLine($"Загружено из: {Path.GetFileName(dataPath)}");
        Console.WriteLine($"Вершин:       {graph.VertexCount}");
        Console.WriteLine($"Рёбер:        {graph.EdgeCount}");
        Console.WriteLine($"Ориентирован: {(graph.IsDirected ? "да" : "нет")}");

        // ============================================================
        //  2. Общая статистика
        // ============================================================

        Section("2. Общая статистика");

        double avgDegree = graph.Vertices.Average(v => graph.Neighbors(v).Count());
        double maxWeight = graph.Edges.Max(e => e.Weight);
        double minWeight = graph.Edges.Min(e => e.Weight);

        Console.WriteLine($"Средняя степень: {avgDegree:F2}");
        Console.WriteLine($"Диапазон весов:  [{minWeight:F2}, {maxWeight:F2}]");

        // ============================================================
        //  3. PageRank
        // ============================================================

        Section("3. PageRank — важность пользователей");

        var pageRank = PageRank.Compute(graph, damping: 0.85, iterations: 100);

        PrintRanking(pageRank, "PageRank", percent: true);

        // ============================================================
        //  4. Классические центральности
        // ============================================================

        Section("4. Центральности");

        var degree = Centralities.Degree(graph);
        var closeness = Centralities.Closeness(graph);
        var betweenness = Centralities.Betweenness(graph);

        Console.WriteLine("Degree (кто со сколькими связан):");
        PrintRanking(degree, top: 5);

        Console.WriteLine("\nCloseness (кто ближе ко всем):");
        PrintRanking(closeness, top: 5);

        Console.WriteLine("\nBetweenness (кто «мост» между группами):");
        PrintRanking(betweenness, top: 5);

        // ============================================================
        //  5. Обнаружение сообществ
        // ============================================================

        Section("5. Обнаружение сообществ (Louvain)");

        var communities = Louvain.Compute(graph);
        double modularity = Louvain.Modularity(graph, communities);

        Console.WriteLine($"Найдено сообществ: {communities.Count}");
        Console.WriteLine($"Модулярность:      {modularity:F4}");
        Console.WriteLine();

        for (int i = 0; i < communities.Count; i++)
        {
            var members = communities[i]
                .OrderByDescending(m => pageRank[m])
                .ToList();

            Console.WriteLine($"  Сообщество #{i + 1} ({members.Count} чел.):");
            Console.WriteLine($"    " + string.Join(", ", members));
        }

        // ============================================================
        //  6. Сравнение: PageRank внутри сообществ
        // ============================================================

        Section("6. Лидеры каждого сообщества");

        for (int i = 0; i < communities.Count; i++)
        {
            var leader = communities[i]
                .OrderByDescending(m => pageRank[m])
                .First();

            Console.WriteLine($"  Сообщество #{i + 1}: лидер — " +
                              $"{leader} (PageRank {pageRank[leader]:P2})");
        }

        // ============================================================
        //  7. Комбинированный рейтинг
        // ============================================================

        Section("7. Комбинированный рейтинг «влиятельности»");

        // 50% PageRank + 30% Betweenness + 20% Closeness
        var combined = graph.Vertices.ToDictionary(
            v => v,
            v => 0.5 * pageRank[v]
               + 0.3 * betweenness[v]
               + 0.2 * closeness[v]);

        Console.WriteLine("Веса: 50% PageRank + 30% Betweenness + 20% Closeness");
        Console.WriteLine();
        PrintRanking(combined, top: 10);

        // ============================================================
        //  8. Проверка «мостов» между группами
        // ============================================================

        Section("8. Мосты между сообществами");

        var bridgeScore = new Dictionary<string, double>();
        var vertexCommunity = new Dictionary<string, int>();

        for (int i = 0; i < communities.Count; i++)
            foreach (var v in communities[i])
                vertexCommunity[v] = i;

        foreach (var v in graph.Vertices)
        {
            int ownCommunity = vertexCommunity[v];
            int externalEdges = graph.Neighbors(v)
                .Count(e => vertexCommunity[e.To] != ownCommunity);

            if (externalEdges > 0)
                bridgeScore[v] = externalEdges;
        }

        if (bridgeScore.Count == 0)
        {
            Console.WriteLine("Нет межгрупповых связей.");
        }
        else
        {
            foreach (var (user, count) in bridgeScore.OrderByDescending(kv => kv.Value))
                Console.WriteLine($"  {user,-8} — {count} связей с другими группами");
        }

        // ============================================================
        //  9. Визуализация
        // ============================================================

        Section("9. Визуализация (Mermaid)");

        // Раскрашиваем вершины по сообществам
        var palette = new[]
        {
            "#FFE0E0", "#E0FFE0", "#E0E0FF",
            "#FFEECC", "#CCFFEE", "#EECCFF"
        };

        var colors = new Dictionary<string, string>();
        for (int i = 0; i < communities.Count; i++)
            foreach (var v in communities[i])
                colors[v] = palette[i % palette.Length];

        var mermaid = GraphExporters.ToMermaid(graph, colors);

        Console.WriteLine(mermaid);

        // ============================================================
        //  10. Экспорт результатов
        // ============================================================

        Section("10. Экспорт результатов");

        var outputPath = Path.Combine(
            AppContext.BaseDirectory, "analysis-result.json");

        var result = new AnalysisResult
        {
            Vertices = graph.VertexCount,
            Edges = graph.EdgeCount,
            Modularity = modularity,
            Communities = communities.Select(c => c.ToList()).ToList(),
            PageRank = pageRank
                .OrderByDescending(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value),
            Betweenness = betweenness
                .OrderByDescending(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value),
            Combined = combined
                .OrderByDescending(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value)
        };

        var json = System.Text.Json.JsonSerializer.Serialize(
            result,
            new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder
                    .UnsafeRelaxedJsonEscaping
            });

        File.WriteAllText(outputPath, json);
        Console.WriteLine($"Результаты сохранены: {outputPath}");

        Section("Готово");
    }

    // ============================================================
    //  Утилиты вывода
    // ============================================================

    private static void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 60));
        Console.WriteLine($"  {title}");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine();
    }

    private static void PrintRanking(
        Dictionary<string, double> ranking,
        string label = "",
        int top = 10,
        bool percent = false)
    {
        if (!string.IsNullOrEmpty(label))
            Console.WriteLine($"{label}:");

        foreach (var (name, value) in ranking.OrderByDescending(kv => kv.Value).Take(top))
        {
            var formatted = percent ? $"{value:P2}" : $"{value:F4}";
            Console.WriteLine($"  {name,-8} {formatted}");
        }
    }
}

/// <summary>
/// Структура для сериализации результатов в JSON.
/// </summary>
internal sealed class AnalysisResult
{
    public int Vertices { get; set; }
    public int Edges { get; set; }
    public double Modularity { get; set; }
    public List<List<string>> Communities { get; set; } = new();
    public Dictionary<string, double> PageRank { get; set; } = new();
    public Dictionary<string, double> Betweenness { get; set; } = new();
    public Dictionary<string, double> Combined { get; set; } = new();
}
