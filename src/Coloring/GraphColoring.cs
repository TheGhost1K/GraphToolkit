using GraphToolkit.Core;

namespace GraphToolkit.Coloring
{
    /// <summary>
    /// Алгоритмы раскраски графа.
    /// </summary>
    public static class GraphColoring
    {
        /// <summary>
        /// Жадная раскраска в порядке убывания степеней (Уэлш-Пауэлл).
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <returns>Словарь "вершина → номер цвета" (нумерация с 0).</returns>
        /// <remarks>Сложность: O(V² + E).</remarks>
        public static Dictionary<T, int> Greedy<T>(IGraph<T> graph) where T : notnull
        {
            var colors = new Dictionary<T, int>();
            var ordered = graph.Vertices
                .OrderByDescending(v => graph.Neighbors(v).Count())
                .ToList();

            foreach (var v in ordered)
            {
                var used = new HashSet<int>();
                foreach (var e in graph.Neighbors(v))
                    if (colors.TryGetValue(e.To, out int c)) used.Add(c);

                int color = 0;
                while (used.Contains(color)) color++;
                colors[v] = color;
            }
            return colors;
        }

        /// <summary>
        /// Точное хроматическое число через backtracking с эвристикой DSATUR.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <returns>
        /// Кортеж: оптимальная раскраска и хроматическое число.
        /// </returns>
        /// <remarks>
        /// Сложность экспоненциальная. Практически применимо для графов 
        /// до нескольких десятков вершин.
        /// </remarks>
        public static (Dictionary<T, int> Colors, int ChromaticNumber)
            Exact<T>(IGraph<T> graph) where T : notnull
        {
            var vertices = graph.Vertices.ToList();
            int n = vertices.Count;
            if (n == 0) return (new Dictionary<T, int>(), 0);

            var index = vertices.Select((v, i) => (v, i))
                .ToDictionary(x => x.v, x => x.i);

            var adjacency = new List<int>[n];
            for (int i = 0; i < n; i++) adjacency[i] = new List<int>();
            foreach (var e in graph.Edges)
            {
                int u = index[e.From], v = index[e.To];
                if (!adjacency[u].Contains(v)) adjacency[u].Add(v);
                if (!graph.IsDirected && !adjacency[v].Contains(u)) adjacency[v].Add(u);
            }

            var result = Enumerable.Repeat(-1, n).ToArray();
            var greedy = Greedy(graph);
            int bestUsed = greedy.Values.Max() + 1;
            var bestColors = new int[n];
            for (int i = 0; i < n; i++) bestColors[i] = greedy[vertices[i]];

            void Dfs(int colored, int usedColors)
            {
                if (usedColors >= bestUsed) return;
                if (colored == n)
                {
                    bestUsed = usedColors;
                    System.Array.Copy(result, bestColors, n);
                    return;
                }

                int bestV = -1, bestSat = -1, bestDeg = -1;
                for (int v = 0; v < n; v++)
                {
                    if (result[v] != -1) continue;
                    var sat = new HashSet<int>();
                    foreach (var u in adjacency[v])
                        if (result[u] != -1) sat.Add(result[u]);
                    int satCount = sat.Count;
                    int deg = adjacency[v].Count;
                    if (satCount > bestSat || (satCount == bestSat && deg > bestDeg))
                    {
                        bestSat = satCount; bestDeg = deg; bestV = v;
                    }
                }

                var used = new HashSet<int>();
                foreach (var u in adjacency[bestV])
                    if (result[u] != -1) used.Add(result[u]);

                for (int c = 0; c <= usedColors; c++)
                {
                    if (used.Contains(c)) continue;
                    result[bestV] = c;
                    Dfs(colored + 1, System.Math.Max(usedColors, c + 1));
                    result[bestV] = -1;
                }
            }

            Dfs(0, 0);

            var colors = new Dictionary<T, int>();
            for (int i = 0; i < n; i++) colors[vertices[i]] = bestColors[i];
            return (colors, bestUsed);
        }

        /// <summary>
        /// Проверяет двудольность графа (2-раскрашиваемость).
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <param name="coloring">Выходная раскраска (если граф двудольный).</param>
        /// <returns><c>true</c>, если граф двудольный.</returns>
        public static bool IsBipartite<T>(IGraph<T> graph, out Dictionary<T, int> coloring)
            where T : notnull
        {
            coloring = new Dictionary<T, int>();
            var queue = new Queue<T>();

            foreach (var start in graph.Vertices)
            {
                if (coloring.ContainsKey(start)) continue;
                coloring[start] = 0;
                queue.Enqueue(start);

                while (queue.Count > 0)
                {
                    var u = queue.Dequeue();
                    foreach (var e in graph.Neighbors(u))
                    {
                        if (!coloring.TryGetValue(e.To, out int value))
                        {
                            coloring[e.To] = 1 - coloring[u];
                            queue.Enqueue(e.To);
                        }
                        else if (value == coloring[u])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}