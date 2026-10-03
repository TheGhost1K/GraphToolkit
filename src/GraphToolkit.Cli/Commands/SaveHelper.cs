using GraphToolkit.Core;
using GraphToolkit.IO;
using GraphToolkit.Visualization;

namespace GraphToolkit.Cli.Commands;

/// <summary>
/// Сохраняет граф в файл, определяя формат по расширению.
/// </summary>
internal static class SaveHelper
{
    /// <summary>
    /// Сохраняет граф в файл формата, определяемого расширением.
    /// </summary>
    /// <param name="graph">Граф.</param>
    /// <param name="path">Путь к файлу.</param>
    /// <exception cref="NotSupportedException">
    /// Если расширение не поддерживается.
    /// </exception>
    public static void Save(IGraph<string> graph, string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        switch (ext)
        {
            case ".graphml":
                GraphIO.SaveGraphML(graph, path);
                break;
            case ".gexf":
                GraphIO.SaveGexf(graph, path);
                break;
            case ".json":
                GraphIO.SaveJson(graph, path);
                break;
            case ".csv":
                GraphIO.SaveCsv(graph, path);
                break;
            case ".dot" or ".gv":
                File.WriteAllText(path, GraphExporters.ToDot(graph));
                break;
            default:
                throw new NotSupportedException(
                    $"Формат '{ext}' не поддерживается для сохранения. " +
                    "Используйте: .graphml, .gexf, .json, .csv, .dot");
        }
    }

    /// <summary>
    /// Строит граф из списка рёбер (для сохранения результатов).
    /// </summary>
    public static Graph<string> BuildFromEdges(
        IEnumerable<(string From, string To, double Weight)> edges,
        bool isDirected = false)
    {
        var g = new Graph<string>(isDirected);
        foreach (var (from, to, w) in edges)
            g.AddEdge(from, to, w);
        return g;
    }
}
