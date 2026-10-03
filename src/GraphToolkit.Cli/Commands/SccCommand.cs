using System.CommandLine;
using GraphToolkit.Components;

namespace GraphToolkit.Cli.Commands;

internal static class SccCommand
{
    public static Command Create()
    {
        var fileArg = GraphLoader.FileArgument();

        var componentsOption = new Option<bool>("--components")
        {
            Description = "Также показать связные компоненты (не только SCC)"
        };

        var cmd = new Command("scc", "Компоненты сильной связности")
        {
            Arguments = { fileArg },
            Options = { componentsOption }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var components = parseResult.GetValue(componentsOption);
            var output = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;
            var graph = GraphLoader.Load(file.FullName);

            var sccs = StronglyConnectedComponents.Find(graph);
            output.WriteLine($"SCC: {sccs.Count}");
            foreach (var scc in sccs)
                output.WriteLine($"  {{{string.Join(", ", scc)}}}");

            if (components)
            {
                var connected = ConnectedComponents.Find(graph);
                output.WriteLine($"\nСвязные компоненты: {connected.Count}");
                foreach (var c in connected)
                    output.WriteLine($"  {{{string.Join(", ", c)}}}");
            }

            return 0;
        });

        return cmd;
    }
}
