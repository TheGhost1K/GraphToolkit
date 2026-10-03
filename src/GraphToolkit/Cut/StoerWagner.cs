using GraphToolkit.Core;

namespace GraphToolkit.Cut
{
    /// <summary>
    /// Алгоритм Штёра-Вагнера для поиска глобального минимального разреза 
    /// в неориентированном взвешенном графе.
    /// </summary>
    /// <remarks>
    /// Сложность: O(V³). Не требует указания источника и стока.
    /// </remarks>
    public static class StoerWagner
    {
        /// <summary>
        /// Результат работы алгоритма.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="MinCut">Вес минимального разреза.</param>
        /// <param name="PartitionA">Первая часть разбиения.</param>
        /// <param name="PartitionB">Вторая часть разбиения.</param>
        public sealed record Result<T>(
            double MinCut,
            List<T> PartitionA,
            List<T> PartitionB);

        /// <summary>
        /// Находит глобальный минимальный разрез.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Неориентированный взвешенный граф.</param>
        /// <returns>Результат с весом разреза и разбиением вершин.</returns>
        public static Result<T> Compute<T>(IGraph<T> graph) where T : notnull
        {
            var vertices = graph.Vertices.ToList();
            int n = vertices.Count;
            if (n < 2) return new Result<T>(0, new List<T>(), new List<T>());

            var index = vertices.Select((v, i) => (v, i))
                .ToDictionary(x => x.v, x => x.i);

            var w = new double[n, n];
            foreach (var e in graph.Edges)
            {
                int u = index[e.From], v = index[e.To];
                w[u, v] += e.Weight;
                if (!graph.IsDirected) w[v, u] += e.Weight;
            }

            var bestCut = double.PositiveInfinity;
            List<int>? bestPartition = null;
            var active = Enumerable.Range(0, n).ToList();
            var groups = Enumerable.Range(0, n)
                .Select(i => new List<int> { i }).ToArray();

            while (active.Count > 1)
            {
                int m = active.Count;
                var weights = new double[n];
                var added = new bool[n];
                var order = new List<int>();

                for (int i = 0; i < m; i++)
                {
                    int sel = -1;
                    double maxW = double.NegativeInfinity;
                    foreach (var v in active)
                        if (!added[v] && weights[v] > maxW)
                        {
                            maxW = weights[v];
                            sel = v;
                        }

                    added[sel] = true;
                    order.Add(sel);

                    foreach (var v in active)
                        if (!added[v])
                            weights[v] += w[sel, v];
                }

                int t = order[m - 1], s = order[m - 2];
                double cutWeight = 0;
                foreach (var v in active)
                    if (v != t) cutWeight += w[t, v];

                if (cutWeight < bestCut)
                {
                    bestCut = cutWeight;
                    bestPartition = [.. groups[t]];
                }

                groups[s].AddRange(groups[t]);
                foreach (var v in active)
                {
                    if (v == s || v == t) continue;
                    w[s, v] += w[t, v];
                    w[v, s] += w[v, t];
                }
                active.Remove(t);
            }

            var setA = new HashSet<int>(bestPartition!);
            var partitionA = vertices.Where((_, i) => setA.Contains(i)).ToList();
            var partitionB = vertices.Where((_, i) => !setA.Contains(i)).ToList();
            return new Result<T>(bestCut, partitionA, partitionB);
        }
    }
}