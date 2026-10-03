using System.CommandLine;
using GraphToolkit.Core;
using GraphToolkit.Matching;

namespace GraphToolkit.Cli.Commands;

/// <summary>
/// Команда <c>matching</c> — паросочетания.
/// </summary>
internal static class MatchingCommand
{
    public static Command Create()
    {
        var fileArg = GraphLoader.FileArgument();

        var algorithmOption = new Option<string>("--algorithm")
        {
            Description = "Алгоритм: blossom (общий граф) | kuhn (двудольный, через bipartite)",
            DefaultValueFactory = _ => "blossom"
        };

        var leftOption = new Option<string?>("--left")
        {
            Description = "Левая доля (для kuhn), через запятую: A,B,C"
        };

        var cmd = new Command("matching", "Паросочетания")
        {
            Arguments = { fileArg },
            Options = { algorithmOption, leftOption }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var algo = parseResult.GetValue(algorithmOption)!.ToLowerInvariant();
            var leftRaw = parseResult.GetValue(leftOption);
            var output = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;
            var graph = GraphLoader.Load(file.FullName);

            switch (algo)
            {
                case "blossom":
                    var result = Blossom.Compute(graph);
                    output.WriteLine($"Паросочетаний: {result.Count}");
                    foreach (var (a, b) in result)
                        output.WriteLine($"  {a} ↔ {b}");
                    if (Blossom.HasPerfectMatching(graph))
                        output.WriteLine("  (совершенное)");
                    return 0;

                case "kuhn":
                    if (leftRaw is null)
                    {
                        error.WriteLine("--left обязателен для kuhn");
                        return 1;
                    }
                    var left = leftRaw.Split(',', StringSplitOptions.TrimEntries)
                        .Where(s => s.Length > 0)
                        .ToList();

                    var adjacency = graph.Vertices.ToDictionary(
                        v => v,
                        v => graph.Neighbors(v)
                            .Select(e => e.To)
                            .Where(u => !left.Contains(u))
                            .ToList());

                    var matching = Kuhn.Compute(left, u => adjacency[u]);

                    output.WriteLine($"Паросочетаний: {matching.Count}");
                    foreach (var (l, r) in matching)
                        output.WriteLine($"  {l} ↔ {r}");
                    return 0;

                default:
                    error.WriteLine($"Неизвестный алгоритм: {algo}");
                    return 1;
            }
        });

        return cmd;
    }
}
