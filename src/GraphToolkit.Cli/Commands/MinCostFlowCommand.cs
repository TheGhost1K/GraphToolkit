using System.CommandLine;
using GraphToolkit.Core;
using GraphToolkit.Flow;

namespace GraphToolkit.Cli.Commands;

/// <summary>
/// Команда <c>min-cost-flow</c> — поток минимальной стоимости.
/// </summary>
internal static class MinCostFlowCommand
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
            Description = "Алгоритм: dijkstra (потенциалы) | spfa",
            DefaultValueFactory = _ => "dijkstra"
        };

        var cmd = new Command("min-cost-flow", "Поток минимальной стоимости")
        {
            Arguments = { fileArg },
            Options = { sourceOption, sinkOption, algorithmOption }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var source = parseResult.GetValue(sourceOption)!;
            var sink = parseResult.GetValue(sinkOption)!;
            var algo = parseResult.GetValue(algorithmOption)!.ToLowerInvariant();
            var output = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;
            // Загружаем обычный граф и превращаем в CostFlowNetwork.
            // Формат: weight — capacity, cost — отдельно не задаётся в файле.
            // Поэтому для CLI используем weight как cost при capacity=1.
            var graph = GraphLoader.Load(file.FullName);

            var net = new CostFlowNetwork<string>();
            foreach (var e in graph.Edges)
            {
                net.AddEdge(e.From, e.To, capacity: 1, cost: e.Weight);
                if (!graph.IsDirected)
                    net.AddEdge(e.To, e.From, capacity: 1, cost: e.Weight);
            }

            (double flow, double cost) result = algo switch
            {
                "dijkstra" or "johnson" =>
                    MinCostFlow.DijkstraWithPotentials(net.Clone(), source, sink),
                "spfa" =>
                    MinCostFlow.Spfa(net.Clone(), source, sink),
                _ => throw new ArgumentException($"Неизвестный алгоритм: {algo}")
            };

            output.WriteLine($"Поток:     {result.flow}");
            output.WriteLine($"Стоимость: {result.cost}");
            if (result.flow > 0)
                output.WriteLine($"Средняя:   {result.cost / result.flow:F4}");

            return 0;
        });

        return cmd;
    }
}
