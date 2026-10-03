using System.CommandLine;
using GraphToolkit.ShortestPaths;
using GraphToolkit.Traversal;

namespace GraphToolkit.Cli.Commands;

/// <summary>
/// Команда <c>path</c> — поиск пути между двумя вершинами.
/// </summary>
internal static class PathCommand
{
    public static Command Create()
    {
        var fileArg = GraphLoader.FileArgument();

        var fromOption = new Option<string>("--from")
        {
            Description = "Начальная вершина",
            Required = true
        };

        var toOption = new Option<string>("--to")
        {
            Description = "Конечная вершина",
            Required = true
        };

        var algorithmOption = new Option<string>("--algorithm")
        {
            Description = "Алгоритм: dijkstra | bfs | dfs | bellman-ford | astar",
            DefaultValueFactory = _ => "dijkstra"
        };

        var cmd = new Command("path", "Найти путь между двумя вершинами")
        {
            Arguments = { fileArg },
            Options = { fromOption, toOption, algorithmOption }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var from = parseResult.GetValue(fromOption)!;
            var to = parseResult.GetValue(toOption)!;
            var algo = parseResult.GetValue(algorithmOption)!;
            var output = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;
            var graph = GraphLoader.Load(file.FullName);

            List<string>? path = algo.ToLowerInvariant() switch
            {
                "dijkstra" => Dijkstra.FindPath(graph, from, to),
                "bfs" => Bfs.FindPath(graph, from, to),
                "dfs" => Dfs.FindPath(graph, from, to),
                "bellman-ford" or "bf" => FindBellmanFordPath(graph, from, to),
                "astar" or "a*" => AStar.FindPath(graph, from, to, (_, _) => 0),
                _ => throw new ArgumentException($"Неизвестный алгоритм: {algo}")
            };

            if (path is null)
            {
                error.WriteLine($"Путь {from} → {to} не найден.");
                return 1;
            }

            output.WriteLine(string.Join(" -> ", path));
            return 0;
        });

        return cmd;
    }

    private static List<string>? FindBellmanFordPath(
        Core.IGraph<string> graph, string from, string to)
    {
        var (dist, prev, hasCycle) = BellmanFord.Compute(graph, from);
        if (hasCycle)
            throw new InvalidOperationException("Обнаружен отрицательный цикл");
        if (double.IsPositiveInfinity(dist[to])) return null;

        var path = new List<string> { to };
        var current = to;
        while (current != from)
        {
            current = prev[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }
}
