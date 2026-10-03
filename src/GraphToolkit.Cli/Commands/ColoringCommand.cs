using System.CommandLine;
using GraphToolkit.Coloring;

namespace GraphToolkit.Cli.Commands;

/// <summary>
/// Команда <c>coloring</c> — раскраска графа.
/// </summary>
internal static class ColoringCommand
{
    public static Command Create()
    {
        var fileArg = GraphLoader.FileArgument();

        var algorithmOption = new Option<string>("--algorithm")
        {
            Description = "Алгоритм: greedy | exact | bipartite",
            DefaultValueFactory = _ => "greedy"
        };

        var cmd = new Command("coloring", "Раскраска графа")
        {
            Arguments = { fileArg },
            Options = { algorithmOption }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var algo = parseResult.GetValue(algorithmOption)!.ToLowerInvariant();
            var output = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;
            var graph = GraphLoader.Load(file.FullName);

            switch (algo)
            {
                case "greedy":
                    var colors = GraphColoring.Greedy(graph);
                    int numColors = colors.Values.Max() + 1;
                    output.WriteLine($"Цветов: {numColors}");
                    foreach (var (v, c) in colors.OrderBy(kv => kv.Value))
                        output.WriteLine($"  {v,-12} цвет {c}");
                    return 0;

                case "exact":
                    var (exactColors, chromatic) = GraphColoring.Exact(graph);
                    output.WriteLine($"Хроматическое число χ(G) = {chromatic}");
                    foreach (var (v, c) in exactColors.OrderBy(kv => kv.Value))
                        output.WriteLine($"  {v,-12} цвет {c}");
                    return 0;

                case "bipartite":
                    bool isBip = GraphColoring.IsBipartite(graph, out var parts);
                    output.WriteLine($"Двудольный: {isBip}");
                    if (isBip)
                    {
                        output.WriteLine("Часть A: " +
                            string.Join(", ",
                                parts.Where(kv => kv.Value == 0).Select(kv => kv.Key)));
                        output.WriteLine("Часть B: " +
                            string.Join(", ",
                                parts.Where(kv => kv.Value == 1).Select(kv => kv.Key)));
                    }
                    return 0;

                default:
                    error.WriteLine($"Неизвестный алгоритм: {algo}");
                    return 1;
            }
        });

        return cmd;
    }
}
