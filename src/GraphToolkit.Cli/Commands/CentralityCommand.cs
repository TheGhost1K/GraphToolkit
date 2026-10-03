using System.CommandLine;
using GraphToolkit.Centrality;

namespace GraphToolkit.Cli.Commands;

/// <summary>
/// Команда <c>centrality</c> — центральности и PageRank.
/// </summary>
internal static class CentralityCommand
{
    public static Command Create()
    {
        var fileArg = GraphLoader.FileArgument();

        var metricOption = new Option<string>("--metric")
        {
            Description = "Метрика: pagerank | degree | closeness | betweenness | eigenvector | katz",
            DefaultValueFactory = _ => "pagerank"
        };

        var topOption = new Option<int>("--top")
        {
            Description = "Сколько верхних результатов показать",
            DefaultValueFactory = _ => 10
        };

        var cmd = new Command("centrality", "Центральности и PageRank")
        {
            Arguments = { fileArg },
            Options = { metricOption, topOption }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var metric = parseResult.GetValue(metricOption)!;
            var top = parseResult.GetValue(topOption);

            var output = parseResult.InvocationConfiguration.Output;
            var graph = GraphLoader.Load(file.FullName);

            var scores = metric.ToLowerInvariant() switch
            {
                "pagerank" or "pr" => PageRank.Compute(graph),
                "degree" or "deg" => Centralities.Degree(graph),
                "closeness" or "clo" => Centralities.Closeness(graph),
                "betweenness" or "bet" => Centralities.Betweenness(graph),
                "eigenvector" or "eig" => Centralities.Eigenvector(graph),
                "katz" => Centralities.Katz(graph),
                _ => throw new ArgumentException($"Неизвестная метрика: {metric}")
            };

            output.WriteLine($"{metric} (топ {top}):");
            foreach (var (v, score) in scores
                .OrderByDescending(kv => kv.Value)
                .Take(top))
            {
                output.WriteLine($"  {v,-12} {score:F6}");
            }

            return 0;
        });

        return cmd;
    }
}
