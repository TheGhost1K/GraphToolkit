using System.CommandLine;
using GraphToolkit.Traversal;

namespace GraphToolkit.Cli.Commands;

/// <summary>
/// Команда <c>traversal</c> — обходы и топологическая сортировка.
/// </summary>
internal static class TraversalCommand
{
    public static Command Create()
    {
        var fileArg = GraphLoader.FileArgument();

        var algorithmOption = new Option<string>("--algorithm")
        {
            Description = "Алгоритм: bfs | dfs | topo | levels",
            DefaultValueFactory = _ => "bfs"
        };

        var startOption = new Option<string?>("--start")
        {
            Description = "Начальная вершина (для bfs/dfs)"
        };

        var endOption = new Option<string?>("--end")
        {
            Description = "Конечная вершина (для bfs/dfs)"
        };

        var cmd = new Command("traversal", "Обходы графа и топосортировка")
        {
            Arguments = { fileArg },
            Options = { algorithmOption, startOption, endOption }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var algo = parseResult.GetValue(algorithmOption)!.ToLowerInvariant();
            var start = parseResult.GetValue(startOption);
            var end = parseResult.GetValue(endOption);
            var output = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;
            var graph = GraphLoader.Load(file.FullName);

            switch (algo)
            {
                case "bfs":
                case "dfs":
                    if (start is null || end is null)
                    {
                        error.WriteLine(
                            "--start и --end обязательны для bfs/dfs");
                        return 1;
                    }

                    var path = algo == "bfs"
                        ? Bfs.FindPath(graph, start, end)
                        : Dfs.FindPath(graph, start, end);

                    if (path is null)
                    {
                        error.WriteLine($"Путь {start} → {end} не найден");
                        return 1;
                    }
                    output.WriteLine(string.Join(" -> ", path));
                    return 0;

                case "topo":
                case "toposort":
                    var order = TopologicalSort.Sort(graph);
                    if (order is null)
                    {
                        error.WriteLine("Граф содержит цикл — топосорт невозможен");
                        return 1;
                    }
                    output.WriteLine(string.Join(" -> ", order));
                    return 0;

                case "levels":
                    try
                    {
                        var levels = TopologicalSort.ComputeLevels(graph);
                        foreach (var group in levels
                            .GroupBy(kv => kv.Value)
                            .OrderBy(g => g.Key))
                        {
                            output.WriteLine($"Уровень {group.Key}: " +
                                string.Join(", ", group.Select(kv => kv.Key)));
                        }
                        return 0;
                    }
                    catch (InvalidOperationException)
                    {
                        error.WriteLine("Граф содержит цикл — уровни невозможны");
                        return 1;
                    }

                default:
                    error.WriteLine($"Неизвестный алгоритм: {algo}");
                    return 1;
            }
        });

        return cmd;
    }
}
