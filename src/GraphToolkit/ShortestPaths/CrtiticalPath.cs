using GraphToolkit.Core;
using GraphToolkit.Traversal;

namespace GraphToolkit.ShortestPaths
{
    /// <summary>
    /// Критический путь в DAG — самый длинный путь по весам.
    /// </summary>
    public static class CriticalPath
    {
        /// <summary>
        /// Вычисляет критический путь в ориентированном ациклическом графе.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="dag">Ориентированный ациклический граф.</param>
        /// <returns>
        /// Кортеж: список вершин критического пути и его длина.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// Если граф содержит цикл.
        /// </exception>
        public static (List<T> Path, double Length) Compute<T>(IGraph<T> dag)
            where T : notnull
        {
            var topo = TopologicalSort.Sort(dag)
                ?? throw new System.InvalidOperationException("Граф содержит цикл");

            var dist = new Dictionary<T, double>();
            var prev = new Dictionary<T, T>();
            foreach (var v in dag.Vertices)
                dist[v] = double.NegativeInfinity;

            foreach (var v in topo)
            {
                if (double.IsNegativeInfinity(dist[v])) dist[v] = 0;
                foreach (var e in dag.Neighbors(v))
                {
                    if (dist[v] + e.Weight > dist[e.To])
                    {
                        dist[e.To] = dist[v] + e.Weight;
                        prev[e.To] = v;
                    }
                }
            }

            var end = dist.OrderByDescending(kv => kv.Value).First().Key;
            var path = new List<T>();
            var cur = end;
            while (true)
            {
                path.Add(cur);
                if (!prev.TryGetValue(cur, out var p)) break;
                cur = p;
            }
            path.Reverse();
            return (path, dist[end]);
        }
    }
}