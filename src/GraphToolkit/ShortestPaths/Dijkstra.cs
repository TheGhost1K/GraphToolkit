using GraphToolkit.Core;

namespace GraphToolkit.ShortestPaths
{
    /// <summary>
    /// Алгоритм Дейкстры для поиска кратчайших путей от одной вершины.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Требует неотрицательных весов рёбер. Сложность O((V + E) log V) 
    /// с использованием двоичной кучи.
    /// </para>
    /// <para>
    /// Для графов с отрицательными весами используйте 
    /// <see cref="BellmanFord"/>.
    /// </para>
    /// </remarks>
    public static class Dijkstra
    {
        /// <summary>
        /// Вычисляет кратчайшие расстояния от указанной вершины до всех остальных.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф с неотрицательными весами.</param>
        /// <param name="source">Начальная вершина.</param>
        /// <returns>
        /// Кортеж из двух словарей: расстояний и предшественников 
        /// (для восстановления путей).
        /// </returns>
        public static (Dictionary<T, double> Distances, Dictionary<T, T> Predecessors)
            Compute<T>(IGraph<T> graph, T source) where T : notnull
        {
            var dist = new Dictionary<T, double>();
            var prev = new Dictionary<T, T>();
            var pq = new PriorityQueue<T, double>();

            foreach (var v in graph.Vertices)
                dist[v] = double.PositiveInfinity;

            dist[source] = 0;
            pq.Enqueue(source, 0);

            while (pq.TryDequeue(out var u, out _))
            {
                foreach (var edge in graph.Neighbors(u))
                {
                    double nd = dist[u] + edge.Weight;
                    if (nd < dist[edge.To])
                    {
                        dist[edge.To] = nd;
                        prev[edge.To] = u;
                        pq.Enqueue(edge.To, nd);
                    }
                }
            }
            return (dist, prev);
        }

        /// <summary>
        /// Находит кратчайший путь между двумя вершинами.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф с неотрицательными весами.</param>
        /// <param name="source">Начальная вершина.</param>
        /// <param name="target">Целевая вершина.</param>
        /// <returns>Список вершин пути или <c>null</c>, если путь недостижим.</returns>
        public static List<T>? FindPath<T>(IGraph<T> graph, T source, T target)
            where T : notnull
        {
            var (dist, prev) = Compute(graph, source);
            if (double.IsPositiveInfinity(dist.GetValueOrDefault(target, double.PositiveInfinity)))
                return null;

            var path = new List<T> { target };
            var current = target;
            while (!EqualityComparer<T>.Default.Equals(current, source))
            {
                if (!prev.TryGetValue(current, out var p)) return null;
                current = p;
                path.Add(current);
            }
            path.Reverse();
            return path;
        }
    }
}