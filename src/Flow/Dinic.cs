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
        private sealed class Arc
        {
            public int To;
            public int Rev;
            public double Cap;
        }

        /// <summary>
        /// Вычисляет максимальный поток.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="network">Сеть.</param>
        /// <param name="source">Источник.</param>
        /// <param name="sink">Сток.</param>
        /// <returns>Величина максимального потока.</returns>
        public static double Compute<T>(FlowNetwork<T> network, T source, T sink)
            where T : notnull
        {
            var vertices = network.Vertices.ToList();
            int n = vertices.Count;
            var index = vertices.Select((v, i) => (v, i))
                .ToDictionary(x => x.v, x => x.i);

            var graph = new List<Arc>[n];
            for (int i = 0; i < n; i++) graph[i] = new List<Arc>();

            void AddArc(int from, int to, double cap)
            {
                var a = new Arc { To = to, Cap = cap, Rev = graph[to].Count };
                var b = new Arc { To = from, Cap = 0, Rev = graph[from].Count };
                graph[from].Add(a);
                graph[to].Add(b);
            }

            var seen = new HashSet<(int, int)>();
            foreach (var u in vertices)
                foreach (var v in network.Neighbors(u))
                {
                    double cap = network.GetCapacity(u, v);
                    if (cap > 0)
                    {
                        int iu = index[u], iv = index[v];
                        if (seen.Add((iu, iv)))
                            AddArc(iu, iv, cap);
                    }
                }

            int s = index[source], t = index[sink];
            double flow = 0;
            var level = new int[n];
            var iter = new int[n];

            bool Bfs()
            {
                for (int i = 0; i < n; i++) level[i] = -1;
                level[s] = 0;
                var q = new Queue<int>();
                q.Enqueue(s);
                while (q.Count > 0)
                {
                    int u = q.Dequeue();
                    foreach (var e in graph[u])
                        if (e.Cap > 1e-12 && level[e.To] < 0)
                        {
                            level[e.To] = level[u] + 1;
                            q.Enqueue(e.To);
                        }
                }
                return level[t] >= 0;
            }

            double Dfs(int u, double f)
            {
                if (u == t) return f;
                for (; iter[u] < graph[u].Count; iter[u]++)
                {
                    var e = graph[u][iter[u]];
                    if (e.Cap > 1e-12 && level[u] < level[e.To])
                    {
                        double d = Dfs(e.To, Math.Min(f, e.Cap));
                        if (d > 1e-12)
                        {
                            e.Cap -= d;
                            graph[e.To][e.Rev].Cap += d;
                            return d;
                        }
                    }
                }
                return 0;
            }

            while (Bfs())
            {
                for (int i = 0; i < n; i++) iter[i] = 0;
                double f;
                while ((f = Dfs(s, double.PositiveInfinity)) > 1e-12)
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
            EdmondsKarp.Compute(network, source, sink);

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
            //Compute(network, source, sink);

            //var reachable = new HashSet<T> { source };
            //var queue = new Queue<T>();
            //queue.Enqueue(source);
            //while (queue.Count > 0)
            //{
            //    var u = queue.Dequeue();
            //    foreach (var v in network.Neighbors(u))
            //        if (!reachable.Contains(v) && network.GetCapacity(u, v) > 1e-12)
            //        {
            //            reachable.Add(v);
            //            queue.Enqueue(v);
            //        }
            //}
            //return reachable;
        }
    }
}