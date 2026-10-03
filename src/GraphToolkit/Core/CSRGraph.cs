using System.Runtime.CompilerServices;

namespace GraphToolkit.Core;

/// <summary>
/// Граф в формате Compressed Sparse Row (CSR).
/// </summary>
/// <typeparam name="T">Тип данных вершины.</typeparam>
/// <remarks>
/// <para>
/// CSR — компактный формат хранения графа. Использует два массива:
/// </para>
/// <list type="bullet">
///   <item><c>rowPtr</c> — индексы начала рёбер каждой вершины в <c>colIdx</c>.</item>
///   <item><c>colIdx</c> — плоский массив соседей.</item>
///   <item><c>values</c> — веса рёбер (опционально).</item>
/// </list>
/// <para>
/// <b>Преимущества:</b> в 3–10× меньше памяти, чем <c>List&lt;Edge&gt;[]</c>,
/// cache-friendly обход соседей.
/// </para>
/// </remarks>
public sealed class CSRGraph<T> where T : notnull
{
    private readonly List<T> _vertices;
    private readonly Dictionary<T, int> _index;
    private readonly int[] _rowPtr;
    private readonly int[] _colIdx;
    private readonly double[] _weights;

    /// <summary>
    /// Количество вершин.
    /// </summary>
    public int VertexCount => _vertices.Count;

    /// <summary>
    /// Количество рёбер.
    /// </summary>
    public int EdgeCount => _colIdx.Length;

    /// <summary>
    /// Список вершин.
    /// </summary>
    public IReadOnlyList<T> Vertices => _vertices;

    /// <summary>
    /// Строит CSR-граф из обычного <see cref="IGraph{T}"/>.
    /// </summary>
    public CSRGraph(IGraph<T> graph)
    {
        _vertices = graph.Vertices.ToList();
        _index = _vertices
            .Select((v, i) => (v, i))
            .ToDictionary(x => x.v, x => x.i);

        int n = _vertices.Count;
        var adjacency = new List<(int To, double Weight)>[n];
        for (int i = 0; i < n; i++) adjacency[i] = new List<(int, double)>();

        foreach (var e in graph.Edges)
        {
            int u = _index[e.From], v = _index[e.To];
            adjacency[u].Add((v, e.Weight));
            if (!graph.IsDirected)
                adjacency[v].Add((u, e.Weight));
        }

        _rowPtr = new int[n + 1];
        int total = 0;
        for (int i = 0; i < n; i++)
        {
            _rowPtr[i] = total;
            total += adjacency[i].Count;
        }
        _rowPtr[n] = total;

        _colIdx = new int[total];
        _weights = new double[total];

        System.Threading.Tasks.Parallel.For(0, n, i =>
        {
            int offset = _rowPtr[i];
            for (int j = 0; j < adjacency[i].Count; j++)
            {
                _colIdx[offset + j] = adjacency[i][j].To;
                _weights[offset + j] = adjacency[i][j].Weight;
            }
        });
    }

    /// <summary>
    /// Возвращает индекс вершины.
    /// </summary>
    public int IndexOf(T vertex) => _index[vertex];

    /// <summary>
    /// Возвращает вершину по индексу.
    /// </summary>
    public T VertexAt(int index) => _vertices[index];

    /// <summary>
    /// Возвращает соседей вершины как span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<int> Neighbors(int vertexIndex)
    {
        int start = _rowPtr[vertexIndex];
        int end = _rowPtr[vertexIndex + 1];
        return _colIdx.AsSpan(start, end - start);
    }

    /// <summary>
    /// Возвращает веса рёбер соседей.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<double> NeighborWeights(int vertexIndex)
    {
        int start = _rowPtr[vertexIndex];
        int end = _rowPtr[vertexIndex + 1];
        return _weights.AsSpan(start, end - start);
    }

    /// <summary>
    /// Оценка памяти в байтах.
    /// </summary>
    public long ApproximateMemoryBytes()
    {
        return (long)_rowPtr.Length * sizeof(int)
             + (long)_colIdx.Length * sizeof(int)
             + (long)_weights.Length * sizeof(double);
    }
}
