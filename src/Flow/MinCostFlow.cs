using GraphToolkit.Core;

namespace GraphToolkit.Flow
{
    /// <summary>
    /// Алгоритмы потока минимальной стоимости (Min-Cost Max-Flow).
    /// </summary>
    public static class MinCostFlow
    {
        /// <summary>
        /// Решает задачу min-cost max-flow через SPFA (Bellman-Ford с очередью).
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="network">Сеть.</param>
        /// <param name="source">Источник.</param>
        /// <param name="sink">Сток.</param>
        /// <param name="maxFlow">Ограничение сверху на поток; <c>null</c> — без ограничения.</param>
        /// <returns>Кортеж: величина потока и его стоимость.</returns>
        /// <remarks>
        /// Работает с отрицательными стоимостями рёбер, но без отрицательных циклов.
        /// Сложность в худшем случае: O(V * E * F).
        /// </remarks>
        public static (double Flow, double Cost) Spfa<T>(
            CostFlowNetwork<T> network, T source, T sink, double? maxFlow = null)
            where T : notnull
        {
            int n = network.Graph.Count;
            int s = network.IndexOf(source);
            int t = network.IndexOf(sink);
            double totalFlow = 0, totalCost = 0;
            double flowLimit = maxFlow ?? double.PositiveInfinity;
            const double INF = double.PositiveInfinity;

            while (totalFlow < flowLimit)
            {
                var dist = new double[n];
                var inQueue = new bool[n];
                var prevV = new int[n];
                var prevE = new int[n];

                for (int i = 0; i < n; i++) { dist[i] = INF; prevV[i] = -1; prevE[i] = -1; }
                dist[s] = 0;

                var queue = new Queue<int>();
                queue.Enqueue(s);
                inQueue[s] = true;

                while (queue.Count > 0)
                {
                    int u = queue.Dequeue();
                    inQueue[u] = false;
                    for (int i = 0; i < network.Graph[u].Count; i++)
                    {
                        var e = network.Graph[u][i];
                        if (e.Cap > 1e-12 && dist[u] + e.Cost < dist[e.To] - 1e-12)
                        {
                            dist[e.To] = dist[u] + e.Cost;
                            prevV[e.To] = u;
                            prevE[e.To] = i;
                            if (!inQueue[e.To]) { queue.Enqueue(e.To); inQueue[e.To] = true; }
                        }
                    }
                }

                if (double.IsPositiveInfinity(dist[t])) break;

                double d = flowLimit - totalFlow;
                for (int v = t; v != s; v = prevV[v])
                    d = Math.Min(d, network.Graph[prevV[v]][prevE[v]].Cap);

                for (int v = t; v != s; v = prevV[v])
                {
                    var e = network.Graph[prevV[v]][prevE[v]];
                    e.Cap -= d;
                    network.Graph[v][e.Rev].Cap += d;
                }

                totalFlow += d;
                totalCost += d * dist[t];
            }
            return (totalFlow, totalCost);
        }

        /// <summary>
        /// Решает задачу min-cost max-flow через Дейкстру с потенциалами (метод Джонсона).
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="network">Сеть.</param>
        /// <param name="source">Источник.</param>
        /// <param name="sink">Сток.</param>
        /// <param name="maxFlow">Ограничение сверху на поток; <c>null</c> — без ограничения.</param>
        /// <returns>Кортеж: величина потока и его стоимость.</returns>
        /// <remarks>
        /// Быстрее SPFA на больших графах. Начальные потенциалы вычисляются 
        /// через Беллмана-Форда, что позволяет работать с отрицательными стоимостями.
        /// </remarks>
        public static (double Flow, double Cost) DijkstraWithPotentials<T>(
            CostFlowNetwork<T> network, T source, T sink, double? maxFlow = null)
            where T : notnull
        {
            int n = network.Graph.Count;
            int s = network.IndexOf(source);
            int t = network.IndexOf(sink);
            double totalFlow = 0, totalCost = 0;
            double flowLimit = maxFlow ?? double.PositiveInfinity;
            const double INF = double.PositiveInfinity;

            // Начальные потенциалы через Беллмана-Форда
            var h = new double[n];
            for (int i = 0; i < n; i++) h[i] = INF;
            h[s] = 0;

            bool updated = true;
            for (int iter = 0; iter < n && updated; iter++)
            {
                updated = false;
                for (int u = 0; u < n; u++)
                {
                    if (double.IsPositiveInfinity(h[u])) continue;
                    foreach (var e in network.Graph[u])
                        if (e.Cap > 1e-12 && h[u] + e.Cost < h[e.To] - 1e-12)
                        {
                            h[e.To] = h[u] + e.Cost;
                            updated = true;
                        }
                }
            }
            for (int i = 0; i < n; i++)
                if (double.IsPositiveInfinity(h[i])) h[i] = 0;

            while (totalFlow < flowLimit)
            {
                var dist = new double[n];
                var prevV = new int[n];
                var prevE = new int[n];
                for (int i = 0; i < n; i++) { dist[i] = INF; prevV[i] = -1; prevE[i] = -1; }
                dist[s] = 0;

                var pq = new PriorityQueue<int, double>();
                pq.Enqueue(s, 0);

                while (pq.TryDequeue(out int u, out double d))
                {
                    if (d > dist[u] + 1e-12) continue;
                    for (int i = 0; i < network.Graph[u].Count; i++)
                    {
                        var e = network.Graph[u][i];
                        if (e.Cap < 1e-12) continue;
                        double nd = dist[u] + e.Cost + h[u] - h[e.To];
                        if (nd < dist[e.To] - 1e-12)
                        {
                            dist[e.To] = nd;
                            prevV[e.To] = u;
                            prevE[e.To] = i;
                            pq.Enqueue(e.To, nd);
                        }
                    }
                }

                if (double.IsPositiveInfinity(dist[t])) break;

                for (int i = 0; i < n; i++)
                    if (!double.IsPositiveInfinity(dist[i])) h[i] += dist[i];

                double f = flowLimit - totalFlow;
                for (int v = t; v != s; v = prevV[v])
                    f = Math.Min(f, network.Graph[prevV[v]][prevE[v]].Cap);

                for (int v = t; v != s; v = prevV[v])
                {
                    var e = network.Graph[prevV[v]][prevE[v]];
                    e.Cap -= f;
                    network.Graph[v][e.Rev].Cap += f;
                }

                totalFlow += f;
                totalCost += f * h[t];
            }
            return (totalFlow, totalCost);
        }
    }
}