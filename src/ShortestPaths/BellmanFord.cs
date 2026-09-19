using GraphToolkit.Core;

namespace GraphToolkit.ShortestPaths
{
    /// <summary>
    /// Алгоритм Беллмана-Форда для поиска кратчайших путей 
    /// с поддержкой отрицательных весов и детекцией отрицательных циклов.
    /// </summary>
    /// <remarks>
    /// Сложность: O(V * E).
    /// </remarks>
    public static class BellmanFord
    {
        /// <summary>
        /// Вычисляет кратчайшие расстояния от указанной вершины.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <param name="source">Начальная вершина.</param>
        /// <returns>
        /// Кортеж: расстояния, предшественники и признак наличия 
        /// отрицательного цикла, достижимого из источника.
        /// </returns>
        public static (Dictionary<T, double> Distances, Dictionary<T, T> Predecessors, bool HasNegativeCycle)
            Compute<T>(IGraph<T> graph, T source) where T : notnull
        {
            var dist = new Dictionary<T, double>();
            var prev = new Dictionary<T, T>();

            foreach (var v in graph.Vertices)
                dist[v] = double.PositiveInfinity;
            dist[source] = 0;

            int n = graph.VertexCount;
            for (int i = 0; i < n - 1; i++)
            {
                bool updated = false;
                foreach (var e in graph.Edges)
                {
                    if (double.IsPositiveInfinity(dist[e.From])) continue;
                    double nd = dist[e.From] + e.Weight;
                    if (nd < dist[e.To])
                    {
                        dist[e.To] = nd;
                        prev[e.To] = e.From;
                        updated = true;
                    }
                }
                if (!updated) break;
            }

            bool hasCycle = false;
            foreach (var e in graph.Edges)
            {
                if (double.IsPositiveInfinity(dist[e.From])) continue;
                if (dist[e.From] + e.Weight < dist[e.To])
                {
                    hasCycle = true;
                    break;
                }
            }
            return (dist, prev, hasCycle);
        }
    }
}