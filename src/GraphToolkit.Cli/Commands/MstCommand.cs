using System.CommandLine;
using GraphToolkit.Core;
using GraphToolkit.IO;
using GraphToolkit.MinimumSpanningTree;
using GraphToolkit.Visualization;

namespace GraphToolkit.Cli.Commands;

internal static class MstCommand
{
    public static Command Create()
    {
        var fileArg = GraphLoader.FileArgument();

        var algorithmOption = new Option<string>("--algorithm")
        {
            Description = "Алгоритм: kruskal | prim | boruvka",
            DefaultValueFactory = _ => "kruskal"
        };

        var outputOption = new Option<FileInfo?>("--output")
        {
            Description = "Сохранить MST в файл (формат по расширению)"
        };

        var cmd = new Command("mst", "Минимальное остовное дерево")
        {
            Arguments = { fileArg },
            Options = { algorithmOption, outputOption }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var algo = parseResult.GetValue(algorithmOption)!;
            var output = parseResult.GetValue(outputOption);
            var outputConsole = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;
            var graph = GraphLoader.Load(file.FullName);

            var mst = algo.ToLowerInvariant() switch
            {
                "kruskal" => Kruskal.Compute(graph),
                "prim" => Prim.Compute(graph, graph.Vertices.First()),
                "boruvka" => Boruvka.Compute(graph),
                _ => throw new ArgumentException($"Неизвестный алгоритм: {algo}")
            };

            double total = 0;
            foreach (var e in mst)
            {
                outputConsole.WriteLine($"{e.From} — {e.To}  ({e.Weight})");
                total += e.Weight;
            }
            outputConsole.WriteLine($"\nИтого: {mst.Count} рёбер, вес = {total}");

            if (output is not null)
            {
                SaveByExtension(graph, mst, output.FullName);
                outputConsole.WriteLine($"\nMST сохранён в {output.FullName}");
            }

            return 0;
        });

        return cmd;
    }

    private static void SaveByExtension(
        IGraph<string> graph,
        List<Edge<string>> mst,
        string path)
    {
        var mstGraph = new Graph<string>(isDirected: false);
        foreach (var v in graph.Vertices) mstGraph.AddVertex(v);
        foreach (var e in mst) mstGraph.AddEdge(e.From, e.To, e.Weight);

        var ext = Path.GetExtension(path).ToLowerInvariant();
        switch (ext)
        {
            case ".graphml": GraphIO.SaveGraphML(mstGraph, path); break;
            case ".gexf": GraphIO.SaveGexf(mstGraph, path); break;
            case ".json": GraphIO.SaveJson(mstGraph, path); break;
            case ".csv": GraphIO.SaveCsv(mstGraph, path); break;
            case ".dot" or ".gv":
                File.WriteAllText(path, GraphExporters.ToDot(mstGraph));
                break;
            default:
                throw new NotSupportedException($"Формат '{ext}' не поддерживается");
        }
    }
}
