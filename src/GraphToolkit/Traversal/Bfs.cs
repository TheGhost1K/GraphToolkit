using GraphToolkit.Core;

namespace GraphToolkit.Traversal
{
    /// <summary>
    /// Обход графа в ширину (Breadth-First Search).
    /// </summary>
    /// <remarks>
    /// Сложность: O(V + E). Используется для поиска кратчайшего пути 
    /// в невзвешенном графе, проверки связности и др.
    /// </remarks>
    public static class Bfs
    {
        /// <summary>
        /// Находит кратчайший путь (по числу рёбер) от <paramref name="start"/> 
        /// до <paramref name="goal"/>.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <param name="start">Начальная вершина.</param>
        /// <param name="goal">Целевая вершина.</param>
        /// <returns>
        /// Список вершин, образующих путь, или <c>null</c>, если путь не существует.
        /// </returns>
        /// <example>
        /// <code>
        /// var path = Bfs.FindPath(graph, "A", "F");
        /// Console.WriteLine(string.Join(" -> ", path));
        /// </code>
        /// </example>
        public static List<T>? FindPath<T>(IGraph<T> graph, T start, T goal) where T : notnull
        {
            var queue = new Queue<T>();
            var visited = new HashSet<T> { start };
            var parent = new Dictionary<T, T>();

            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (EqualityComparer<T>.Default.Equals(current, goal))
                    return ReconstructPath(parent, start, goal);

                foreach (var edge in graph.Neighbors(current))
                {
                    if (visited.Add(edge.To))
                    {
                        parent[edge.To] = current;
                        queue.Enqueue(edge.To);
                    }
                }
            }
            return null;
        }

        private static List<T> ReconstructPath<T>(
            Dictionary<T, T> parent, T start, T goal) where T : notnull
        {
            var path = new List<T> { goal };
            var current = goal;
            while (!EqualityComparer<T>.Default.Equals(current, start))
            {
                if (!parent.TryGetValue(current, out var p)) return new List<T>();
                current = p;
                path.Add(current);
            }
            path.Reverse();
            return path;
        }
    }
}