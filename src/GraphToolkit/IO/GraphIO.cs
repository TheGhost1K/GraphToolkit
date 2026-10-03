using GraphToolkit.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace GraphToolkit.IO;

/// <summary>
/// Импорт и экспорт графов в стандартные форматы.
/// </summary>
/// <remarks>
/// <para>Поддерживаемые форматы:</para>
/// <list type="bullet">
///   <item><b>GraphML</b> — XML-стандарт для соцсетей и Gephi.</item>
///   <item><b>GEXF</b> — формат Gephi.</item>
///   <item><b>JSON</b> — собственный формат для быстрого обмена.</item>
///   <item><b>DOT</b> — GraphViz (импорт и экспорт).</item>
///   <item><b>CSV</b> — список рёбер (source,target[,weight]).</item>
/// </list>
/// </remarks>
public static class GraphIO
{
    // ============================================================
    //  GraphML
    // ============================================================

    /// <summary>
    /// Сохраняет граф в формате GraphML.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Граф.</param>
    /// <param name="path">Путь к файлу.</param>
    public static void SaveGraphML<T>(IGraph<T> graph, string path)
        where T : notnull
    {
        var ns = XNamespace.Get("http://graphml.graphdrawing.org/xmlns");

        var root = new XElement(ns + "graphml",
            new XAttribute("xmlns", ns.NamespaceName),
            new XElement(ns + "key",
                new XAttribute("id", "w"),
                new XAttribute("for", "edge"),
                new XAttribute("attr.name", "weight"),
                new XAttribute("attr.type", "double")),
            new XElement(ns + "graph",
                new XAttribute("id", "G"),
                new XAttribute("edgedefault",
                    graph.IsDirected ? "directed" : "undirected"),
                graph.Vertices.Select(v =>
                    new XElement(ns + "node",
                        new XAttribute("id", v.ToString()!))),
                graph.Edges.Select(e =>
                    new XElement(ns + "edge",
                        new XAttribute("source", e.From.ToString()!),
                        new XAttribute("target", e.To.ToString()!),
                        new XElement(ns + "data",
                            new XAttribute("key", "w"),
                            e.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture))))));

        new XDocument(root).Save(path);
    }

    /// <summary>
    /// Загружает граф из GraphML.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="path">Путь к файлу.</param>
    /// <param name="parseVertex">Функция преобразования строки в вершину.</param>
    /// <returns>Загруженный граф.</returns>
    public static Graph<T> LoadGraphML<T>(
        string path,
        Func<string, T> parseVertex) where T : notnull
    {
        var doc = XDocument.Load(path);
        var ns = doc.Root!.GetDefaultNamespace();

        var graphElement = doc.Descendants(ns + "graph").First();
        bool directed = graphElement.Attribute("edgedefault")?.Value == "directed";

        var graph = new Graph<T>(directed);
        var vertexMap = new Dictionary<string, T>();

        foreach (var node in graphElement.Elements(ns + "node"))
        {
            string id = node.Attribute("id")!.Value;
            var v = parseVertex(id);
            vertexMap[id] = v;
            graph.AddVertex(v);
        }

        foreach (var edge in graphElement.Elements(ns + "edge"))
        {
            string src = edge.Attribute("source")!.Value;
            string tgt = edge.Attribute("target")!.Value;
            double w = 1.0;

            var data = edge.Elements(ns + "data").FirstOrDefault();
            if (data is not null && double.TryParse(data.Value,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                w = parsed;

            graph.AddEdge(vertexMap[src], vertexMap[tgt], w);
        }

        return graph;
    }

    // ============================================================
    //  GEXF
    // ============================================================

    /// <summary>
    /// Сохраняет граф в формате GEXF (Gephi).
    /// </summary>
    public static void SaveGexf<T>(IGraph<T> graph, string path)
        where T : notnull
    {
        var ns = XNamespace.Get("http://www.gexf.net/1.2draft");

        var nodeIdMap = graph.Vertices
            .Select((v, i) => (v, i))
            .ToDictionary(x => x.v, x => x.i);

        var root = new XElement(ns + "gexf",
            new XAttribute("xmlns", ns.NamespaceName),
            new XAttribute("version", "1.2"),
            new XElement(ns + "graph",
                new XAttribute("mode", "static"),
                new XAttribute("defaultedgetype",
                    graph.IsDirected ? "directed" : "undirected"),
                new XElement(ns + "nodes",
                    graph.Vertices.Select(v =>
                        new XElement(ns + "node",
                            new XAttribute("id", nodeIdMap[v]),
                            new XAttribute("label", v.ToString()!)))),
                new XElement(ns + "edges",
                    graph.Edges.Select(e =>
                        new XElement(ns + "edge",
                            new XAttribute("source", nodeIdMap[e.From]),
                            new XAttribute("target", nodeIdMap[e.To]),
                            new XAttribute("weight",
                                e.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture)))))));

        new XDocument(root).Save(path);
    }

    /// <summary>
    /// Загружает граф из GEXF.
    /// </summary>
    public static Graph<T> LoadGexf<T>(
        string path,
        Func<string, T> parseVertex) where T : notnull
    {
        var doc = XDocument.Load(path);
        var ns = doc.Root!.GetDefaultNamespace();

        var graphElement = doc.Descendants(ns + "graph").First();
        bool directed = graphElement.Attribute("defaultedgetype")?.Value == "directed";
        var graph = new Graph<T>(directed);

        var idMap = new Dictionary<int, T>();

        foreach (var node in graphElement.Descendants(ns + "node"))
        {
            int id = int.Parse(node.Attribute("id")!.Value);
            string label = node.Attribute("label")?.Value ?? id.ToString();
            var v = parseVertex(label);
            idMap[id] = v;
            graph.AddVertex(v);
        }

        foreach (var edge in graphElement.Descendants(ns + "edge"))
        {
            int src = int.Parse(edge.Attribute("source")!.Value);
            int tgt = int.Parse(edge.Attribute("target")!.Value);
            double w = double.TryParse(edge.Attribute("weight")?.Value,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var parsed)
                ? parsed : 1.0;

            graph.AddEdge(idMap[src], idMap[tgt], w);
        }

        return graph;
    }

    // ============================================================
    //  JSON
    // ============================================================

    private sealed class JsonGraph
    {
        [JsonPropertyName("directed")] public bool Directed { get; set; }
        [JsonPropertyName("vertices")] public List<string> Vertices { get; set; } = new();
        [JsonPropertyName("edges")] public List<JsonEdge> Edges { get; set; } = new();
    }

    private sealed class JsonEdge
    {
        [JsonPropertyName("from")] public string From { get; set; } = "";
        [JsonPropertyName("to")] public string To { get; set; } = "";
        [JsonPropertyName("weight")] public double Weight { get; set; }
    }

    /// <summary>
    /// Сохраняет граф в JSON.
    /// </summary>
    public static void SaveJson<T>(IGraph<T> graph, string path, bool indented = true)
        where T : notnull
    {
        var jsonGraph = new JsonGraph
        {
            Directed = graph.IsDirected,
            Vertices = graph.Vertices.Select(v => v.ToString()!).ToList(),
            Edges = graph.Edges
                .Select(e => new JsonEdge
                {
                    From = e.From.ToString()!,
                    To = e.To.ToString()!,
                    Weight = e.Weight
                })
                .ToList()
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = indented,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        File.WriteAllText(path, JsonSerializer.Serialize(jsonGraph, options));
    }

    /// <summary>
    /// Загружает граф из JSON.
    /// </summary>
    public static Graph<T> LoadJson<T>(
        string path,
        Func<string, T> parseVertex) where T : notnull
    {
        var text = File.ReadAllText(path);
        var jsonGraph = JsonSerializer.Deserialize<JsonGraph>(text)
            ?? throw new InvalidOperationException("Не удалось прочитать JSON");

        var graph = new Graph<T>(jsonGraph.Directed);
        foreach (var v in jsonGraph.Vertices)
            graph.AddVertex(parseVertex(v));

        foreach (var e in jsonGraph.Edges)
            graph.AddEdge(parseVertex(e.From), parseVertex(e.To), e.Weight);

        return graph;
    }

    // ============================================================
    //  CSV
    // ============================================================

    /// <summary>
    /// Сохраняет рёбра графа в CSV.
    /// </summary>
    /// <remarks>
    /// Формат: <c>source,target,weight</c>. Опционально — заголовок.
    /// </remarks>
    public static void SaveCsv<T>(
        IGraph<T> graph, string path, bool writeHeader = true)
        where T : notnull
    {
        using var writer = new StreamWriter(path);
        if (writeHeader)
            writer.WriteLine("source,target,weight");

        foreach (var e in graph.Edges)
        {
            writer.Write(e.From);
            writer.Write(',');
            writer.Write(e.To);
            writer.Write(',');
            writer.WriteLine(e.Weight.ToString(
                System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// Загружает граф из CSV.
    /// </summary>
    /// <remarks>
    /// Ожидается формат <c>source,target[,weight]</c>.
    /// Первая строка может быть заголовком — определяется по наличию слова "source".
    /// </remarks>
    public static Graph<T> LoadCsv<T>(
        string path,
        Func<string, T> parseVertex,
        bool isDirected = false) where T : notnull
    {
        var graph = new Graph<T>(isDirected);
        var lines = File.ReadAllLines(path);

        int start = 0;
        if (lines.Length > 0 &&
            lines[0].StartsWith("source", StringComparison.OrdinalIgnoreCase))
            start = 1;

        for (int i = start; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var parts = line.Split(',');
            if (parts.Length < 2) continue;

            var from = parseVertex(parts[0].Trim());
            var to = parseVertex(parts[1].Trim());
            double weight = parts.Length >= 3
                && double.TryParse(parts[2].Trim(),
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var w)
                ? w : 1.0;

            graph.AddEdge(from, to, weight);
        }

        return graph;
    }

    // ============================================================
    //  DOT (импорт)
    // ============================================================

    /// <summary>
    /// Загружает граф из простого DOT-файла.
    /// </summary>
    /// <remarks>
    /// Поддерживает простые конструкции: <c>digraph/graph</c>, рёбра
    /// <c>a -> b [label="5"]</c> или <c>a -- b</c>. Атрибуты вершин игнорируются.
    /// Для сложных DOT-файлов используйте специализированные парсеры.
    /// </remarks>
    public static Graph<T> LoadDot<T>(
        string path,
        Func<string, T> parseVertex) where T : notnull
    {
        var content = File.ReadAllText(path);
        bool directed = content.Contains("digraph", StringComparison.OrdinalIgnoreCase);
        var graph = new Graph<T>(directed);

        var edgeRegex = new System.Text.RegularExpressions.Regex(
            @"""?([\w\d_]+)""?\s*(->|--)\s*""?([\w\d_]+)""?(\s*\[.*?label\s*=\s*""?([\d.]+)""?.*?\])?",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (System.Text.RegularExpressions.Match m in edgeRegex.Matches(content))
        {
            var from = parseVertex(m.Groups[1].Value);
            var to = parseVertex(m.Groups[3].Value);
            double w = m.Groups[5].Success
                && double.TryParse(m.Groups[5].Value,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var parsed)
                ? parsed : 1.0;

            graph.AddEdge(from, to, w);
        }

        return graph;
    }
}
