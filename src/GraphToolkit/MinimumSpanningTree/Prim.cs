using GraphToolkit.Core;

namespace GraphToolkit.MinimumSpanningTree
{
    /// <summary>
    /// Алгоритм Прима для поиска MST.
    /// </summary>
    /// <remarks>
    /// Сложность: O((V + E) log V) с двоичной кучей. Хорошо работает 
    /// на плотных графах.
    /// </remarks>
    public static class Prim
    {
        /// <summary>
        /// Строит MST начиная с указанной вершины.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Неориентированный граф.</param>
        /// <param name="start">Начальная вершина.</param>
        /// <returns>Список рёбер MST.</returns>
        public static List<Edge<T>> Compute<T>(IGraph<T> graph, T start) where T : notnull
        {
            var mst = new List<Edge<T>>();
            var visited = new HashSet<T> { start };
            var pq = new PriorityQueue<Edge<T>, double>();

            foreach (var e in graph.Neighbors(start))
                pq.Enqueue(e, e.Weight);

            while (pq.Count > 0)
            {
                var edge = pq.Dequeue();
                if (visited.Contains(edge.To)) continue;

                visited.Add(edge.To);
                mst.Add(edge);

                foreach (var next in graph.Neighbors(edge.To))
                    if (!visited.Contains(next.To))
                        pq.Enqueue(next, next.Weight);
            }
            return mst;
        }
    }
}