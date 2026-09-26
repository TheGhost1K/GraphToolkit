namespace GraphToolkit.Core
{
    /// <summary>
    /// Сеть для задач потока минимальной стоимости: каждое ребро
    /// имеет пропускную способность и стоимость единицы потока.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <remarks>
    /// <para>
    /// <b>Важно:</b> алгоритмы min-cost flow
    /// (<see cref="Flow.MinCostFlow.Spfa{T}"/> и
    /// <see cref="Flow.MinCostFlow.DijkstraWithPotentials{T}"/>)
    /// <b>модифицируют</b> сеть. Используйте <see cref="Clone"/>
    /// перед каждым запуском, если нужно сравнить результаты
    /// разных алгоритмов.
    /// </para>
    /// </remarks>
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

        /// <summary>
        /// Создаёт полную копию сети.
        /// </summary>
        /// <returns>Новый независимый экземпляр <see cref="CostFlowNetwork{T}"/>.</returns>
        /// <remarks>
        /// <para>
        /// Копируются вершины, все прямые и обратные рёбра, а также
        /// текущее состояние остаточных пропускных способностей
        /// (то есть сеть может быть уже частично «использована»).
        /// </para>
        /// <para>
        /// <b>Сложность:</b> O(V + E).
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var net = new CostFlowNetwork&lt;string&gt;();
        /// net.AddEdge("S", "A", 4, 2);
        /// net.AddEdge("A", "T", 4, 3);
        /// 
        /// var (f1, c1) = MinCostFlow.Spfa(net.Clone(), "S", "T");
        /// var (f2, c2) = MinCostFlow.DijkstraWithPotentials(net.Clone(), "S", "T");
        /// // Результаты должны совпасть
        /// </code>
        /// </example>
        public CostFlowNetwork<T> Clone()
        {
            var copy = new CostFlowNetwork<T>();

            // Копируем вершины и их индексы
            foreach (var v in _vertices)
            {
                copy.GetOrAddIndex(v);
            }

            // Копируем рёбра (включая обратные) с сохранением структуры
            for (int u = 0; u < Graph.Count; u++)
            {
                foreach (var arc in Graph[u])
                {
                    copy.Graph[u].Add(new Arc
                    {
                        To = arc.To,
                        Rev = arc.Rev,
                        Cap = arc.Cap,
                        Cost = arc.Cost
                    });
                }
            }

            return copy;
        }
    }
}