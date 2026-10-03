using System.CommandLine;
using GraphToolkit.Eulerian;

namespace GraphToolkit.Cli.Commands;

/// <summary>
/// Команда <c>eulerian</c> — эйлеровы пути и циклы.
/// </summary>
internal static class EulerianCommand
{
    public static Command Create()
    {
        var fileArg = GraphLoader.FileArgument();

        var modeOption = new Option<string>("--mode")
        {
            Description = "Режим: check | find",
            DefaultValueFactory = _ => "check"
        };

        var cmd = new Command("eulerian", "Эйлеровы пути и циклы")
        {
            Arguments = { fileArg },
            Options = { modeOption }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var mode = parseResult.GetValue(modeOption)!.ToLowerInvariant();
            var output = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;
            var graph = GraphLoader.Load(file.FullName);

            switch (mode)
            {
                case "check":
                    bool hasCycle = EulerianPath.HasCycle(graph);
                    var check = EulerianPath.HasPath(graph);

                    output.WriteLine($"Эйлеров цикл: {hasCycle}");
                    output.WriteLine($"Эйлеров путь: {check.Exists}");
                    if (check.Exists)
                    {
                        var s = check.Start?.ToString() ?? "(любая)";
                        var e = check.End?.ToString() ?? "(любая)";
                        output.WriteLine($"  Начало: {s}, конец: {e}");
                    }
                    return 0;

                case "find":
                    var path = EulerianPath.Find(graph);
                    if (path is null || path.Count == 0)
                    {
                        error.WriteLine("Эйлеров путь не существует");
                        return 1;
                    }
                    output.WriteLine(string.Join(" -> ", path));
                    return 0;

                default:
                    error.WriteLine($"Неизвестный режим: {mode}");
                    return 1;
            }
        });

        return cmd;
    }
}
