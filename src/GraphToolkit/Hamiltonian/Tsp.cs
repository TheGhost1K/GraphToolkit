using GraphToolkit.Core;

namespace GraphToolkit.Hamiltonian
{
    /// <summary>
    /// Решения задачи коммивояжёра (TSP): точное и приближённое.
    /// </summary>
    public static class Tsp
    {
        /// <summary>
        /// Результат решения TSP.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="Path">Найденный маршрут (или <c>null</c>, если маршрут не найден).</param>
        /// <param name="Cost">Суммарная стоимость маршрута.</param>
        public sealed record Result<T>(List<T>? Path, double Cost);

        /// <summary>
        /// Точное решение TSP методом ветвей и границ.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Полный (или почти полный) взвешенный граф.</param>
        /// <param name="start">Начальная вершина маршрута.</param>
        /// <returns>Оптимальный маршрут и его стоимость.</returns>
        /// <remarks>
        /// Сложность O(V² * 2^V) в худшем случае. Используется нижняя оценка 
        /// через сумму двух минимальных рёбер / 2 для отсечения.
        /// </remarks>
        public static Result<T> BranchAndBound<T>(IGraph<T> graph, T start) where T : notnull
        {
            var vertices = graph.Vertices.ToList();
            int n = vertices.Count;
            if (n == 0) return new Result<T>(null, 0);

            var index = vertices.Select((v, i) => (v, i))
                .ToDictionary(x => x.v, x => x.i);
            var dist = BuildDistanceMatrix(graph, vertices, index);

            double LowerBound(bool[] visited)
            {
                double sum = 0;
                for (int i = 0; i < n; i++)
                {
                    if (visited[i] && i != 0) continue;
                    double m1 = double.PositiveInfinity, m2 = double.PositiveInfinity;
                    for (int j = 0; j < n; j++)
                    {
                        if (i == j) continue;
                        double d = dist[i, j];
                        if (d < m1) { m2 = m1; m1 = d; }
                        else if (d < m2) m2 = d;
                    }
                    if (double.IsPositiveInfinity(m1)) continue;
                    sum += double.IsPositiveInfinity(m2) ? m1 : m1 + m2;
                }
                return sum / 2;
            }

            double bestCost = double.PositiveInfinity;
            List<int>? bestPath = null;
            var currentPath = new List<int> { index[start] };
            var visitedArr = new bool[n];
            visitedArr[index[start]] = true;

            void Branch(int current, double cost, int depth)
            {
                if (depth == n)
                {
                    double total = cost + dist[current, index[start]];
                    if (total < bestCost)
                    {
                        bestCost = total;
                        bestPath = new List<int>(currentPath) { index[start] };
                    }
                    return;
                }

                if (cost + LowerBound(visitedArr) >= bestCost) return;

                for (int next = 0; next < n; next++)
                {
                    if (visitedArr[next] || double.IsPositiveInfinity(dist[current, next]))
                        continue;
                    double newCost = cost + dist[current, next];
                    if (newCost >= bestCost) continue;

                    visitedArr[next] = true;
                    currentPath.Add(next);
                    Branch(next, newCost, depth + 1);
                    currentPath.RemoveAt(currentPath.Count - 1);
                    visitedArr[next] = false;
                }
            }

            Branch(index[start], 0, 1);

            if (bestPath == null) return new Result<T>(null, double.PositiveInfinity);
            return new Result<T>(bestPath.Select(i => vertices[i]).ToList(), bestCost);
        }

        /// <summary>
        /// Приближённое решение TSP: ближайший сосед + улучшение 2-opt.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <param name="start">Начальная вершина.</param>
        /// <returns>Маршрут и его стоимость.</returns>
        /// <remarks>
        /// Сложность O(V²) + O(V²) для 2-opt. Обычно даёт результат в пределах 
        /// 5–25% от оптимума.
        /// </remarks>
        public static Result<T> NearestNeighbor<T>(IGraph<T> graph, T start) where T : notnull
        {
            var vertices = graph.Vertices.ToList();
            int n = vertices.Count;
            if (n == 0) return new Result<T>(null, 0);

            var index = vertices.Select((v, i) => (v, i))
                .ToDictionary(x => x.v, x => x.i);
            var dist = BuildDistanceMatrix(graph, vertices, index);

            var visited = new bool[n];
            var path = new List<int> { index[start] };
            visited[index[start]] = true;
            int cur = index[start];
            double cost = 0;

            for (int step = 1; step < n; step++)
            {
                int best = -1;
                double bestD = double.PositiveInfinity;
                for (int j = 0; j < n; j++)
                {
                    if (visited[j]) continue;
                    if (dist[cur, j] < bestD) { bestD = dist[cur, j]; best = j; }
                }
                if (best == -1) return new Result<T>(null, double.PositiveInfinity);
                visited[best] = true;
                cost += bestD;
                cur = best;
                path.Add(best);
            }
            cost += dist[cur, index[start]];
            path.Add(index[start]);

            // 2-opt
            bool improved = true;
            while (improved)
            {
                improved = false;
                for (int i = 1; i < path.Count - 2; i++)
                    for (int k = i + 1; k < path.Count - 1; k++)
                    {
                        int a = path[i - 1], b = path[i], c = path[k], d = path[k + 1];
                        double delta = dist[a, c] + dist[b, d] - dist[a, b] - dist[c, d];
                        if (delta < -1e-9)
                        {
                            path.Reverse(i, k - i + 1);
                            cost += delta;
                            improved = true;
                        }
                    }
            }

            return new Result<T>(path.Select(i => vertices[i]).ToList(), cost);
        }

        private static double[,] BuildDistanceMatrix<T>(
            IGraph<T> graph, List<T> vertices, Dictionary<T, int> index)
            where T : notnull
        {
            int n = vertices.Count;
            var dist = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    dist[i, j] = i == j ? 0 : double.PositiveInfinity;

            foreach (var e in graph.Edges)
            {
                int u = index[e.From], v = index[e.To];
                if (e.Weight < dist[u, v]) dist[u, v] = e.Weight;
                if (!graph.IsDirected && e.Weight < dist[v, u]) dist[v, u] = e.Weight;
            }
            return dist;
        }
    }
}