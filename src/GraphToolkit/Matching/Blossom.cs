using GraphToolkit.Core;

namespace GraphToolkit.Matching
{
    /// <summary>
    /// Максимальное паросочетание в произвольном графе (алгоритм Эдмондса / Blossom).
    /// </summary>
    /// <remarks>
    /// Сложность: O(V³). Работает с недвудольными графами.
    /// </remarks>
    public static class Blossom
    {
        /// <summary>
        /// Находит максимальное паросочетание.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Неориентированный граф.</param>
        /// <returns>Список пар вершин.</returns>
        public static List<(T, T)> Compute<T>(IGraph<T> graph) where T : notnull
        {
            var vertices = graph.Vertices.ToList();
            int n = vertices.Count;
            if (n == 0) return new List<(T, T)>();

            var index = vertices.Select((v, i) => (v, i))
                .ToDictionary(x => x.v, x => x.i);

            var adj = new bool[n, n];
            foreach (var e in graph.Edges)
            {
                int u = index[e.From], v = index[e.To];
                adj[u, v] = true;
                if (!graph.IsDirected) adj[v, u] = true;
            }

            var match = Enumerable.Repeat(-1, n).ToArray();
            var p = new int[n];
            var baseV = new int[n];
            var q = new int[n];
            var used = new bool[n];
            var blossom = new bool[n];

            int Lca(int a, int b)
            {
                var usedPath = new bool[n];
                while (true)
                {
                    a = baseV[a];
                    usedPath[a] = true;
                    if (match[a] == -1) break;
                    a = p[match[a]];
                }
                while (true)
                {
                    b = baseV[b];
                    if (usedPath[b]) return b;
                    b = p[match[b]];
                }
            }

            void MarkPath(int v, int b, int child)
            {
                while (baseV[v] != b)
                {
                    blossom[baseV[v]] = true;
                    blossom[baseV[match[v]]] = true;
                    p[v] = child;
                    child = match[v];
                    v = p[match[v]];
                }
            }

            int FindPath(int root)
            {
                for (int i = 0; i < n; i++) { used[i] = false; p[i] = -1; baseV[i] = i; }
                used[root] = true;
                int qh = 0, qt = 0;
                q[qt++] = root;

                while (qh < qt)
                {
                    int v = q[qh++];
                    for (int to = 0; to < n; to++)
                    {
                        if (!adj[v, to] || baseV[v] == baseV[to] || match[v] == to) continue;

                        if (to == root || (match[to] != -1 && p[match[to]] != -1))
                        {
                            int curbase = Lca(v, to);
                            for (int i = 0; i < n; i++) blossom[i] = false;
                            MarkPath(v, curbase, to);
                            MarkPath(to, curbase, v);

                            for (int i = 0; i < n; i++)
                                if (blossom[baseV[i]])
                                {
                                    baseV[i] = curbase;
                                    if (!used[i]) { used[i] = true; q[qt++] = i; }
                                }
                        }
                        else if (p[to] == -1)
                        {
                            p[to] = v;
                            if (match[to] == -1) return to;
                            used[match[to]] = true;
                            q[qt++] = match[to];
                        }
                    }
                }
                return -1;
            }

            // Жадное начальное паросочетание
            for (int u = 0; u < n; u++)
            {
                if (match[u] != -1) continue;
                for (int v = 0; v < n; v++)
                {
                    if (u == v || !adj[u, v] || match[v] != -1) continue;
                    match[u] = v; match[v] = u;
                    break;
                }
            }

            for (int i = 0; i < n; i++)
            {
                if (match[i] != -1) continue;
                int v = FindPath(i);
                while (v != -1)
                {
                    int pv = p[v], ppv = match[pv];
                    match[v] = pv; match[pv] = v;
                    v = ppv;
                }
            }

            var result = new List<(T, T)>();
            var seen = new HashSet<int>();
            for (int i = 0; i < n; i++)
                if (match[i] != -1 && seen.Add(i) && seen.Add(match[i]))
                    result.Add((vertices[i], vertices[match[i]]));
            return result;
        }

        /// <summary>
        /// Проверяет, есть ли в графе совершенное паросочетание.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Неориентированный граф.</param>
        /// <returns><c>true</c>, если существует паросочетание, покрывающее все вершины.</returns>
        public static bool HasPerfectMatching<T>(IGraph<T> graph) where T : notnull
        {
            if (graph.VertexCount % 2 != 0) return false;
            return Compute(graph).Count * 2 == graph.VertexCount;
        }
    }
}