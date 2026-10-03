using GraphToolkit.Core;

namespace GraphToolkit.Trees
{
    /// <summary>
    /// Наименьший общий предок (LCA) с бинарным подъёмом.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <remarks>
    /// <para>Предобработка: O(V log V).</para>
    /// <para>Запрос: O(log V).</para>
    /// </remarks>
    public sealed class Lca<T> where T : notnull
    {
        private readonly Dictionary<T, int> _depth = new();
        private readonly Dictionary<T, Dictionary<int, T>> _up = new();
        private readonly int _log;

        /// <summary>
        /// Инициализирует структуру для указанного дерева и корня.
        /// </summary>
        /// <param name="tree">Дерево.</param>
        /// <param name="root">Корневая вершина.</param>
        public Lca(IGraph<T> tree, T root)
        {
            var vertices = tree.Vertices.ToList();
            _log = Math.Max(1, (int)Math.Ceiling(Math.Log2(vertices.Count + 1)));

            var parent = new Dictionary<T, T>();
            var queue = new Queue<T>();
            queue.Enqueue(root);
            _depth[root] = 0;

            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                foreach (var e in tree.Neighbors(u))
                {
                    if (_depth.ContainsKey(e.To)) continue;
                    _depth[e.To] = _depth[u] + 1;
                    parent[e.To] = u;
                    queue.Enqueue(e.To);
                }
            }

            foreach (var v in vertices)
            {
                _up[v] = new Dictionary<int, T> { [0] = parent.GetValueOrDefault(v, v) };
            }

            for (int k = 1; k < _log; k++)
                foreach (var v in vertices)
                {
                    var mid = _up[v][k - 1];
                    _up[v][k] = _up.TryGetValue(mid, out Dictionary<int, T>? value) && value.ContainsKey(k - 1)
                        ? value[k - 1] : mid;
                }
        }

        /// <summary>
        /// Находит наименьшего общего предка двух вершин.
        /// </summary>
        /// <param name="u">Первая вершина.</param>
        /// <param name="v">Вторая вершина.</param>
        /// <returns>Вершина — LCA.</returns>
        public T Query(T u, T v)
        {
            if (_depth[u] < _depth[v]) (u, v) = (v, u);

            int diff = _depth[u] - _depth[v];
            for (int k = 0; k < _log; k++)
                if ((diff & (1 << k)) != 0)
                    u = _up[u][k];

            if (EqualityComparer<T>.Default.Equals(u, v)) return u;

            for (int k = _log - 1; k >= 0; k--)
                if (!EqualityComparer<T>.Default.Equals(_up[u][k], _up[v][k]))
                {
                    u = _up[u][k];
                    v = _up[v][k];
                }
            return _up[u][0];
        }
    }
}