namespace GraphToolkit.Structures;

/// <summary>
/// Дерево Фенвика (Binary Indexed Tree, BIT) для точечных обновлений
/// и запросов агрегата на префиксе/отрезке.
/// </summary>
/// <typeparam name="T">Тип хранимых значений.</typeparam>
/// <remarks>
/// <para>
/// Все операции выполняются за <b>O(log n)</b>, но константа меньше,
/// чем у <see cref="SegmentTree{T}"/>. Поддерживает точечные обновления
/// и запросы на префиксе; запрос на произвольном отрезке возможен
/// при наличии обратной операции (например, вычитание для суммы).
/// </para>
/// <para>
/// Для операций без обратного элемента (например, <c>min</c> или <c>max</c>)
/// используйте <see cref="SegmentTree{T}"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var bit = new FenwickTree&lt;long&gt;(
///     values: new long[] { 1, 2, 3, 4, 5 },
///     add: (a, b) =&gt; a + b,
///     identity: 0,
///     subtract: (a, b) =&gt; a - b);
/// 
/// Console.WriteLine(bit.PrefixAggregate(3));     // 1 + 2 + 3 = 6
/// Console.WriteLine(bit.RangeAggregate(1, 4));   // 2 + 3 + 4 = 9
/// 
/// bit.Add(2, 10);
/// Console.WriteLine(bit.RangeAggregate(1, 4));   // 2 + 13 + 4 = 19
/// </code>
/// </example>
public sealed class FenwickTree<T>
{
    private readonly T[] _tree;
    private readonly T _identity;
    private readonly Func<T, T, T> _add;      // операция для «накопления»
    private readonly Func<T, T, T> _subtract; // обратная операция (для RangeQuery)

    /// <summary>
    /// Создаёт пустое дерево Фенвика заданного размера.
    /// </summary>
    /// <param name="size">
    /// Количество элементов. Все элементы инициализируются значением
    /// <paramref name="identity"/>. Должно быть неотрицательным.
    /// </param>
    /// <param name="add">
    /// Ассоциативная бинарная операция для накопления агрегата,
    /// например <c>(a, b) =&gt; a + b</c> для суммы.
    /// Не должна быть <c>null</c>.
    /// </param>
    /// <param name="identity">
    /// Нейтральный элемент для <paramref name="add"/> — такой, что
    /// <c>add(identity, x) == add(x, identity) == x</c>.
    /// Для суммы — <c>0</c>, для произведения — <c>1</c>.
    /// </param>
    /// <param name="subtract">
    /// Обратная операция для <see cref="RangeAggregate"/> — такая, что
    /// <c>subtract(add(a, b), a) == b</c>. Например, вычитание для суммы.
    /// Может быть <c>null</c>, если запросы на произвольном отрезке
    /// не нужны (тогда будет работать только <see cref="PrefixAggregate"/>).
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Если <paramref name="size"/> меньше нуля.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Если <paramref name="add"/> равен <c>null</c>.
    /// </exception>
    /// <example>
    /// <code>
    /// // Сумма на 10 элементах без поддержки range query
    /// var bit = new FenwickTree&lt;long&gt;(
    ///     size: 10,
    ///     add: (a, b) =&gt; a + b,
    ///     identity: 0);
    /// 
    /// bit.Add(3, 42);
    /// Console.WriteLine(bit.PrefixAggregate(5));   // 42 (сумма первых 5)
    /// </code>
    /// </example>
    public FenwickTree(
        int size,
        Func<T, T, T> add,
        T identity,
        Func<T, T, T>? subtract = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(size);
        _tree = new T[size + 1];
        for (int i = 0; i <= size; i++) _tree[i] = identity;
        _identity = identity;
        _add = add ?? throw new ArgumentNullException(nameof(add));
        _subtract = subtract!;
    }

    /// <summary>
    /// Создаёт дерево Фенвика и инициализирует его значениями массива.
    /// </summary>
    /// <param name="values">
    /// Исходный массив значений. Элементы копируются в дерево
    /// через последовательные вызовы <see cref="Add"/>.
    /// Не должен быть <c>null</c>; может быть пустым.
    /// </param>
    /// <param name="add">
    /// Ассоциативная бинарная операция для накопления агрегата,
    /// например <c>(a, b) =&gt; a + b</c> для суммы.
    /// Не должна быть <c>null</c>.
    /// </param>
    /// <param name="identity">
    /// Нейтральный элемент для <paramref name="add"/>. Для суммы — <c>0</c>,
    /// для произведения — <c>1</c>.
    /// </param>
    /// <param name="subtract">
    /// Обратная операция для <see cref="RangeAggregate"/> (например,
    /// вычитание для суммы). Может быть <c>null</c>, если нужен
    /// только <see cref="PrefixAggregate"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Если <paramref name="values"/> или <paramref name="add"/> равны <c>null</c>.
    /// </exception>
    /// <remarks>
    /// Элементы массива добавляются в дерево по одному, поэтому
    /// создание занимает <b>O(n log n)</b>. Для <b>O(n)</b> можно было бы
    /// использовать специальную процедуру построения, но она требует
    /// знания обратной операции и здесь не реализована.
    /// </remarks>
    /// <example>
    /// <code>
    /// var bit = new FenwickTree&lt;long&gt;(
    ///     values: new long[] { 1, 2, 3, 4, 5 },
    ///     add: (a, b) =&gt; a + b,
    ///     identity: 0,
    ///     subtract: (a, b) =&gt; a - b);
    /// 
    /// Console.WriteLine(bit.RangeAggregate(1, 4));   // 2 + 3 + 4 = 9
    /// </code>
    /// </example>
    public FenwickTree(
        IReadOnlyList<T> values,
        Func<T, T, T> add,
        T identity,
        Func<T, T, T>? subtract = null)
        : this(values.Count, add, identity, subtract)
    {
        for (int i = 0; i < values.Count; i++)
            Add(i, values[i]);
    }

    /// <summary>
    /// Количество элементов в дереве.
    /// </summary>
    public int Count => _tree.Length - 1;

    /// <summary>
    /// Добавляет значение к элементу по указанному индексу.
    /// </summary>
    /// <param name="index">Индекс элемента (0-based).</param>
    /// <param name="value">
    /// Значение, которое нужно накопить с текущим через операцию <c>add</c>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Если <paramref name="index"/> вне диапазона <c>[0, Count)</c>.
    /// </exception>
    /// <remarks>
    /// Сложность: <b>O(log n)</b>.
    /// </remarks>
    public void Add(int index, T value)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        for (int i = index + 1; i <= Count; i += i & (-i))
            _tree[i] = _add(_tree[i], value);
    }

    /// <summary>
    /// Возвращает агрегат на префиксе <c>[0, index)</c>.
    /// </summary>
    /// <param name="index">
    /// Длина префикса (количество первых элементов).
    /// <c>0</c> даёт <c>identity</c>, <c>Count</c> — агрегат всего массива.
    /// </param>
    /// <returns>Агрегат на префиксе.</returns>
    /// <remarks>
    /// Сложность: <b>O(log n)</b>.
    /// </remarks>
    public T PrefixAggregate(int index)
    {
        T result = _identity;
        for (int i = index; i > 0; i -= i & (-i))
            result = _add(result, _tree[i]);
        return result;
    }

    /// <summary>
    /// Возвращает агрегат на отрезке <c>[left, right)</c>.
    /// </summary>
    /// <param name="left">Левая граница (включительно).</param>
    /// <param name="right">Правая граница (не включительно).</param>
    /// <returns>Агрегат на отрезке.</returns>
    /// <exception cref="InvalidOperationException">
    /// Если при создании дерева не была передана обратная операция
    /// <c>subtract</c>.
    /// </exception>
    /// <remarks>
    /// Сложность: <b>O(log n)</b>. Требует наличия обратной операции,
    /// так как вычисляется как <c>subtract(prefix(right), prefix(left))</c>.
    /// </remarks>
    public T RangeAggregate(int left, int right)
    {
        if (_subtract is null)
            throw new InvalidOperationException(
                "Для RangeAggregate нужно передать subtract в конструкторе.");
        return _subtract(PrefixAggregate(right), PrefixAggregate(left));
    }
}