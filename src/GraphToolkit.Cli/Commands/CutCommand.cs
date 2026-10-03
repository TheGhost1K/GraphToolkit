using System.CommandLine;
using GraphToolkit.Cut;

namespace GraphToolkit.Cli.Commands;

/// <summary>
/// Команда <c>cut</c> — минимальные разрезы.
/// </summary>
internal static class CutCommand
{
    public static Command Create()
    {
        var fileArg = GraphLoader.FileArgument();

        var typeOption = new Option<string>("--type")
        {
            Description = "Тип: stoer-wagner (глобальный) | gomory-hu (все пары)",
            DefaultValueFactory = _ => "stoer-wagner"
        };

        var fromOption = new Option<string?>("--from")
        {
            Description = "Первая вершина (для gomory-hu)"
        };

        var toOption = new Option<string?>("--to")
        {
            Description = "Вторая вершина (для gomory-hu)"
        };

        var cmd = new Command("cut", "Минимальные разрезы")
        {
            Arguments = { fileArg },
            Options = { typeOption, fromOption, toOption }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var type = parseResult.GetValue(typeOption)!.ToLowerInvariant();
            var from = parseResult.GetValue(fromOption);
            var to = parseResult.GetValue(toOption);
            var output = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;

            var graph = GraphLoader.Load(file.FullName);

            switch (type)
            {
                case "stoer-wagner":
                case "sw":
                    var result = StoerWagner.Compute(graph);
                    output.WriteLine($"Min-cut: {result.MinCut}");
                    output.WriteLine($"Сторона A: " +
                        "{" + string.Join(", ", result.PartitionA) + "}");
                    output.WriteLine($"Сторона B: " +
                        "{" + string.Join(", ", result.PartitionB) + "}");
                    return 0;

                case "gomory-hu":
                case "gh":
                    if (from is null || to is null)
                    {
                        error.WriteLine(
                            "--from и --to обязательны для gomory-hu");
                        return 1;
                    }
                    var tree = GomoryHu.Compute(graph);
                    double minCut = tree.MinCut(from, to);
                    output.WriteLine($"Min-cut({from}, {to}) = {minCut}");
                    return 0;

                default:
                    Console.Error.WriteLine($"Неизвестный тип: {type}");
                    return 1;
            }
        });

        return cmd;
    }
}
