using GraphToolkit.Core;

namespace GraphToolkit.Flow
{
    /// <summary>
    /// Алгоритм Форда-Фалкерсона для поиска максимального потока (DFS-версия).
    /// </summary>
    /// <remarks>
    /// Сложность зависит от способа поиска увеличивающего пути; 
    /// для целочисленных пропускных способностей может быть O(E * f).
    /// </remarks>
    public static class FordFulkerson
    {
        /// <summary>
        /// Вычисляет максимальный поток.
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
            var visited = new HashSet<T>();
            var path = new List<T>();

            bool Dfs(T u)
            {
                if (EqualityComparer<T>.Default.Equals(u, sink)) return true;
                visited.Add(u);
                foreach (var v in network.Neighbors(u))
                {
                    if (!visited.Contains(v) && network.GetCapacity(u, v) > 0)
                    {
                        path.Add(v);
                        if (Dfs(v)) return true;
                        path.RemoveAt(path.Count - 1);
                    }
                }
                return false;
            }

            while (true)
            {
                visited.Clear();
                path.Clear();
                path.Add(source);
                if (!Dfs(source)) break;

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
    }
}