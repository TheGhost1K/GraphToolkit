using GraphToolkit.Core;

namespace GraphToolkit.Traversal
{
    /// <summary>
    /// Топологическая сортировка ориентированного ациклического графа (DAG).
    /// </summary>
    public static class TopologicalSort
    {
        /// <summary>
        /// Выполняет топологическую сортировку (алгоритм Кана).
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Ориентированный граф.</param>
        /// <returns>
        /// Линейный порядок вершин или <c>null</c>, если граф содержит цикл.
        /// </returns>
        /// <remarks>
        /// Сложность: O(V + E).
        /// </remarks>
        public static List<T>? Sort<T>(IGraph<T> graph) where T : notnull
        {
            var inDegree = graph.Vertices.ToDictionary(v => v, _ => 0);
            foreach (var e in graph.Edges)
                inDegree[e.To]++;

            var queue = new Queue<T>(
                inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
            var result = new List<T>();

            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                result.Add(u);
                foreach (var edge in graph.Neighbors(u))
                    if (--inDegree[edge.To] == 0)
                        queue.Enqueue(edge.To);
            }

            return result.Count == graph.VertexCount ? result : null;
        }

        /// <summary>
        /// Вычисляет топологические уровни вершин DAG: 
        /// уровень = длина самого длинного пути от истока в рёбрах.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Ориентированный ациклический граф.</param>
        /// <returns>Словарь "вершина → уровень".</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Если граф содержит цикл.
        /// </exception>
        public static Dictionary<T, int> ComputeLevels<T>(IGraph<T> graph) where T : notnull
        {
            var topo = Sort(graph)
                ?? throw new System.InvalidOperationException("Граф содержит цикл");

            var level = graph.Vertices.ToDictionary(v => v, _ => 0);
            foreach (var u in topo)
                foreach (var e in graph.Neighbors(u))
                    level[e.To] = System.Math.Max(level[e.To], level[u] + 1);
            return level;
        }
    }
}