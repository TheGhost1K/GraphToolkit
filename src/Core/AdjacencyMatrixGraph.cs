namespace GraphToolkit.Core;

/// <summary>
/// Реализация <see cref="IGraph{T}"/> на основе матрицы смежности.
/// </summary>
/// <typeparam name="T">Тип данных вершины.</typeparam>
/// <remarks>
/// <para>
/// Подходит для <b>плотных</b> графов, где число рёбер близко к V².
/// Проверка наличия ребра и получение веса — O(1), в отличие от
/// <see cref="Graph{T}"/> (список смежности), где эти операции требуют O(deg(v)).
/// </para>
/// <para>
/// <b>Память:</b> O(V²) — независимо от числа рёбер. Для больших разреженных графов
/// используйте <see cref="Graph{T}"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var g = new AdjacencyMatrixGraph&lt;string&gt;(isDirected: false);
/// g.AddEdge("A", "B", 4);
/// g.AddEdge("B", "C", 2);
/// 
/// // Все алгоритмы GraphToolkit работают без изменений:
/// var path = Dijkstra.FindPath(g, "A", "C");
/// </code>
/// </example>
public sealed class AdjacencyMatrixGraph<T> : IGraph<T> where T : notnull
{
    private readonly List<T> _vertices = new();
    private readonly Dictionary<T, int> _index = new();
    private double[,] _weights;
    private int _capacity;

    /// <inheritdoc/>
    public bool IsDirected { get; }

    /// <inheritdoc/>
    public IEnumerable<T> Vertices => _vertices;

    /// <inheritdoc/>
    public int VertexCount => _vertices.Count;

    /// <inheritdoc/>
    public int EdgeCount { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyList<Edge<T>> Edges
    {
        get
        {
            var result = new List<Edge<T>>(EdgeCount);
            int n = _vertices.Count;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    if (double.IsPositiveInfinity(_weights[i, j])) continue;
                    if (!IsDirected && j <= i) continue;
                    result.Add(new Edge<T>(_vertices[i], _vertices[j], _weights[i, j]));
                }
            return result;
        }
    }

    /// <summary>
    /// Инициализирует пустую матрицу смежности.
    /// </summary>
    /// <param name="isDirected">Признак ориентированности графа.</param>
    /// <param name="initialCapacity">
    /// Начальная ёмкость (число вершин). Матрица будет расти автоматически.
    /// </param>
    public AdjacencyMatrixGraph(bool isDirected = false, int initialCapacity = 16)
    {
        IsDirected = isDirected;
        _capacity = Math.Max(4, initialCapacity);
        _weights = CreateMatrix(_capacity);
    }

    private static double[,] CreateMatrix(int size)
    {
        var m = new double[size, size];
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                m[i, j] = double.PositiveInfinity;
        return m;
    }

    /// <summary>
    /// Добавляет вершину. Если она уже есть — ничего не делает.
    /// </summary>
    /// <param name="vertex">Вершина.</param>
    public void AddVertex(T vertex)
    {
        if (_index.ContainsKey(vertex)) return;
        if (_vertices.Count >= _capacity) Grow();

        _index[vertex] = _vertices.Count;
        _vertices.Add(vertex);
    }

    /// <summary>
    /// Добавляет ребро. Автоматически создаёт вершины.
    /// </summary>
    /// <param name="from">Источник.</param>
    /// <param name="to">Приёмник.</param>
    /// <param name="weight">Вес ребра.</param>
    public void AddEdge(T from, T to, double weight = 1.0)
    {
        AddVertex(from);
        AddVertex(to);

        int u = _index[from], v = _index[to];
        if (double.IsPositiveInfinity(_weights[u, v])) EdgeCount++;
        _weights[u, v] = weight;

        if (!IsDirected && u != v)
            _weights[v, u] = weight;
    }

    /// <summary>
    /// Удаляет ребро.
    /// </summary>
    /// <param name="from">Источник.</param>
    /// <param name="to">Приёмник.</param>
    public void RemoveEdge(T from, T to)
    {
        if (!_index.TryGetValue(from, out var u)) return;
        if (!_index.TryGetValue(to, out var v)) return;

        if (!double.IsPositiveInfinity(_weights[u, v]))
        {
            _weights[u, v] = double.PositiveInfinity;
            EdgeCount--;
        }
        if (!IsDirected && u != v)
            _weights[v, u] = double.PositiveInfinity;
    }

    /// <inheritdoc/>
    public IEnumerable<Edge<T>> Neighbors(T vertex)
    {
        if (!_index.TryGetValue(vertex, out int u))
            yield break;

        int n = _vertices.Count;
        for (int v = 0; v < n; v++)
        {
            if (double.IsPositiveInfinity(_weights[u, v])) continue;
            yield return new Edge<T>(_vertices[u], _vertices[v], _weights[u, v]);
        }
    }

    /// <summary>
    /// Возвращает вес ребра между двумя вершинами за O(1).
    /// </summary>
    /// <param name="from">Источник.</param>
    /// <param name="to">Приёмник.</param>
    /// <returns>
    /// Вес ребра или <see cref="double.PositiveInfinity"/>, если ребра нет.
    /// </returns>
    public double GetWeight(T from, T to)
    {
        if (!_index.TryGetValue(from, out var u)) return double.PositiveInfinity;
        if (!_index.TryGetValue(to, out var v)) return double.PositiveInfinity;
        return _weights[u, v];
    }

    /// <summary>
    /// Проверяет наличие ребра за O(1).
    /// </summary>
    /// <param name="from">Источник.</param>
    /// <param name="to">Приёмник.</param>
    /// <returns><c>true</c>, если ребро существует.</returns>
    public bool HasEdge(T from, T to)
    {
        if (!_index.TryGetValue(from, out var u)) return false;
        if (!_index.TryGetValue(to, out var v)) return false;
        return !double.IsPositiveInfinity(_weights[u, v]);
    }

    /// <summary>
    /// Проверяет наличие вершины за O(1).
    /// </summary>
    /// <param name="vertex">Вершина.</param>
    public bool HasVertex(T vertex) => _index.ContainsKey(vertex);

    /// <summary>
    /// Удваивает ёмкость матрицы, сохраняя существующие данные.
    /// </summary>
    private void Grow()
    {
        int newCap = _capacity * 2;
        var newMatrix = CreateMatrix(newCap);

        for (int i = 0; i < _capacity; i++)
            for (int j = 0; j < _capacity; j++)
                newMatrix[i, j] = _weights[i, j];

        _weights = newMatrix;
        _capacity = newCap;
    }
}