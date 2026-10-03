using System;
using System.Collections.Generic;

namespace GraphToolkit.Structures;

/// <summary>
/// Дерево отрезков (Segment Tree) с ленивым распространением.
/// </summary>
/// <typeparam name="T">Тип хранимых значений.</typeparam>
/// <remarks>
/// <para>
/// Поддерживает операции на отрезках массива:
/// <list type="bullet">
///   <item><b>Query(l, r)</b> — агрегат на отрезке [l, r) за O(log n).</item>
///   <item><b>Update(i, value)</b> — точечное обновление за O(log n).</item>
///   <item><b>RangeUpdate(l, r, value)</b> — обновление отрезка за O(log n)
///         с ленивым распространением.</item>
/// </list>
/// </para>
/// <para>
/// Агрегатная операция задаётся функцией <c>combine</c>, нейтральный
/// элемент — <c>identity</c> (например, 0 для суммы, +∞ для минимума).
/// Для lazy-обновлений нужна функция <c>apply</c> — как применить значение
/// к элементу, и <c>composeUpdates</c> — как объединить два отложенных
/// обновления.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Дерево суммы
/// var st = new SegmentTree&lt;long&gt;(
///     values: new long[] { 1, 2, 3, 4, 5 },
///     combine: (a, b) =&gt; a + b,
///     identity: 0);
/// 
/// Console.WriteLine(st.Query(1, 4));   // 2 + 3 + 4 = 9
/// st.Update(2, 10);
/// Console.WriteLine(st.Query(1, 4));   // 2 + 10 + 4 = 16
/// </code>
/// </example>
public sealed class SegmentTree<T>
{
    private readonly int _n;
    private readonly T[] _tree;
    private readonly T[] _lazy;
    private readonly bool[] _hasLazy;
    private readonly T _identity;
    private readonly Func<T, T, T> _combine;
    private readonly Func<T, T, T>? _applyLazy;      // как применить lazy к узлу
    private readonly Func<T, T, T>? _composeLazy;    // как объединить два lazy

    /// <summary>
    /// Создаёт дерево отрезков на массиве.
    /// </summary>
    /// <param name="values">Исходный массив значений.</param>
    /// <param name="combine">Ассоциативная бинарная операция (например, сумма, min, max, gcd).</param>
    /// <param name="identity">Нейтральный элемент для <paramref name="combine"/>.</param>
    /// <param name="applyLazy">
    /// Функция применения отложенного обновления к агрегату узла.
    /// Обязательна для <see cref="RangeUpdate(int, int, T)"/>.
    /// </param>
    /// <param name="composeLazy">
    /// Функция объединения двух отложенных обновлений.
    /// Обязательна для <see cref="RangeUpdate(int, int, T)"/>.
    /// </param>
    public SegmentTree(
        IReadOnlyList<T> values,
        Func<T, T, T> combine,
        T identity,
        Func<T, T, T>? applyLazy = null,
        Func<T, T, T>? composeLazy = null)
    {
        _n = values.Count;
        _tree = new T[4 * Math.Max(1, _n)];
        _lazy = new T[4 * Math.Max(1, _n)];
        _hasLazy = new bool[4 * Math.Max(1, _n)];
        _identity = identity;
        _combine = combine ?? throw new ArgumentNullException(nameof(combine));
        _applyLazy = applyLazy;
        _composeLazy = composeLazy;

        if (_n > 0)
            Build(1, 0, _n, values);
    }

    /// <summary>
    /// Длина массива, на котором построено дерево.
    /// </summary>
    public int Count => _n;

    /// <summary>
    /// Запрос агрегата на отрезке [<paramref name="left"/>, <paramref name="right"/>).
    /// </summary>
    /// <param name="left">Левая граница (включительно).</param>
    /// <param name="right">Правая граница (не включительно).</param>
    /// <returns>Агрегат на отрезке.</returns>
    public T Query(int left, int right)
    {
        if (left < 0 || right > _n || left >= right)
            return _identity;
        return Query(1, 0, _n, left, right);
    }

    /// <summary>
    /// Запрос агрегата на всём массиве.
    /// </summary>
    public T QueryAll() => _n == 0 ? _identity : _tree[1];

    /// <summary>
    /// Точечное обновление: заменяет значение в позиции.
    /// </summary>
    /// <param name="index">Индекс (0-based).</param>
    /// <param name="value">Новое значение.</param>
    public void Update(int index, T value)
    {
        if (index < 0 || index >= _n)
            throw new ArgumentOutOfRangeException(nameof(index));
        Update(1, 0, _n, index, value);
    }

    /// <summary>
    /// Обновление отрезка: применяет <paramref name="value"/> через
    /// <c>applyLazy</c> ко всем элементам [<paramref name="left"/>, <paramref name="right"/>).
    /// </summary>
    /// <param name="left">Левая граница.</param>
    /// <param name="right">Правая граница.</param>
    /// <param name="value">Значение обновления.</param>
    /// <exception cref="InvalidOperationException">
    /// Если при создании не были переданы <c>applyLazy</c> и <c>composeLazy</c>.
    /// </exception>
    public void RangeUpdate(int left, int right, T value)
    {
        if (_applyLazy is null || _composeLazy is null)
            throw new InvalidOperationException(
                "Для RangeUpdate нужно передать applyLazy и composeLazy в конструкторе.");

        if (left < 0 || right > _n || left >= right)
            return;
        RangeUpdate(1, 0, _n, left, right, value);
    }

    /// <summary>
    /// Возвращает значение в конкретной позиции.
    /// </summary>
    public T GetAt(int index)
    {
        if (index < 0 || index >= _n)
            throw new ArgumentOutOfRangeException(nameof(index));
        return Query(index, index + 1);
    }

    // ---------- Внутренние методы ----------

    private void Build(int node, int l, int r, IReadOnlyList<T> values)
    {
        if (r - l == 1)
        {
            _tree[node] = values[l];
            return;
        }
        int mid = (l + r) / 2;
        Build(2 * node, l, mid, values);
        Build(2 * node + 1, mid, r, values);
        _tree[node] = _combine(_tree[2 * node], _tree[2 * node + 1]);
    }

    private void PushDown(int node)
    {
        if (!_hasLazy[node]) return;
        if (_applyLazy is null || _composeLazy is null) return;

        // Применяем ленивое обновление к детям
        for (int child = 2 * node; child <= 2 * node + 1; child++)
        {
            _tree[child] = _applyLazy(_tree[child], _lazy[node]);
            if (_hasLazy[child])
                _lazy[child] = _composeLazy(_lazy[child], _lazy[node]);
            else
            {
                _lazy[child] = _lazy[node];
                _hasLazy[child] = true;
            }
        }
        _hasLazy[node] = false;
    }

    private T Query(int node, int l, int r, int ql, int qr)
    {
        if (qr <= l || r <= ql) return _identity;
        if (ql <= l && r <= qr)
        {
            return _tree[node];
        }
        PushDown(node);
        int mid = (l + r) / 2;
        var left = Query(2 * node, l, mid, ql, qr);
        var right = Query(2 * node + 1, mid, r, ql, qr);
        return _combine(left, right);
    }

    private void Update(int node, int l, int r, int index, T value)
    {
        if (r - l == 1)
        {
            _tree[node] = value;
            _hasLazy[node] = false;
            return;
        }
        PushDown(node);
        int mid = (l + r) / 2;
        if (index < mid)
            Update(2 * node, l, mid, index, value);
        else
            Update(2 * node + 1, mid, r, index, value);
        _tree[node] = _combine(_tree[2 * node], _tree[2 * node + 1]);
    }

    private void RangeUpdate(int node, int l, int r, int ql, int qr, T value)
    {
        if (qr <= l || r <= ql) return;

        if (ql <= l && r <= qr)
        {
            _tree[node] = _applyLazy!(_tree[node], value);
            if (_hasLazy[node])
                _lazy[node] = _composeLazy!(_lazy[node], value);
            else
            {
                _lazy[node] = value;
                _hasLazy[node] = true;
            }
            return;
        }

        PushDown(node);
        int mid = (l + r) / 2;
        RangeUpdate(2 * node, l, mid, ql, qr, value);
        RangeUpdate(2 * node + 1, mid, r, ql, qr, value);
        _tree[node] = _combine(_tree[2 * node], _tree[2 * node + 1]);
    }
}