using GraphToolkit.Core;

namespace GraphToolkit.ShortestPaths
{
    /// <summary>
    /// Алгоритм Флойда-Уоршелла — все пары кратчайших путей.
    /// </summary>
    /// <remarks>
    /// Сложность: O(V³). Поддерживает отрицательные веса, но не отрицательные циклы.
    /// </remarks>
    public static class FloydWarshall
    {
        /// <summary>
        /// Результат работы алгоритма Флойда-Уоршелла.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="Distances">
        /// Матрица расстояний <c>[i, j]</c> между вершинами с индексами <c>i</c> и <c>j</c>.
        /// </param>
        /// <param name="Next">
        /// Матрица следующих вершин: <c>Next[i, j]</c> — индекс вершины, следующей 
        /// после <c>i</c> на пути к <c>j</c>; <c>-1</c>, если пути нет.
        /// </param>
        /// <param name="Vertices">
        /// Список вершин, соответствующих индексам матриц.
        /// </param>
        public sealed record Result<T>(
            double[,] Distances,
            int[,] Next,
            IReadOnlyList<T> Vertices);

        /// <summary>
        /// Запускает алгоритм Флойда-Уоршелла.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <returns>Результат с матрицами расстояний и следующих вершин.</returns>
        public static Result<T> Compute<T>(IGraph<T> graph) where T : notnull
        {
            var vertices = graph.Vertices.ToList();
            var index = vertices.Select((v, i) => (v, i))
                .ToDictionary(x => x.v, x => x.i);
            int n = vertices.Count;

            var dist = new double[n, n];
            var next = new int[n, n];

            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    dist[i, j] = i == j ? 0 : double.PositiveInfinity;
                    next[i, j] = -1;
                }

            foreach (var e in graph.Edges)
            {
                int u = index[e.From], v = index[e.To];
                if (e.Weight < dist[u, v])
                {
                    dist[u, v] = e.Weight;
                    next[u, v] = v;
                }
                if (!graph.IsDirected && e.Weight < dist[v, u])
                {
                    dist[v, u] = e.Weight;
                    next[v, u] = u;
                }
            }

            for (int k = 0; k < n; k++)
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        if (dist[i, k] + dist[k, j] < dist[i, j])
                        {
                            dist[i, j] = dist[i, k] + dist[k, j];
                            next[i, j] = next[i, k];
                        }

            return new Result<T>(dist, next, vertices);
        }

        /// <summary>
        /// Восстанавливает путь между двумя вершинами.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <param name="from">Начальная вершина.</param>
        /// <param name="to">Целевая вершина.</param>
        /// <param name="distance">Выходной параметр: расстояние между вершинами.</param>
        /// <returns>Список вершин пути или <c>null</c>, если пути нет.</returns>
        public static List<T>? FindPath<T>(
            IGraph<T> graph, T from, T to, out double distance) where T : notnull
        {
            var result = Compute(graph);
            var vertices = result.Vertices.ToList();
            var index = vertices.Select((v, i) => (v, i))
                .ToDictionary(x => x.v, x => x.i);

            int u = index[from], v = index[to];
            distance = result.Distances[u, v];
            if (result.Next[u, v] == -1) return null;

            var path = new List<T> { from };
            while (u != v)
            {
                u = result.Next[u, v];
                path.Add(vertices[u]);
            }
            return path;
        }
    }
}