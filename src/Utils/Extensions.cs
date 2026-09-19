using GraphToolkit.Core;

namespace GraphToolkit.Utils
{
    /// <summary>
    /// Полезные расширения для работы с графами.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Вычисляет степень вершины (число исходящих рёбер).
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <param name="vertex">Вершина.</param>
        /// <returns>Степень вершины.</returns>
        public static int Degree<T>(this IGraph<T> graph, T vertex) where T : notnull
            => graph.Neighbors(vertex).Count();

        /// <summary>
        /// Вычисляет входящую степень вершины (для ориентированных графов).
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <param name="vertex">Вершина.</param>
        /// <returns>Число входящих рёбер.</returns>
        public static int InDegree<T>(this IGraph<T> graph, T vertex) where T : notnull
            => graph.Edges.Count(e => EqualityComparer<T>.Default.Equals(e.To, vertex));

        /// <summary>
        /// Проверяет, содержит ли граф указанную вершину.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <param name="vertex">Вершина.</param>
        /// <returns><c>true</c>, если вершина есть в графе.</returns>
        public static bool ContainsVertex<T>(this IGraph<T> graph, T vertex)
            where T : notnull
            => graph.Vertices.Contains(vertex);

        /// <summary>
        /// Проверяет, содержит ли граф ребро между двумя вершинами.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <param name="from">Источник.</param>
        /// <param name="to">Приёмник.</param>
        /// <returns><c>true</c>, если ребро существует.</returns>
        public static bool ContainsEdge<T>(this IGraph<T> graph, T from, T to)
            where T : notnull
            => graph.Neighbors(from).Any(e =>
                EqualityComparer<T>.Default.Equals(e.To, to));

        /// <summary>
        /// Возвращает вершины с максимальной степенью (например, для раскраски).
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <returns>Перечисление вершин в порядке убывания степени.</returns>
        public static IEnumerable<T> OrderByDegreeDescending<T>(this IGraph<T> graph)
            where T : notnull
            => graph.Vertices.OrderByDescending(v => graph.Degree(v));

        /// <summary>
        /// Проверяет, является ли граф деревом (связный, V - 1 рёбер).
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Неориентированный граф.</param>
        /// <returns><c>true</c>, если граф — дерево.</returns>
        public static bool IsTree<T>(this IGraph<T> graph) where T : notnull
        {
            if (graph.VertexCount == 0) return false;
            if (graph.EdgeCount != graph.VertexCount - 1) return false;

            var visited = new HashSet<T>();
            var stack = new Stack<T>();
            var start = graph.Vertices.First();
            stack.Push(start);

            while (stack.Count > 0)
            {
                var v = stack.Pop();
                if (!visited.Add(v)) continue;
                foreach (var e in graph.Neighbors(v))
                    if (!visited.Contains(e.To)) stack.Push(e.To);
            }
            return visited.Count == graph.VertexCount;
        }
    }
}