using System.CommandLine;
using GraphToolkit.Community;

namespace GraphToolkit.Cli.Commands;

/// <summary>
/// Команда <c>community</c> — обнаружение сообществ.
/// </summary>
internal static class CommunityCommand
{
    public static Command Create()
    {
        var fileArg = GraphLoader.FileArgument();

        var algorithmOption = new Option<string>("--algorithm")
        {
            Description = "Алгоритм: louvain | label-propagation",
            DefaultValueFactory = _ => "louvain"
        };

        var seedOption = new Option<int?>("--seed")
        {
            Description = "Seed для воспроизводимости (только label-propagation)"
        };

        var cmd = new Command("community", "Обнаружение сообществ")
        {
            Arguments = { fileArg },
            Options = { algorithmOption, seedOption }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var algo = parseResult.GetValue(algorithmOption)!;
            var seed = parseResult.GetValue(seedOption);
            var output = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;
            var graph = GraphLoader.Load(file.FullName);

            List<List<string>> communities;
            double? modularity = null;

            switch (algo.ToLowerInvariant())
            {
                case "louvain":
                case "louv":
                    communities = Louvain.Compute(graph);
                    modularity = Louvain.Modularity(graph, communities);
                    break;

                case "label-propagation":
                case "label":
                case "lp":
                    communities = LabelPropagation.Compute(graph, seed: seed);
                    break;

                default:
                    throw new ArgumentException($"Неизвестный алгоритм: {algo}");
            }

            output.WriteLine($"Алгоритм: {algo}");
            output.WriteLine($"Найдено сообществ: {communities.Count}");
            if (modularity.HasValue)
                output.WriteLine($"Модулярность: {modularity.Value:F4}");
            output.WriteLine();

            for (int i = 0; i < communities.Count; i++)
            {
                output.WriteLine($"  #{i + 1} ({communities[i].Count}): " +
                                  string.Join(", ", communities[i]));
            }

            return 0;
        });

        return cmd;
    }
}
