using GraphToolkit.Core;
using System.Text;

namespace GraphToolkit.Visualization
{
    /// <summary>
    /// Экспорт графа в различные текстовые форматы.
    /// </summary>
    public static class GraphExporters
    {
        /// <summary>
        /// Экспортирует граф в формат GraphViz DOT.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <param name="title">Имя графа в DOT.</param>
        /// <param name="labels">Опциональные метки вершин.</param>
        /// <param name="colors">Опциональные цвета вершин (формат CSS).</param>
        /// <param name="edgeColors">Опциональные цвета рёбер.</param>
        /// <returns>Строка в формате DOT.</returns>
        /// <example>
        /// <code>
        /// var dot = GraphExporters.ToDot(graph, "MyGraph");
        /// File.WriteAllText("graph.dot", dot);
        /// // Затем: dot -Tpng graph.dot -o graph.png
        /// </code>
        /// </example>
        public static string ToDot<T>(
            IGraph<T> graph,
            string title = "G",
            Dictionary<T, string>? labels = null,
            Dictionary<T, string>? colors = null,
            Dictionary<Edge<T>, string>? edgeColors = null) where T : notnull
        {
            var sb = new StringBuilder();
            sb.AppendLine(graph.IsDirected
                ? $"digraph {Sanitize(title)} {{"
                : $"graph {Sanitize(title)} {{");
            sb.AppendLine("    rankdir=LR;");
            sb.AppendLine("    node [shape=circle, style=filled, fillcolor=lightblue];");

            foreach (var v in graph.Vertices)
            {
                string name = Sanitize(v.ToString()!);
                string label = labels?.GetValueOrDefault(v) ?? v.ToString()!;
                string? color = colors?.GetValueOrDefault(v);
                sb.Append($"    \"{name}\" [label=\"{label}\"");
                if (color != null) sb.Append($", fillcolor=\"{color}\"");
                sb.AppendLine("];");
            }

            string connector = graph.IsDirected ? "->" : "--";
            foreach (var e in graph.Edges)
            {
                string from = Sanitize(e.From.ToString()!);
                string to = Sanitize(e.To.ToString()!);
                string? color = edgeColors?.GetValueOrDefault(e);
                sb.Append($"    \"{from}\" {connector} \"{to}\"");
                var attrs = new List<string>();
                if (Math.Abs(e.Weight - 1.0) > 1e-9) attrs.Add($"label=\"{e.Weight}\"");
                if (color != null) attrs.Add($"color=\"{color}\"");
                if (attrs.Count > 0) sb.Append($" [{string.Join(", ", attrs)}]");
                sb.AppendLine(";");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// Экспортирует граф в формат Mermaid (для markdown / GitHub / GitLab).
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <param name="colors">Опциональные цвета вершин.</param>
        /// <returns>Строка в формате Mermaid.</returns>
        public static string ToMermaid<T>(
            IGraph<T> graph,
            Dictionary<T, string>? colors = null) where T : notnull
        {
            var sb = new StringBuilder();
            sb.AppendLine(graph.IsDirected ? "graph LR" : "graph TD");

            var ids = graph.Vertices
                .Select((v, i) => (v, id: $"V{i}"))
                .ToDictionary(x => x.v, x => x.id);

            foreach (var v in graph.Vertices)
            {
                string id = ids[v];
                string label = v.ToString()!.Replace("\"", "'");
                sb.AppendLine($"    {id}[\"{label}\"]");
                if (colors?.TryGetValue(v, out var c) == true)
                    sb.AppendLine($"    style {id} fill:{c}");
            }

            var seen = new HashSet<(string, string)>();
            string connector = graph.IsDirected ? "-->" : "---";
            foreach (var e in graph.Edges)
            {
                string from = ids[e.From], to = ids[e.To];
                if (!graph.IsDirected)
                {
                    var key = string.CompareOrdinal(from, to) < 0 ? (from, to) : (to, from);
                    if (!seen.Add(key)) continue;
                }
                string label = Math.Abs(e.Weight - 1.0) > 1e-9 ? $"|{e.Weight}|" : "";
                sb.AppendLine($"    {from} {connector}{label} {to}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Возвращает матрицу смежности в виде текста.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <param name="showWeights">Показывать веса вместо 1/0.</param>
        /// <returns>Текстовое представление матрицы.</returns>
        public static string ToAdjacencyMatrix<T>(IGraph<T> graph, bool showWeights = true)
            where T : notnull
        {
            var vertices = graph.Vertices.ToList();
            int n = vertices.Count;
            var index = vertices.Select((v, i) => (v, i))
                .ToDictionary(x => x.v, x => x.i);

            var matrix = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    matrix[i, j] = i == j ? 0 : double.PositiveInfinity;

            foreach (var e in graph.Edges)
            {
                int u = index[e.From], v = index[e.To];
                if (e.Weight < matrix[u, v]) matrix[u, v] = e.Weight;
                if (!graph.IsDirected && e.Weight < matrix[v, u]) matrix[v, u] = e.Weight;
            }

            var sb = new StringBuilder();
            sb.Append("      ");
            foreach (var v in vertices) sb.Append($"{v,6}");
            sb.AppendLine();

            for (int i = 0; i < n; i++)
            {
                sb.Append($"{vertices[i],5} ");
                for (int j = 0; j < n; j++)
                {
                    if (i == j) sb.Append($"{"0",6}");
                    else if (double.IsPositiveInfinity(matrix[i, j])) sb.Append($"{"-",6}");
                    else if (!showWeights) sb.Append($"{"1",6}");
                    else sb.Append($"{matrix[i, j],6:0.##}");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Возвращает список смежности в виде текста.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <returns>Текстовое представление списка.</returns>
        public static string ToAdjacencyList<T>(IGraph<T> graph) where T : notnull
        {
            var sb = new StringBuilder();
            foreach (var v in graph.Vertices)
            {
                sb.Append($"{v}: ");
                var neighbors = graph.Neighbors(v)
                    .Select(e => Math.Abs(e.Weight - 1.0) > 1e-9
                        ? $"{e.To}({e.Weight})"
                        : e.To.ToString());
                sb.AppendLine(string.Join(", ", neighbors));
            }
            return sb.ToString();
        }

        private static string Sanitize(string s) => s.Replace("\"", "\\\"");
    }
}