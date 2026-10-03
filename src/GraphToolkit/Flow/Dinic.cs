using GraphToolkit.Core;

namespace GraphToolkit.Flow
{
    /// <summary>
    /// Алгоритм Диница для поиска максимального потока.
    /// </summary>
    /// <remarks>
    /// Сложность: O(V² * E) — самая быстрая из классических реализаций 
    /// для общих графов.
    /// </remarks>
    public static class Dinic
    {
        /// <summary>
        /// Вычисляет максимальный поток.
        /// </summary>
        /// <remarks>
        /// <b>Модифицирует</b> <paramref name="network"/>: остаточные пропускные
        /// способности уменьшаются на величину прошедшего потока.
        /// Используйте <see cref="FlowNetwork{T}.Clone"/> для сохранения
        /// исходного состояния.
        /// </remarks>
        public static double Compute<T>(FlowNetwork<T> network, T source, T sink)
            where T : notnull
        {
            var vertices = network.Vertices.ToList();
            int n = vertices.Count;
            var index = vertices.Select((v, i) => (v, i))
                .ToDictionary(x => x.v, x => x.i);

            int s = index[source], t = index[sink];
            double flow = 0;

            const double EPS = 1e-12;

            while (true)
            {
                // BFS: строим уровневый граф в остаточной сети
                var level = new int[n];
                for (int i = 0; i < n; i++) level[i] = -1;
                level[s] = 0;
                var queue = new Queue<int>();
                queue.Enqueue(s);

                while (queue.Count > 0)
                {
                    int u = queue.Dequeue();
                    foreach (var v in network.Neighbors(vertices[u]))
                    {
                        int vi = index[v];
                        if (level[vi] < 0 && network.GetCapacity(vertices[u], v) > EPS)
                        {
                            level[vi] = level[u] + 1;
                            queue.Enqueue(vi);
                        }
                    }
                }

                if (level[t] < 0) break;   // путь до стока не найден

                // iter[u] — указатель на текущее ребро в уровневом графе
                var iter = new int[n];

                // DFS: ищем блокирующий поток
                double BlockingFlow(int u, double pushed)
                {
                    if (u == t) return pushed;

                    var neighbors = network.Neighbors(vertices[u]).ToList();
                    for (; iter[u] < neighbors.Count; iter[u]++)
                    {
                        var v = neighbors[iter[u]];
                        int vi = index[v];
                        double cap = network.GetCapacity(vertices[u], v);

                        if (cap > EPS && level[vi] == level[u] + 1)
                        {
                            double tr = BlockingFlow(vi, Math.Min(pushed, cap));
                            if (tr > EPS)
                            {
                                network.UpdateCapacity(vertices[u], v, tr);
                                return tr;
                            }
                        }
                    }
                    return 0;
                }

                double f;
                while ((f = BlockingFlow(s, double.PositiveInfinity)) > EPS)
                    flow += f;
            }

            return flow;
        }

        /// <summary>
        /// Возвращает минимальный разрез (множество вершин на стороне источника).
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="network">Сеть (будет изменена).</param>
        /// <param name="source">Источник.</param>
        /// <param name="sink">Сток.</param>
        /// <returns>Множество вершин, достижимых от источника в остаточном графе.</returns>
        public static HashSet<T> MinCut<T>(FlowNetwork<T> network, T source, T sink)
            where T : notnull
        {
            // Запускаем max-flow — сеть теперь в состоянии с максимальным потоком
            Compute(network, source, sink);

            // BFS по остаточному графу
            var reachable = new HashSet<T> { source };
            var queue = new Queue<T>();
            queue.Enqueue(source);
            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                foreach (var v in network.Neighbors(u))
                    if (!reachable.Contains(v) && network.GetCapacity(u, v) > 1e-12)
                    {
                        reachable.Add(v);
                        queue.Enqueue(v);
                    }
            }
            return reachable;
        }
    }
}