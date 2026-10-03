using System.CommandLine;

namespace GraphToolkit.Cli.Commands;

internal static class InfoCommand
{
    public static Command Create()
    {
        var fileArg = GraphLoader.FileArgument();

        var cmd = new Command("info", "Информация о графе")
        {
            Arguments = { fileArg }
        };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var output = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;
            var graph = GraphLoader.Load(file.FullName);
            output.WriteLine($"Файл:              {file.Name}");
            output.WriteLine($"Формат:            {file.Extension.TrimStart('.')}");
            output.WriteLine($"Ориентированный:   {(graph.IsDirected ? "да" : "нет")}");
            output.WriteLine($"Вершин:            {graph.VertexCount}");
            output.WriteLine($"Рёбер:             {graph.EdgeCount}");

            if (graph.VertexCount > 0)
            {
                double maxDeg = graph.Vertices.Max(v => graph.Neighbors(v).Count());
                double minDeg = graph.Vertices.Min(v => graph.Neighbors(v).Count());
                double avgDeg = graph.Vertices.Average(v => graph.Neighbors(v).Count());

                output.WriteLine($"Макс. степень:     {maxDeg}");
                output.WriteLine($"Мин. степень:      {minDeg}");
                output.WriteLine($"Средняя степень:   {avgDeg:F2}");

                if (graph.EdgeCount > 0)
                {
                    output.WriteLine($"Мин. вес:          {graph.Edges.Min(e => e.Weight)}");
                    output.WriteLine($"Макс. вес:         {graph.Edges.Max(e => e.Weight)}");
                }
            }

            return 0;
        });

        return cmd;
    }
}
