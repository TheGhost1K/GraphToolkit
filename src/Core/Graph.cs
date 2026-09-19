namespace GraphToolkit.Core
{
    /// <summary>
    /// Универсальный взвешенный граф, поддерживающий ориентированный 
    /// и неориентированный режимы.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <remarks>
    /// <para>
    /// Внутренне хранит список смежности в виде <see cref="Dictionary{TKey, TValue}"/>,
    /// что даёт O(1) доступ к соседям вершины.
    /// </para>
    /// <para>
    /// Пример создания графа:
    /// <code>
    /// var graph = new Graph&lt;string&gt;(isDirected: true);
    /// graph.AddEdge("A", "B", 4);
    /// graph.AddEdge("B", "C", 2);
    /// </code>
    /// </para>
    /// </remarks>
    public sealed class Graph<T> : IGraph<T> where T : notnull
    {
        private readonly Dictionary<T, List<Edge<T>>> _adjacency;
        private readonly List<Edge<T>> _edges;

        /// <inheritdoc/>
        public bool IsDirected { get; }

        /// <inheritdoc/>
        public IEnumerable<T> Vertices => _adjacency.Keys;

        /// <inheritdoc/>
        public IReadOnlyList<Edge<T>> Edges => _edges;

        /// <inheritdoc/>
        public int VertexCount => _adjacency.Count;

        /// <inheritdoc/>
        public int EdgeCount => _edges.Count;

        /// <summary>
        /// Инициализирует пустой граф.
        /// </summary>
        /// <param name="isDirected">
        /// <c>true</c> для ориентированного графа; <c>false</c> для неориентированного.
        /// </param>
        public Graph(bool isDirected = false)
        {
            IsDirected = isDirected;
            _adjacency = new Dictionary<T, List<Edge<T>>>();
            _edges = new List<Edge<T>>();
        }

        /// <summary>
        /// Добавляет вершину в граф. Если вершина уже существует — ничего не делает.
        /// </summary>
        /// <param name="vertex">Добавляемая вершина.</param>
        public void AddVertex(T vertex)
        {
            if (!_adjacency.ContainsKey(vertex))
                _adjacency[vertex] = new List<Edge<T>>();
        }

        /// <summary>
        /// Добавляет ребро в граф. Автоматически создаёт отсутствующие вершины.
        /// </summary>
        /// <param name="from">Начальная вершина.</param>
        /// <param name="to">Конечная вершина.</param>
        /// <param name="weight">Вес ребра. По умолчанию <c>1.0</c>.</param>
        /// <remarks>
        /// Для неориентированного графа добавляются два направления.
        /// </remarks>
        public void AddEdge(T from, T to, double weight = 1.0)
        {
            AddVertex(from);
            AddVertex(to);

            var edge = new Edge<T>(from, to, weight);
            _adjacency[from].Add(edge);
            _edges.Add(edge);

            if (!IsDirected)
            {
                _adjacency[to].Add(new Edge<T>(to, from, weight));
            }
        }

        /// <summary>
        /// Удаляет ребро из графа.
        /// </summary>
        /// <param name="from">Начальная вершина.</param>
        /// <param name="to">Конечная вершина.</param>
        public void RemoveEdge(T from, T to)
        {
            if (!_adjacency.TryGetValue(from, out List<Edge<T>>? edgesList)) return;
            edgesList.RemoveAll(e => EqualityComparer<T>.Default.Equals(e.To, to));
            _edges.RemoveAll(e =>
                EqualityComparer<T>.Default.Equals(e.From, from) &&
                EqualityComparer<T>.Default.Equals(e.To, to));

            if (!IsDirected && _adjacency.TryGetValue(to, out List<Edge<T>>? outEdgesList))
                outEdgesList.RemoveAll(e => EqualityComparer<T>.Default.Equals(e.To, from));
        }

        /// <inheritdoc/>
        public IEnumerable<Edge<T>> Neighbors(T vertex)
            => _adjacency.TryGetValue(vertex, out var list)
                ? list
                : Enumerable.Empty<Edge<T>>();

        /// <summary>
        /// Создаёт транспонированный граф (все рёбра меняют направление).
        /// </summary>
        /// <returns>Новый граф с обратными рёбрами.</returns>
        /// <remarks>
        /// Используется в алгоритме Косараю для поиска SCC.
        /// </remarks>
        public Graph<T> Transpose()
        {
            var g = new Graph<T>(IsDirected);
            foreach (var v in Vertices) g.AddVertex(v);
            foreach (var e in _edges)
                g.AddEdge(e.To, e.From, e.Weight);
            return g;
        }
    }
}