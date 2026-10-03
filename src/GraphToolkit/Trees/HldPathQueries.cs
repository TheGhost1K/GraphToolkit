using System;
using System.Collections.Generic;
using GraphToolkit.Core;
using GraphToolkit.Structures;

namespace GraphToolkit.Trees;

/// <summary>
/// Обёртка над <see cref="HeavyLightDecomposition{T}"/> с деревом отрезков
/// для запросов на путях дерева.
/// </summary>
/// <typeparam name="T">Тип данных вершины.</typeparam>
/// <typeparam name="TValue">Тип значений в вершинах.</typeparam>
/// <remarks>
/// <para>
/// Позволяет за O(log² V) делать запросы <c>Query(u, v)</c> и
/// <c>Update(u, value)</c> на путях в дереве. Для этого нужно задать:
/// <list type="bullet">
///   <item>начальное значение каждой вершины;</item>
///   <item>операцию агрегата (например, сумма, min, max);</item>
///   <item>нейтральный элемент.</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var tree = new GraphBuilder&lt;int&gt;()
///     .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 4)
///     .Build();
/// 
/// var weights = new Dictionary&lt;int, long&gt;
/// {
///     [1] = 10, [2] = 20, [3] = 30, [4] = 40
/// };
/// 
/// var hld = new HldPathQueries&lt;int, long&gt;(
///     tree, root: 1,
///     valueOf: v =&gt; weights[v],
///     combine: (a, b) =&gt; a + b,
///     identity: 0);
/// 
/// Console.WriteLine(hld.Query(1, 4));   // 100
/// hld.Update(2, 200);
/// Console.WriteLine(hld.Query(1, 4));   // 280
/// </code>
/// </example>
public sealed class HldPathQueries<T, TValue> where T : notnull
{
    private readonly HeavyLightDecomposition<T> _hld;
    private readonly SegmentTree<TValue> _seg;
    private readonly Func<TValue, TValue, TValue> _combine;

    /// <summary>
    /// Создаёт HLD с деревом отрезков для запросов на путях.
    /// </summary>
    /// <param name="tree">Дерево.</param>
    /// <param name="root">Корень.</param>
    /// <param name="valueOf">Функция начального значения вершины.</param>
    /// <param name="combine">Ассоциативная операция.</param>
    /// <param name="identity">Нейтральный элемент для <paramref name="combine"/>.</param>
    public HldPathQueries(
        IGraph<T> tree,
        T root,
        Func<T, TValue> valueOf,
        Func<TValue, TValue, TValue> combine,
        TValue identity)
    {
        _combine = combine ?? throw new ArgumentNullException(nameof(combine));
        _hld = new HeavyLightDecomposition<T>(tree, root);

        var vertices = tree.Vertices.ToList();
        int n = vertices.Count;

        var linear = new TValue[n];
        foreach (var v in vertices)
            linear[_hld.Position(v)] = valueOf(v);

        _seg = new SegmentTree<TValue>(linear, combine, identity);
    }

    /// <summary>
    /// Запрос агрегата на пути между u и v.
    /// </summary>
    /// <param name="u">Первая вершина.</param>
    /// <param name="v">Вторая вершина.</param>
    /// <returns>Агрегат по всем вершинам на пути (включая u и v).</returns>
    public TValue Query(T u, T v)
    {
        var segments = new List<(int Left, int Right)>();

        while (!EqualityComparer<T>.Default.Equals(_hld.Head(u), _hld.Head(v)))
        {
            if (_hld.Position(_hld.Head(u)) < _hld.Position(_hld.Head(v)))
                (u, v) = (v, u);
            segments.Add((_hld.Position(_hld.Head(u)), _hld.Position(u)));
            u = _hld.Parent(_hld.Head(u))!;
        }

        int pu = _hld.Position(u), pv = _hld.Position(v);
        if (pu > pv) (pu, pv) = (pv, pu);
        segments.Add((pu, pv));

        TValue result = _seg.Query(segments[0].Left, segments[0].Right + 1);
        for (int i = 1; i < segments.Count; i++)
        {
            var partial = _seg.Query(segments[i].Left, segments[i].Right + 1);
            result = _combine(result, partial);
        }
        return result;
    }

    /// <summary>
    /// Точечное обновление значения в вершине.
    /// </summary>
    public void Update(T vertex, TValue value)
    {
        _seg.Update(_hld.Position(vertex), value);
    }

    /// <summary>
    /// Возвращает вершину на позиции (для отладки).
    /// </summary>
    public T VertexAt(int pos) => _hld.VertexAt(pos);

    /// <summary>
    /// Возвращает позицию вершины в линейном массиве.
    /// </summary>
    public int PositionOf(T vertex) => _hld.Position(vertex);
}