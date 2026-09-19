using GraphToolkit.Core;

namespace GraphToolkit.Components
{
    /// <summary>
    /// Поиск мостов и точек сочленения (алгоритм Тарьяна).
    /// </summary>
    /// <remarks>
    /// Сложность: O(V + E). Работает только для неориентированных графов.
    /// </remarks>
    public static class BridgesAndArticulation
    {
        /// <summary>
        /// Результат поиска мостов и точек сочленения.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="Bridges">
        /// Список мостов — рёбер, удаление которых увеличивает число компонент связности.
        /// </param>
        /// <param name="ArticulationPoints">
        /// Множество точек сочленения — вершин, удаление которых увеличивает 
        /// число компонент связности.
        /// </param>
        public sealed record Result<T>(
            List<(T From, T To)> Bridges,
            HashSet<T> ArticulationPoints);

        /// <summary>
        /// Находит мосты и точки сочленения.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Неориентированный граф.</param>
        /// <returns>Результат с мостами и точками сочленения.</returns>
        public static Result<T> Find<T>(IGraph<T> graph) where T : notnull
        {
            var disc = new Dictionary<T, int>();
            var low = new Dictionary<T, int>();
            var parent = new Dictionary<T, T>();
            var bridges = new List<(T, T)>();
            var articulation = new HashSet<T>();
            int time = 0;

            void Dfs(T u)
            {
                disc[u] = low[u] = ++time;
                int children = 0;

                foreach (var e in graph.Neighbors(u))
                {
                    var v = e.To;
                    if (!disc.TryGetValue(v, out int value))
                    {
                        children++;
                        parent[v] = u;
                        Dfs(v);
                        low[u] = Math.Min(low[u], low[v]);

                        if (low[v] > disc[u]) bridges.Add((u, v));
                        if (!parent.ContainsKey(u) && children > 1) articulation.Add(u);
                        if (parent.ContainsKey(u) && low[v] >= disc[u]) articulation.Add(u);
                    }
                    else if (!EqualityComparer<T>.Default.Equals(
                        v, parent.GetValueOrDefault(u)))
                    {
                        low[u] = Math.Min(low[u], value);
                    }
                }
            }

            foreach (var v in graph.Vertices)
                if (!disc.ContainsKey(v)) Dfs(v);

            return new Result<T>(bridges, articulation);
        }
    }
}