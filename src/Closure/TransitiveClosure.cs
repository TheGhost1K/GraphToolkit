using GraphToolkit.Core;
using GraphToolkit.Traversal;

namespace GraphToolkit.Closure
{
    /// <summary>
    /// Транзитивное замыкание и сокращение графа.
    /// </summary>
    public static class TransitiveClosure
    {
        /// <summary>
        /// Вычисляет транзитивное замыкание (алгоритм Флойда-Уоршелла в булевом варианте).
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Ориентированный граф.</param>
        /// <returns>Матрица достижимости: <c>[i, j] == true</c>, если j достижима из i.</returns>
        /// <remarks>Сложность: O(V³).</remarks>
        public static bool[,] Compute<T>(IGraph<T> graph) where T : notnull
        {
            var vertices = graph.Vertices.ToList();
            int n = vertices.Count;
            var index = vertices.Select((v, i) => (v, i))
                .ToDictionary(x => x.v, x => x.i);

            var reach = new bool[n, n];
            for (int i = 0; i < n; i++) reach[i, i] = true;

            foreach (var e in graph.Edges)
                reach[index[e.From], index[e.To]] = true;

            for (int k = 0; k < n; k++)
                for (int i = 0; i < n; i++)
                    if (reach[i, k])
                        for (int j = 0; j < n; j++)
                            if (reach[k, j])
                                reach[i, j] = true;

            return reach;
        }

        /// <summary>
        /// Возвращает транзитивное замыкание в виде словаря.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <returns>Словарь "вершина → множество достижимых вершин".</returns>
        public static Dictionary<T, HashSet<T>> ComputeDict<T>(IGraph<T> graph) where T : notnull
        {
            var vertices = graph.Vertices.ToList();
            var reach = Compute(graph);
            var result = new Dictionary<T, HashSet<T>>();

            for (int i = 0; i < vertices.Count; i++)
            {
                result[vertices[i]] = new HashSet<T>();
                for (int j = 0; j < vertices.Count; j++)
                    if (reach[i, j]) result[vertices[i]].Add(vertices[j]);
            }
            return result;
        }

        /// <summary>
        /// Транзитивное сокращение DAG — минимальный граф с тем же замыканием.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="dag">Ориентированный ациклический граф.</param>
        /// <returns>Новый граф — транзитивное сокращение.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Если граф содержит цикл.
        /// </exception>
        public static Graph<T> Reduce<T>(IGraph<T> dag) where T : notnull
        {
            _ = TopologicalSort.Sort(dag)
                ?? throw new System.InvalidOperationException("Граф должен быть ациклическим");

            var reach = Compute(dag);
            var vertices = dag.Vertices.ToList();
            int n = vertices.Count;
            var index = vertices.Select((v, i) => (v, i))
                .ToDictionary(x => x.v, x => x.i);

            var result = new Graph<T>(dag.IsDirected);
            foreach (var v in vertices) result.AddVertex(v);

            foreach (var e in dag.Edges)
            {
                int u = index[e.From], v = index[e.To];
                bool redundant = false;
                for (int w = 0; w < n; w++)
                {
                    if (w == u || w == v) continue;
                    if (reach[u, w] && reach[w, v])
                    {
                        redundant = true;
                        break;
                    }
                }
                if (!redundant) result.AddEdge(e.From, e.To, e.Weight);
            }
            return result;
        }
    }
}