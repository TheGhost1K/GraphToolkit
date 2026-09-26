namespace GraphToolkit.Core
{
    /// <summary>
    /// Сеть для задач максимального потока: ориентированный граф
    /// с пропускными способностями рёбер.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <remarks>
    /// <para>
    /// <b>Важно:</b> алгоритмы max-flow (<see cref="Flow.Dinic.Compute{T}"/>,
    /// <see cref="Flow.EdmondsKarp.Compute{T}"/>, <see cref="Flow.FordFulkerson.Compute{T}"/>)
    /// <b>модифицируют</b> сеть — уменьшают остаточные пропускные способности.
    /// Если нужно запустить несколько алгоритмов на одной сети,
    /// используйте <see cref="Clone"/> перед каждым запуском.
    /// </para>
    /// </remarks>
    public sealed class FlowNetwork<T> where T : notnull
    {
        private readonly Dictionary<T, Dictionary<T, double>> _capacity = new();
        private readonly HashSet<T> _vertices = new();

        /// <summary>
        /// Добавляет вершину в сеть.
        /// </summary>
        /// <param name="v">Вершина.</param>
        public void AddVertex(T v) => _vertices.Add(v);

        /// <summary>
        /// Добавляет направленное ребро с указанной пропускной способностью.
        /// </summary>
        /// <param name="from">Источник.</param>
        /// <param name="to">Приёмник.</param>
        /// <param name="capacity">Пропускная способность (должна быть неотрицательной).</param>
        /// <remarks>
        /// Если ребро уже существует, пропускные способности суммируются.
        /// </remarks>
        public void AddEdge(T from, T to, double capacity)
        {
            _vertices.Add(from);
            _vertices.Add(to);

            if (!_capacity.ContainsKey(from)) _capacity[from] = new Dictionary<T, double>();
            if (!_capacity.ContainsKey(to)) _capacity[to] = new Dictionary<T, double>();

            _capacity[from][to] = _capacity[from].GetValueOrDefault(to) + capacity;
            if (!_capacity[to].ContainsKey(from)) _capacity[to][from] = 0;
        }

        /// <summary>
        /// Возвращает все вершины, смежные с указанной (в любом направлении).
        /// </summary>
        /// <param name="v">Вершина.</param>
        /// <returns>Перечисление смежных вершин.</returns>
        public IEnumerable<T> Neighbors(T v)
            => _capacity.TryGetValue(v, out var d) ? d.Keys : Enumerable.Empty<T>();

        /// <summary>
        /// Возвращает текущую остаточную пропускную способность ребра.
        /// </summary>
        /// <param name="u">Источник.</param>
        /// <param name="v">Приёмник.</param>
        /// <returns>Остаточная пропускная способность, или 0, если ребра нет.</returns>
        public double GetCapacity(T u, T v)
            => _capacity.TryGetValue(u, out var d) ? d.GetValueOrDefault(v) : 0;

        /// <summary>
        /// Изменяет остаточные пропускные способности прямого и обратного рёбер.
        /// </summary>
        /// <param name="u">Источник.</param>
        /// <param name="v">Приёмник.</param>
        /// <param name="delta">Величина изменения (положительная для увеличения потока).</param>
        internal void UpdateCapacity(T u, T v, double delta)
        {
            _capacity[u][v] -= delta;
            _capacity[v][u] = _capacity[v].GetValueOrDefault(u) + delta;
        }

        /// <summary>
        /// Все вершины сети.
        /// </summary>
        public IEnumerable<T> Vertices => _vertices;

        /// <summary>
        /// Создаёт полную копию сети: те же вершины, рёбра и пропускные способности.
        /// </summary>
        /// <returns>Новый независимый экземпляр <see cref="FlowNetwork{T}"/>.</returns>
        /// <remarks>
        /// <para>
        /// Полезно, когда нужно запустить несколько алгоритмов max-flow
        /// на одной и той же исходной сети: каждый алгоритм модифицирует
        /// сеть, поэтому клонируйте её перед каждым запуском.
        /// </para>
        /// <para>
        /// <b>Сложность:</b> O(V + E).
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var net = new FlowNetwork&lt;string&gt;();
        /// net.AddEdge("S", "A", 10);
        /// net.AddEdge("A", "T", 5);
        /// 
        /// double f1 = Dinic.Compute(net.Clone(), "S", "T");
        /// double f2 = EdmondsKarp.Compute(net.Clone(), "S", "T");
        /// // net остался нетронутым
        /// </code>
        /// </example>
        public FlowNetwork<T> Clone()
        {
            var copy = new FlowNetwork<T>();

            foreach (var v in _vertices)
                copy._vertices.Add(v);

            foreach (var (from, edges) in _capacity)
            {
                foreach (var (to, cap) in edges)
                {
                    // Копируем только "прямые" рёбра — те, у которых исходная
                    // capacity > 0 или которые есть в _capacity[from] явно.
                    // Обратные рёбра (cap = 0) тоже копируем, чтобы сохранить
                    // структуру остаточной сети.
                    if (!copy._capacity.ContainsKey(from))
                        copy._capacity[from] = new Dictionary<T, double>();

                    copy._capacity[from][to] = cap;
                }
            }

            // Восстанавливаем обратные рёбра нулевой пропускной способности,
            // которых может не быть в исходной _capacity, но они нужны
            // для корректной работы UpdateCapacity.
            foreach (var v in _vertices)
                if (!copy._capacity.ContainsKey(v))
                    copy._capacity[v] = new Dictionary<T, double>();

            return copy;
        }
    }
}