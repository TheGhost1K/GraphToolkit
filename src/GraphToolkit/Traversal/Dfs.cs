using GraphToolkit.Core;

namespace GraphToolkit.Traversal
{
    /// <summary>
    /// Обход графа в глубину (Depth-First Search).
    /// </summary>
    /// <remarks>
    /// Сложность: O(V + E). Реализация итеративная, что исключает 
    /// переполнение стека на больших графах.
    /// </remarks>
    public static class Dfs
    {
        /// <summary>
        /// Находит путь от <paramref name="start"/> до <paramref name="goal"/> 
        /// (не обязательно кратчайший).
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <param name="start">Начальная вершина.</param>
        /// <param name="goal">Целевая вершина.</param>
        /// <returns>Список вершин пути или <c>null</c>, если путь не существует.</returns>
        public static List<T>? FindPath<T>(IGraph<T> graph, T start, T goal) where T : notnull
        {
            var stack = new Stack<T>();
            var visited = new HashSet<T>();
            var parent = new Dictionary<T, T>();

            stack.Push(start);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!visited.Add(current)) continue;

                if (EqualityComparer<T>.Default.Equals(current, goal))
                    return ReconstructPath(parent, start, goal);

                foreach (var edge in graph.Neighbors(current))
                {
                    if (!visited.Contains(edge.To))
                    {
                        if (!parent.ContainsKey(edge.To))
                            parent[edge.To] = current;
                        stack.Push(edge.To);
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