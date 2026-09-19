namespace GraphToolkit.Core
{
    /// <summary>
    /// Сеть для задач потока минимальной стоимости: каждое ребро 
    /// имеет пропускную способность и стоимость единицы потока.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    public sealed class CostFlowNetwork<T> where T : notnull
    {
        internal sealed class Arc
        {
            public int To;
            public int Rev;
            public double Cap;
            public double Cost;
        }

        private readonly List<T> _vertices = new();
        private readonly Dictionary<T, int> _index = new();
        internal readonly List<List<Arc>> Graph = new();

        /// <summary>
        /// Возвращает индекс вершины или добавляет её в сеть.
        /// </summary>
        /// <param name="v">Вершина.</param>
        /// <returns>Индекс вершины.</returns>
        public int GetOrAddIndex(T v)
        {
            if (_index.TryGetValue(v, out var i)) return i;
            i = _vertices.Count;
            _index[v] = i;
            _vertices.Add(v);
            Graph.Add(new List<Arc>());
            return i;
        }

        /// <summary>
        /// Добавляет направленное ребро с пропускной способностью и стоимостью.
        /// </summary>
        /// <param name="from">Источник.</param>
        /// <param name="to">Приёмник.</param>
        /// <param name="capacity">Пропускная способность.</param>
        /// <param name="cost">Стоимость единицы потока.</param>
        public void AddEdge(T from, T to, double capacity, double cost)
        {
            int u = GetOrAddIndex(from);
            int v = GetOrAddIndex(to);

            var a = new Arc { To = v, Cap = capacity, Cost = cost, Rev = Graph[v].Count };
            var b = new Arc { To = u, Cap = 0, Cost = -cost, Rev = Graph[u].Count };
            Graph[u].Add(a);
            Graph[v].Add(b);
        }

        /// <summary>
        /// Возвращает индекс указанной вершины.
        /// </summary>
        /// <param name="v">Вершина.</param>
        /// <returns>Индекс вершины.</returns>
        public int IndexOf(T v) => GetOrAddIndex(v);

        /// <summary>
        /// Все вершины сети.
        /// </summary>
        public IReadOnlyList<T> Vertices => _vertices;
    }
}