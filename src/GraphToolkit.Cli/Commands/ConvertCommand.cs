using System.CommandLine;
using GraphToolkit.Core;
using GraphToolkit.IO;
using GraphToolkit.Visualization;

namespace GraphToolkit.Cli.Commands;

/// <summary>
/// Команда <c>convert</c> — конвертация графа между форматами.
/// </summary>
internal static class ConvertCommand
{
    public static Command Create()
    {
        var fileArg = GraphLoader.FileArgument();

        var toOption = new Option<string>("--to")
        {
            Description = "Целевой формат: graphml | gexf | json | csv | dot",
            Required = true
        };

        var outputOption = new Option<FileInfo?>("--output")
        {
            Description = "Путь к выходному файлу (по умолчанию — <input>.<to>)"
        };

        var cmd = new Command("convert", "Конвертировать граф между форматами")
        {
            Arguments = { fileArg },
            Options = { toOption, outputOption }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var to = parseResult.GetValue(toOption)!;
            var output = parseResult.GetValue(outputOption);
            var outputConsole = parseResult.InvocationConfiguration.Output;

            var graph = GraphLoader.Load(file.FullName);

            var outPath = output?.FullName
                ?? Path.ChangeExtension(file.FullName, "." + to.ToLowerInvariant());

            SaveAs(graph, to.ToLowerInvariant(), outPath);
            outputConsole.WriteLine($"Конвертировано: {file.Name} → {Path.GetFileName(outPath)}");
            return 0;
        });

        return cmd;
    }

    internal static void SaveAs(IGraph<string> graph, string format, string path)
    {
        switch (format)
        {
            case "graphml": GraphIO.SaveGraphML(graph, path); break;
            case "gexf": GraphIO.SaveGexf(graph, path); break;
            case "json": GraphIO.SaveJson(graph, path); break;
            case "csv": GraphIO.SaveCsv(graph, path); break;
            case "dot" or "gv":
                File.WriteAllText(path, GraphExporters.ToDot(graph));
                break;
            default:
                throw new ArgumentException(
                    $"Неизвестный формат '{format}'. " +
                    $"Используйте: graphml | gexf | json | csv | dot");
        }
    }
}
