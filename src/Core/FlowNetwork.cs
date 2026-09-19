namespace GraphToolkit.Core
{
    /// <summary>
    /// Сеть для задач максимального потока: ориентированный граф 
    /// с пропускными способностями рёбер.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
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
    }
}