using System.CommandLine;
using GraphToolkit.Visualization;

namespace GraphToolkit.Cli.Commands;

internal static class VisualizeCommand
{
    public static Command Create()
    {
        var fileArg = GraphLoader.FileArgument();

        var formatOption = new Option<string>("--format")
        {
            Description = "Формат: dot | mermaid | matrix | list",
            DefaultValueFactory = _ => "dot"
        };

        var outputOption = new Option<FileInfo?>("--output")
        {
            Description = "Сохранить результат в файл"
        };

        var cmd = new Command("visualize", "Экспорт графа в текстовый формат")
        {
            Arguments = { fileArg },
            Options = { formatOption, outputOption }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var format = parseResult.GetValue(formatOption)!;
            var output = parseResult.GetValue(outputOption);
            var outputConsole = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;
            var graph = GraphLoader.Load(file.FullName);

            string result = format.ToLowerInvariant() switch
            {
                "dot" => GraphExporters.ToDot(graph, "G"),
                "mermaid" => GraphExporters.ToMermaid(graph),
                "matrix" => GraphExporters.ToAdjacencyMatrix(graph),
                "list" => GraphExporters.ToAdjacencyList(graph),
                _ => throw new ArgumentException($"Неизвестный формат: {format}")
            };

            if (output is not null)
            {
                File.WriteAllText(output.FullName, result);
                outputConsole.WriteLine($"Сохранено в {output.FullName}");
            }
            else
            {
                outputConsole.WriteLine(result);
            }

            return 0;
        });

        return cmd;
    }
}
