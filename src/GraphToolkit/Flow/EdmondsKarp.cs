using GraphToolkit.Core;

namespace GraphToolkit.Flow
{
    /// <summary>
    /// Алгоритм Эдмондса-Карпа для поиска максимального потока.
    /// </summary>
    /// <remarks>
    /// BFS-версия Форда-Фалкерсона. Сложность: O(V * E²).
    /// </remarks>
    public static class EdmondsKarp
    {
        /// <summary>
        /// Вычисляет максимальный поток от <paramref name="source"/> 
        /// до <paramref name="sink"/>.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="network">Сеть. Модифицируется в процессе работы.</param>
        /// <param name="source">Источник.</param>
        /// <param name="sink">Сток.</param>
        /// <returns>Величина максимального потока.</returns>
        public static double Compute<T>(FlowNetwork<T> network, T source, T sink)
            where T : notnull
        {
            double flow = 0;
            while (true)
            {
                var path = FindAugmentingPath(network, source, sink);
                if (path == null) break;

                double bottleneck = double.PositiveInfinity;
                for (int i = 0; i < path.Count - 1; i++)
                    bottleneck = System.Math.Min(
                        bottleneck, network.GetCapacity(path[i], path[i + 1]));

                for (int i = 0; i < path.Count - 1; i++)
                    network.UpdateCapacity(path[i], path[i + 1], bottleneck);

                flow += bottleneck;
            }
            return flow;
        }

        internal static List<T>? FindAugmentingPath<T>(
            FlowNetwork<T> network, T source, T sink) where T : notnull
        {
            var parent = new Dictionary<T, T>();
            var visited = new HashSet<T> { source };
            var queue = new Queue<T>();
            queue.Enqueue(source);

            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                if (EqualityComparer<T>.Default.Equals(u, sink)) break;

                foreach (var v in network.Neighbors(u))
                {
                    if (!visited.Contains(v) && network.GetCapacity(u, v) > 0)
                    {
                        visited.Add(v);
                        parent[v] = u;
                        queue.Enqueue(v);
                    }
                }
            }

            if (!visited.Contains(sink)) return null;

            var path = new List<T>();
            for (var cur = sink; !EqualityComparer<T>.Default.Equals(cur, source);
                 cur = parent[cur])
                path.Add(cur);
            path.Add(source);
            path.Reverse();
            return path;
        }
    }
}