using System.CommandLine;
using GraphToolkit.Core;
using GraphToolkit.IO;

namespace GraphToolkit.Cli.Commands;

/// <summary>
/// Загружает граф из файла, определяя формат по расширению.
/// </summary>
internal static class GraphLoader
{
    /// <summary>
    /// Общий аргумент команды — путь к файлу.
    /// </summary>
    public static Argument<FileInfo> FileArgument() =>
        new("file") { Description = "Файл с графом" };

    public static Graph<string> Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Файл не найден: {path}", path);

        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".graphml" => GraphIO.LoadGraphML(path, s => s),
            ".gexf" => GraphIO.LoadGexf(path, s => s),
            ".json" => GraphIO.LoadJson(path, s => s),
            ".csv" => GraphIO.LoadCsv(path, s => s),
            ".dot" or ".gv" => GraphIO.LoadDot(path, s => s),
            _ => throw new NotSupportedException(
                $"Формат '{ext}' не поддерживается. Используйте: .dot, .graphml, .gexf, .json, .csv")
        };
    }
}
