using System.CommandLine;
using GraphToolkit.Components;

namespace GraphToolkit.Cli.Commands;

/// <summary>
/// Команда <c>components</c> — связные компоненты, мосты, точки сочленения.
/// </summary>
internal static class ComponentsCommand
{
    public static Command Create()
    {
        var fileArg = GraphLoader.FileArgument();

        var typeOption = new Option<string>("--type")
        {
            Description = "Тип: connected | bridges | articulation",
            DefaultValueFactory = _ => "connected"
        };

        var cmd = new Command("components",
            "Связные компоненты, мосты, точки сочленения")
        {
            Arguments = { fileArg },
            Options = { typeOption }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var type = parseResult.GetValue(typeOption)!.ToLowerInvariant();
            var output = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;
            var graph = GraphLoader.Load(file.FullName);

            switch (type)
            {
                case "connected":
                case "cc":
                    var components = ConnectedComponents.Find(graph);
                    output.WriteLine($"Связных компонент: {components.Count}");
                    for (int i = 0; i < components.Count; i++)
                        output.WriteLine($"  #{i + 1}: " +
                            "{" + string.Join(", ", components[i]) + "}");
                    return 0;

                case "bridges":
                case "articulation":
                    var result = BridgesAndArticulation.Find(graph);
                    if (type == "bridges")
                    {
                        output.WriteLine($"Мостов: {result.Bridges.Count}");
                        foreach (var (u, v) in result.Bridges)
                            output.WriteLine($"  {u} — {v}");
                    }
                    else
                    {
                        output.WriteLine($"Точек сочленения: " +
                            $"{result.ArticulationPoints.Count}");
                        foreach (var v in result.ArticulationPoints)
                            output.WriteLine($"  {v}");
                    }
                    return 0;

                default:
                    error.WriteLine(
                        $"Неизвестный тип: {type}. " +
                        $"Используйте: connected | bridges | articulation");
                    return 1;
            }
        });

        return cmd;
    }
}
