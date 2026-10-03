using System.CommandLine;
using GraphToolkit.Flow;

namespace GraphToolkit.Cli.Commands;

/// <summary>
/// Команда <c>flow</c> — максимальный поток.
/// </summary>
internal static class FlowCommand
{
    public static Command Create()
    {
        var fileArg = GraphLoader.FileArgument();

        var sourceOption = new Option<string>("--source")
        {
            Description = "Исток",
            Required = true
        };

        var sinkOption = new Option<string>("--sink")
        {
            Description = "Сток",
            Required = true
        };

        var algorithmOption = new Option<string>("--algorithm")
        {
            Description = "Алгоритм: dinic | edmonds-karp | ford-fulkerson",
            DefaultValueFactory = _ => "dinic"
        };

        var minCutOption = new Option<bool>("--min-cut")
        {
            Description = "Также вывести минимальный разрез"
        };

        var cmd = new Command("flow", "Максимальный поток")
        {
            Arguments = { fileArg },
            Options = { sourceOption, sinkOption, algorithmOption, minCutOption }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var source = parseResult.GetValue(sourceOption)!;
            var sink = parseResult.GetValue(sinkOption)!;
            var algo = parseResult.GetValue(algorithmOption)!.ToLowerInvariant();
            var minCut = parseResult.GetValue(minCutOption);
            var output = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;
            // Загружаем как обычный граф и строим FlowNetwork
            var graph = GraphLoader.Load(file.FullName);
            var net = BuildFlowNetwork(graph);

            double flow = algo switch
            {
                "dinic" => Dinic.Compute(net.Clone(), source, sink),
                "edmonds-karp" or "ek" => EdmondsKarp.Compute(net.Clone(), source, sink),
                "ford-fulkerson" or "ff" => FordFulkerson.Compute(net.Clone(), source, sink),
                _ => throw new ArgumentException($"Неизвестный алгоритм: {algo}")
            };

            output.WriteLine($"Максимальный поток: {flow}");

            if (minCut)
            {
                var sourceSide = Dinic.MinCut(net.Clone(), source, sink);
                output.WriteLine("Минимальный разрез (сторона источника):");
                output.WriteLine("  {" + string.Join(", ", sourceSide) + "}");
            }

            return 0;
        });

        return cmd;
    }

    internal static Core.FlowNetwork<string> BuildFlowNetwork(
        Core.IGraph<string> graph)
    {
        var net = new Core.FlowNetwork<string>();
        foreach (var v in graph.Vertices) net.AddVertex(v);
        foreach (var e in graph.Edges)
        {
            // Для directed — только прямое направление
            net.AddEdge(e.From, e.To, e.Weight);
            if (!graph.IsDirected)
                net.AddEdge(e.To, e.From, e.Weight);
        }
        return net;
    }
}
