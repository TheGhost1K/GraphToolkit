using GraphToolkit.Core;

namespace GraphToolkit.Trees;

/// <summary>
/// Центроидная декомпозиция дерева.
/// </summary>
/// <typeparam name="T">Тип данных вершины.</typeparam>
/// <remarks>
/// <para>
/// Разбивает дерево на уровни по центроидам: корень — центроид всего дерева,
/// дети — центроиды поддеревьев, полученных удалением корня, и т. д.
/// Высота дерева центроидов — O(log V).
/// </para>
/// <para>
/// <b>Построение:</b> O(V log V).
/// <b>Запрос пути через центроид:</b> O(log V) или O(log² V) в зависимости от агрегата.
/// </para>
/// <para>
/// <b>Применения:</b> запросы расстояний между парами вершин на дереве,
/// подсчёт количества путей с заданным свойством, offline-запросы
/// «найти вершину на пути».
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var tree = new GraphBuilder&lt;int&gt;()
///     .AddEdge(1, 2, 1)
///     .AddEdge(2, 3, 1)
///     .AddEdge(3, 4, 1)
///     .Build();
/// 
/// var cd = new CentroidDecomposition&lt;int&gt;(tree);
/// int distance = cd.Distance(1, 4);   // 3
/// </code>
/// </example>
public sealed class CentroidDecomposition<T> where T : notnull
{
    private readonly IGraph<T> _tree;
    private readonly Dictionary<T, T?> _centroidParent = new();
    private readonly Dictionary<T, int> _subtreeSize = new();
    private readonly Dictionary<T, int> _depth = new();
    private readonly Dictionary<T, Dictionary<T, double>> _ancestorDistances = new();
    private readonly HashSet<T> _removed = new();
    private readonly T _root;

    /// <summary>
    /// Строит центроидную декомпозицию для указанного дерева.
    /// </summary>
    /// <param name="tree">
    /// Неориентированное дерево (связный граф без циклов).
    /// </param>
    /// <exception cref="ArgumentException">
    /// Если граф не является деревом (содержит цикл или не связный).
    /// </exception>
    public CentroidDecomposition(IGraph<T> tree)
    {
        _tree = tree ?? throw new ArgumentNullException(nameof(tree));

        if (tree.VertexCount == 0)
            throw new ArgumentException("Дерево не может быть пустым.", nameof(tree));

        if (tree.VertexCount > 1 && tree.EdgeCount != tree.VertexCount - 1)
            throw new ArgumentException(
                "Граф не является деревом: число рёбер не равно V - 1.", nameof(tree));

        _root = BuildRecursive(tree.Vertices.First(), default, 0);
    }

    /// <summary>
    /// Корень дерева центроидов.
    /// </summary>
    public T Root => _root;

    /// <summary>
    /// Возвращает центроидного родителя вершины.
    /// </summary>
    /// <param name="v">Вершина.</param>
    /// <returns>Родитель в дереве центроидов или <c>default</c>, если v — корень.</returns>
    public T? Parent(T v) => _centroidParent.GetValueOrDefault(v);

    /// <summary>
    /// Глубина вершины в дереве центроидов.
    /// </summary>
    /// <param name="v">Вершина.</param>
    public int Depth(T v) => _depth.GetValueOrDefault(v);

    /// <summary>
    /// Расстояние между двумя вершинами в исходном дереве.
    /// </summary>
    /// <param name="u">Первая вершина.</param>
    /// <param name="v">Вторая вершина.</param>
    /// <returns>Сумма весов рёбер на пути между u и v.</returns>
    /// <remarks>
    /// Работает за O(log V), если расстояния от каждой вершины до её
    /// центроидных предков предпосчитаны. В данной реализации — O(log V · deg).
    /// </remarks>
    public double Distance(T u, T v)
    {
        if (EqualityComparer<T>.Default.Equals(u, v)) return 0;

        var ancestorsU = _ancestorDistances[u];
        var ancestorsV = _ancestorDistances[v];

        var current = (T?)u;
        while (current is not null)
        {
            if (ancestorsV.ContainsKey(current))
                return ancestorsU[current] + ancestorsV[current];
            current = _centroidParent.GetValueOrDefault(current);
        }

        return double.PositiveInfinity;
    }

    /// <summary>
    /// Возвращает всех центроидных предков вершины с расстояниями до них.
    /// </summary>
    /// <param name="v">Вершина.</param>
    /// <returns>
    /// Словарь: центроид → расстояние от v до этого центроида в исходном дереве.
    /// </returns>
    public Dictionary<T, double> GetAncestorDistances(T v)
    {
        return _ancestorDistances.TryGetValue(v, out var map)
            ? new Dictionary<T, double>(map)
            : new Dictionary<T, double>();
    }

    /// <summary>
    /// Обходит все вершины в порядке, определяемом декомпозицией
    /// (корень → дети → внуки).
    /// </summary>
    public IEnumerable<T> Traverse()
    {
        var queue = new Queue<T>();
        queue.Enqueue(_root);
        while (queue.Count > 0)
        {
            var v = queue.Dequeue();
            yield return v;
            foreach (var child in Children(v))
                queue.Enqueue(child);
        }
    }

    /// <summary>
    /// Возвращает центроидных детей вершины.
    /// </summary>
    public IEnumerable<T> Children(T v)
    {
        return _centroidParent
            .Where(kv => kv.Value is not null &&
                         EqualityComparer<T>.Default.Equals(kv.Value, v))
            .Select(kv => kv.Key);
    }

    // ---------- Внутренние методы ----------

    private T BuildRecursive(T start, T? parent, int depth)
    {
        int size = ComputeSizes(start, default);
        T centroid = FindCentroid(start, default, size);

        _centroidParent[centroid] = parent;
        _depth[centroid] = depth;
        _removed.Add(centroid);

        // Предпосчитываем расстояния от centroid до всех вершин его компоненты
        ComputeDistancesFromCentroid(centroid);

        foreach (var edge in _tree.Neighbors(centroid))
        {
            if (_removed.Contains(edge.To)) continue;
            BuildRecursive(edge.To, centroid, depth + 1);
        }

        return centroid;
    }

    private void ComputeDistancesFromCentroid(T centroid)
    {
        var dist = new Dictionary<T, double> { [centroid] = 0 };
        var queue = new Queue<T>();
        queue.Enqueue(centroid);

        while (queue.Count > 0)
        {
            var u = queue.Dequeue();
            foreach (var edge in _tree.Neighbors(u))
            {
                if (_removed.Contains(edge.To) && !EqualityComparer<T>.Default.Equals(edge.To, centroid))
                    continue;
                if (dist.ContainsKey(edge.To)) continue;
                dist[edge.To] = dist[u] + edge.Weight;
                queue.Enqueue(edge.To);
            }
        }

        foreach (var (v, d) in dist)
        {
            if (!_ancestorDistances.ContainsKey(v))
                _ancestorDistances[v] = new Dictionary<T, double>();
            _ancestorDistances[v][centroid] = d;
        }
    }

    private int ComputeSizes(T u, T? parent)
    {
        int size = 1;
        foreach (var edge in _tree.Neighbors(u))
        {
            if (EqualityComparer<T>.Default.Equals(edge.To, parent)) continue;
            if (_removed.Contains(edge.To)) continue;
            size += ComputeSizes(edge.To, u);
        }
        _subtreeSize[u] = size;
        return size;
    }

    private T FindCentroid(T u, T? parent, int totalSize)
    {
        foreach (var edge in _tree.Neighbors(u))
        {
            if (EqualityComparer<T>.Default.Equals(edge.To, parent)) continue;
            if (_removed.Contains(edge.To)) continue;
            if (_subtreeSize[edge.To] > totalSize / 2)
                return FindCentroid(edge.To, u, totalSize);
        }
        return u;
    }
}