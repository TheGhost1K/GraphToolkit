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

        /// <summary>
        /// Создаёт полную копию графа: те же вершины, рёбра и веса.
        /// </summary>
        /// <returns>Новый независимый граф с теми же данными.</returns>
        /// <remarks>
        /// Возвращаемый граф не разделяет внутренние структуры с исходным —
        /// изменение копии не влияет на оригинал и наоборот.
        /// </remarks>
        /// <example>
        /// <code>
        /// var copy = graph.Clone();
        /// copy.AddEdge("X", "Y");           // оригинал не меняется
        /// Console.WriteLine(graph.VertexCount);  // без X, Y
        /// </code>
        /// </example>
        public Graph<T> Clone()
        {
            var g = new Graph<T>(IsDirected);
            foreach (var v in Vertices) g.AddVertex(v);
            foreach (var e in _edges)
                g.AddEdge(e.From, e.To, e.Weight);
            return g;
        }

        /// <summary>
        /// Создаёт неориентированную копию графа.
        /// </summary>
        /// <returns>
        /// Неориентированный граф со всеми рёбрами исходного.
        /// Каждое ориентированное ребро превращается в неориентированное.
        /// </returns>
        /// <remarks>
        /// Если исходный граф уже неориентированный — эквивалентно <see cref="Clone"/>.
        /// Если в исходном графе есть рёбра <c>u → v</c> и <c>v → u</c> с разными весами,
        /// в результат добавится ребро с весом первого добавленного.
        /// </remarks>
        /// <example>
        /// <code>
        /// var directed = new Graph&lt;string&gt;(isDirected: true);
        /// directed.AddEdge("A", "B", 1);
        /// directed.AddEdge("B", "A", 5);
        /// 
        /// var undirected = directed.ToUndirected();
        /// // В undirected будет только одно ребро A—B (вес 1)
        /// </code>
        /// </example>
        public Graph<T> ToUndirected()
        {
            if (!IsDirected) return Clone();

            var g = new Graph<T>(isDirected: false);
            foreach (var v in Vertices) g.AddVertex(v);

            // Хеш-сет для дедупликации пар (u, v) — чтобы не добавить
            // A—B дважды, если были A→B и B→A
            var seen = new HashSet<(T, T)>();
            foreach (var e in _edges)
            {
                var pair = NormalizePair(e.From, e.To);
                if (seen.Add(pair))
                    g.AddEdge(pair.Item1, pair.Item2, e.Weight);
            }
            return g;
        }

        /// <summary>
        /// Создаёт ориентированную копию графа.
        /// </summary>
        /// <returns>
        /// Ориентированный граф со всеми рёбрами исходного.
        /// Для неориентированного графа каждое ребро <c>u — v</c> превращается
        /// в два ориентированных: <c>u → v</c> и <c>v → u</c>.
        /// </returns>
        /// <remarks>
        /// Если исходный граф уже ориентированный — эквивалентно <see cref="Clone"/>.
        /// </remarks>
        /// <example>
        /// <code>
        /// var undirected = new Graph&lt;string&gt;();
        /// undirected.AddEdge("A", "B", 3);
        /// 
        /// var directed = undirected.ToDirected();
        /// // В directed будут рёбра A→B и B→A с весом 3
        /// </code>
        /// </example>
        public Graph<T> ToDirected()
        {
            if (IsDirected) return Clone();

            var g = new Graph<T>(isDirected: true);
            foreach (var v in Vertices) g.AddVertex(v);

            foreach (var e in _edges)
            {
                g.AddEdge(e.From, e.To, e.Weight);
                g.AddEdge(e.To, e.From, e.Weight);
            }
            return g;
        }

        /// <summary>
        /// Нормализует пару вершин так, чтобы меньшая по хешу была первой.
        /// Нужно для дедупликации неориентированных рёбер.
        /// </summary>
        private static (T, T) NormalizePair(T a, T b)
        {
            int ha = a.GetHashCode();
            int hb = b.GetHashCode();
            if (ha != hb) return ha < hb ? (a, b) : (b, a);
            // При равных хешах — сравнение через default comparer
            return Comparer<T>.Default.Compare(a, b) <= 0 ? (a, b) : (b, a);
        }
    }
}