using System.CommandLine;
using GraphToolkit.Hamiltonian;

namespace GraphToolkit.Cli.Commands;

/// <summary>
/// Команда <c>tsp</c> — задача коммивояжёра и гамильтоновы циклы.
/// </summary>
internal static class TspCommand
{
    public static Command Create()
    {
        var fileArg = GraphLoader.FileArgument();

        var startOption = new Option<string?>("--start")
        {
            Description = "Стартовая вершина (обязательна для TSP)"
        };

        var algorithmOption = new Option<string>("--algorithm")
        {
            Description = "Алгоритм: tsp-exact | tsp-approx | hamiltonian",
            DefaultValueFactory = _ => "tsp-approx"
        };

        var cmd = new Command("tsp", "TSP и гамильтоновы циклы")
        {
            Arguments = { fileArg },
            Options = { startOption, algorithmOption }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var start = parseResult.GetValue(startOption);
            var algo = parseResult.GetValue(algorithmOption)!.ToLowerInvariant();
            var output = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;
            var graph = GraphLoader.Load(file.FullName);

            switch (algo)
            {
                case "tsp-exact":
                case "exact":
                    if (start is null)
                    {
                        error.WriteLine("--start обязателен для tsp-exact");
                        return 1;
                    }
                    var exact = Tsp.BranchAndBound(graph, start);
                    if (exact.Path is null)
                    {
                        error.WriteLine("Маршрут не найден");
                        return 1;
                    }
                    output.WriteLine($"Оптимальный маршрут: " +
                        string.Join(" -> ", exact.Path));
                    output.WriteLine($"Стоимость: {exact.Cost}");
                    return 0;

                case "tsp-approx":
                case "approx":
                    if (start is null)
                    {
                        error.WriteLine("--start обязателен для tsp-approx");
                        return 1;
                    }
                    var approx = Tsp.NearestNeighbor(graph, start);
                    if (approx.Path is null)
                    {
                        error.WriteLine("Маршрут не найден");
                        return 1;
                    }
                    output.WriteLine($"Приближённый маршрут: " +
                        string.Join(" -> ", approx.Path));
                    output.WriteLine($"Стоимость: {approx.Cost}");
                    return 0;

                case "hamiltonian":
                    var cycle = HamiltonianCycle.FindCycle(graph);
                    if (cycle is null)
                    {
                        output.WriteLine("Гамильтонов цикл не найден");
                        return 1;
                    }
                    output.WriteLine("Гамильтонов цикл: " +
                        string.Join(" -> ", cycle));
                    return 0;

                default:
                    error.WriteLine($"Неизвестный алгоритм: {algo}");
                    return 1;
            }
        });

        return cmd;
    }
}
