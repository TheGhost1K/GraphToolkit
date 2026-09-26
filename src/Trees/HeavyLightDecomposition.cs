using GraphToolkit.Core;

namespace GraphToolkit.Trees;

/// <summary>
/// Heavy-Light Decomposition (HLD) дерева.
/// </summary>
/// <typeparam name="T">Тип данных вершины.</typeparam>
/// <remarks>
/// <para>
/// Разбивает дерево на «тяжёлые пути» так, что любой путь между двумя
/// вершинами пересекает O(log V) путей. Позволяет применять запросы
/// на путях через одномерные структуры (отрезки массива).
/// </para>
/// <para>
/// <b>Построение:</b> O(V).
/// <b>Запрос/обновление на пути:</b> O(log² V) (при использовании
/// дерева отрезков или Fenwick для отрезков).
/// </para>
/// <para>
/// <b>Реализация:</b> все обходы итеративные (через явный стек) —
/// это исключает переполнение стека на деревьях-цепочках большой длины.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var tree = new GraphBuilder&lt;int&gt;(isDirected: false)
///     .AddEdge(1, 2)
///     .AddEdge(1, 3)
///     .AddEdge(2, 4)
///     .AddEdge(2, 5)
///     .Build();
/// 
/// var hld = new HeavyLightDecomposition&lt;int&gt;(tree, root: 1);
/// var lca = hld.Lca(4, 5);                    // 2
/// var path = hld.PathVertices(4, 3);          // [4, 2, 1, 3]
/// var segments = hld.PathSegments(4, 3);      // позиции для дерева отрезков
/// </code>
/// </example>
public sealed class HeavyLightDecomposition<T> where T : notnull
{
    private readonly IGraph<T> _tree;
    private readonly T _root;

    /// <summary>
    /// Родители вершин. Отсутствие ключа означает, что вершина —
    /// корень дерева (родителя нет).
    /// </summary>
    /// <remarks>
    /// В отличие от «классического» подхода с <c>T?</c>, здесь используется
    /// <c>Dictionary&lt;T, T&gt;</c> без nullable-значения. Отсутствие ключа
    /// — единственный корректный способ выразить «родителя нет» для
    /// generic-типа <typeparamref name="T"/> под ограничением
    /// <c>where T : notnull</c>.
    /// </remarks>
    private readonly Dictionary<T, T> _parent = new();

    /// <summary>
    /// Глубины вершин от корня (корень = 0).
    /// </summary>
    private readonly Dictionary<T, int> _depth = new();

    /// <summary>
    /// Размеры поддеревьев (число вершин в поддереве каждой вершины).
    /// </summary>
    private readonly Dictionary<T, int> _subtreeSize = new();

    /// <summary>
    /// «Тяжёлые» дети вершин — те, у которых поддерево максимального размера.
    /// Отсутствие ключа означает, что у вершины нет детей.
    /// </summary>
    private readonly Dictionary<T, T> _heavyChild = new();

    /// <summary>
    /// Глава тяжёлого пути, которому принадлежит вершина (самая верхняя
    /// вершина этого пути).
    /// </summary>
    private readonly Dictionary<T, T> _head = new();

    /// <summary>
    /// Позиция вершины в линейном массиве (0..V-1).
    /// Вершины одного тяжёлого пути занимают непрерывный отрезок позиций.
    /// </summary>
    private readonly Dictionary<T, int> _position = new();

    /// <summary>
    /// Обратное отображение: по позиции — вершина.
    /// </summary>
    private readonly List<T> _verticesByPosition = new();

    /// <summary>
    /// Строит Heavy-Light Decomposition для дерева с заданным корнем.
    /// </summary>
    /// <param name="tree">Дерево — неориентированный граф без циклов.</param>
    /// <param name="root">Корень дерева.</param>
    /// <exception cref="ArgumentNullException">
    /// Если <paramref name="tree"/> равен <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Если <paramref name="root"/> отсутствует в графе или граф
    /// не является деревом (содержит цикл или не связный).
    /// </exception>
    public HeavyLightDecomposition(IGraph<T> tree, T root)
    {
        _tree = tree ?? throw new ArgumentNullException(nameof(tree));

        if (!tree.Vertices.Contains(root))
            throw new ArgumentException(
                $"Вершина {root} отсутствует в графе.", nameof(root));

        // Проверка: неориентированное дерево имеет E = V - 1 рёбер
        int v = tree.VertexCount;
        int e = tree.EdgeCount;
        if (v > 1 && e != v - 1)
            throw new ArgumentException(
                $"Граф не является деревом: V = {v}, E = {e}. " +
                $"Ожидалось E = V - 1 = {v - 1}.",
                nameof(tree));

        _root = root;

        // Шаг 1: обход в глубину — вычисляем parent, depth, subtreeSize, heavyChild
        ComputeSizesAndHeavyChildren(root);

        // Шаг 2: разбиение на тяжёлые пути и заполнение позиций
        Decompose(root, root);
    }

    /// <summary>
    /// Позиция вершины в линейном массиве (0..V-1).
    /// </summary>
    /// <param name="v">Вершина.</param>
    /// <returns>Индекс в линейном массиве, пригодный для запроса к дереву отрезков.</returns>
    public int Position(T v) => _position[v];

    /// <summary>
    /// Вершина, стоящая на данной позиции.
    /// </summary>
    /// <param name="pos">Позиция (0..V-1).</param>
    /// <returns>Вершина.</returns>
    public T VertexAt(int pos) => _verticesByPosition[pos];

    /// <summary>
    /// Глава тяжёлого пути, которому принадлежит вершина.
    /// </summary>
    /// <param name="v">Вершина.</param>
    /// <returns>Самая верхняя вершина тяжёлого пути.</returns>
    public T Head(T v) => _head[v];

    /// <summary>
    /// Глубина вершины от корня (корень = 0).
    /// </summary>
    /// <param name="v">Вершина.</param>
    public int Depth(T v) => _depth[v];

    /// <summary>
    /// Корень дерева, переданный в конструктор.
    /// </summary>
    public T Root => _root;

    /// <summary>
    /// Количество вершин в дереве.
    /// </summary>
    public int Count => _tree.VertexCount;

    /// <summary>
    /// Родитель вершины.
    /// </summary>
    /// <param name="v">Вершина.</param>
    /// <returns>
    /// Родитель вершины, или <c>default</c>, если <paramref name="v"/> —
    /// корень дерева. Используйте <see cref="IsRoot"/> для однозначной
    /// проверки.
    /// </returns>
    /// <remarks>
    /// Для generic-типов под <c>where T : notnull</c> невозможно вернуть
    /// настоящий <c>null</c>, поэтому для корня возвращается
    /// <c>default(T)</c>. Проверить «это корень?» нужно через
    /// <see cref="IsRoot"/>.
    /// </remarks>
    public T Parent(T v) => _parent.TryGetValue(v, out var p) ? p : default!;

    /// <summary>
    /// Проверяет, является ли вершина корнем дерева.
    /// </summary>
    /// <param name="v">Вершина.</param>
    /// <returns><c>true</c>, если у вершины нет родителя.</returns>
    public bool IsRoot(T v) => !_parent.ContainsKey(v);

    /// <summary>
    /// Возвращает список отрезков <c>[left, right]</c> (включительно) в линейном
    /// массиве, объединение которых покрывает путь между <paramref name="u"/>
    /// и <paramref name="v"/>.
    /// </summary>
    /// <param name="u">Первая вершина пути.</param>
    /// <param name="v">Вторая вершина пути.</param>
    /// <returns>
    /// Перечисление отрезков в порядке от <paramref name="u"/> к <paramref name="v"/>.
    /// Каждый отрезок — непрерывный диапазон позиций, соответствующий части
    /// одного тяжёлого пути.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Применяйте к каждому отрезку свою операцию (min, max, sum и т. п.)
    /// через дерево отрезков или Fenwick.
    /// </para>
    /// <para>
    /// Порядок отрезков важен для <b>некоммутативных</b> операций
    /// (например, конкатенации строк, произведения матриц).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// foreach (var (left, right) in hld.PathSegments(4, 6))
    /// {
    ///     var partial = segmentTree.Query(left, right + 1);
    ///     result = combine(result, partial);
    /// }
    /// </code>
    /// </example>
    public IEnumerable<(int Left, int Right)> PathSegments(T u, T v)
    {
        // Собираем две «половинки» — от u вверх и от v вверх — и объединяем.
        var fromU = new List<(int Left, int Right)>();
        var fromV = new List<(int Left, int Right)>();

        while (!EqualityComparer<T>.Default.Equals(_head[u], _head[v]))
        {
            // Чья глава глубже — ту поднимаем
            if (_depth[_head[u]] < _depth[_head[v]])
            {
                // Поднимаем v
                fromV.Add((_position[_head[v]], _position[v]));
                v = _parent[_head[v]];
            }
            else
            {
                // Поднимаем u
                fromU.Add((_position[_head[u]], _position[u]));
                u = _parent[_head[u]];
            }
        }

        // Теперь u и v на одном тяжёлом пути.
        // Позиции внутри пути возрастают сверху вниз.
        int pu = _position[u], pv = _position[v];
        if (pu > pv) (pu, pv) = (pv, pu);

        // Финальный отрезок между u и v
        var middle = (pu, pv);

        // Половинка от исходного u идёт в обратном порядке (мы поднимались
        // вверх по дереву — сначала добавили дальние от u вершины).
        // Половинка от v идёт в правильном порядке (мы поднимались от v).
        for (int i = fromU.Count - 1; i >= 0; i--)
            yield return fromU[i];

        yield return middle;

        for (int i = 0; i < fromV.Count; i++)
            yield return fromV[i];
    }

    /// <summary>
    /// Возвращает все вершины на пути между <paramref name="u"/> и
    /// <paramref name="v"/> в порядке от <paramref name="u"/> до <paramref name="v"/>.
    /// </summary>
    /// <param name="u">Начальная вершина.</param>
    /// <param name="v">Конечная вершина.</param>
    /// <returns>Список вершин пути, включая <paramref name="u"/> и <paramref name="v"/>.</returns>
    public List<T> PathVertices(T u, T v)
    {
        var result = new List<T>();
        foreach (var (left, right) in PathSegments(u, v))
            for (int i = left; i <= right; i++)
                result.Add(_verticesByPosition[i]);
        return result;
    }

    /// <summary>
    /// Наименьший общий предок двух вершин.
    /// </summary>
    /// <param name="u">Первая вершина.</param>
    /// <param name="v">Вторая вершина.</param>
    /// <returns>LCA вершин <paramref name="u"/> и <paramref name="v"/>.</returns>
    /// <remarks>
    /// Сложность: <b>O(log V)</b>.
    /// </remarks>
    public T Lca(T u, T v)
    {
        while (!EqualityComparer<T>.Default.Equals(_head[u], _head[v]))
        {
            if (_depth[_head[u]] < _depth[_head[v]])
                v = _parent[_head[v]];
            else
                u = _parent[_head[u]];
        }
        return _depth[u] < _depth[v] ? u : v;
    }

    // ============================================================
    //  Внутренние методы
    // ============================================================

    /// <summary>
    /// Итеративный обход в глубину: вычисляет <c>_parent</c>, <c>_depth</c>,
    /// <c>_subtreeSize</c> и <c>_heavyChild</c>.
    /// </summary>
    /// <param name="root">Корень дерева.</param>
    /// <remarks>
    /// <para>
    /// Использует явный стек, чтобы избежать переполнения стека на больших
    /// деревьях. Порядок обхода сохраняется в <c>order</c>, затем
    /// обрабатывается в обратном порядке (post-order) для подсчёта размеров
    /// поддеревьев снизу вверх.
    /// </para>
    /// <para>
    /// Кортеж <c>(T Node, bool HasParent, T Parent)</c> используется вместо
    /// <c>T?</c>, потому что под <c>where T : notnull</c> невозможно
    /// присвоить <c>null</c> переменной типа <c>T?</c>.
    /// </para>
    /// </remarks>
    private void ComputeSizesAndHeavyChildren(T root)
    {
        var order = new List<T>();
        var stack = new Stack<(T Node, bool HasParent, T Parent)>();
        stack.Push((root, false, default!));

        _depth[root] = 0;
        // _parent[root] НЕ записываем — отсутствие ключа означает «корень».

        while (stack.Count > 0)
        {
            var (node, hasParent, parent) = stack.Pop();
            order.Add(node);

            foreach (var edge in _tree.Neighbors(node))
            {
                // Пропускаем родителя
                if (hasParent && EqualityComparer<T>.Default.Equals(edge.To, parent))
                    continue;

                // Пропускаем уже посещённые вершины
                if (_parent.ContainsKey(edge.To)) continue;

                // Защита от возврата в корень (для неориентированного графа)
                if (EqualityComparer<T>.Default.Equals(edge.To, root)) continue;

                _parent[edge.To] = node;
                _depth[edge.To] = _depth[node] + 1;
                stack.Push((edge.To, true, node));
            }
        }

        // Идём в обратном порядке — от листьев к корню
        for (int i = order.Count - 1; i >= 0; i--)
        {
            var node = order[i];
            int size = 1;
            T heavy = default!;
            bool hasHeavy = false;
            int maxChildSize = 0;

            bool hasParent = _parent.ContainsKey(node);
            T parent = hasParent ? _parent[node] : default!;

            foreach (var edge in _tree.Neighbors(node))
            {
                if (hasParent && EqualityComparer<T>.Default.Equals(edge.To, parent))
                    continue;
                if (!_parent.ContainsKey(edge.To)) continue;

                int childSize = _subtreeSize.TryGetValue(edge.To, out var s) ? s : 0;
                size += childSize;

                if (childSize > maxChildSize)
                {
                    maxChildSize = childSize;
                    heavy = edge.To;
                    hasHeavy = true;
                }
            }

            _subtreeSize[node] = size;
            if (hasHeavy)
                _heavyChild[node] = heavy;
        }
    }

    /// <summary>
    /// Разбивает дерево на тяжёлые пути и заполняет <c>_head</c> и <c>_position</c>.
    /// </summary>
    /// <param name="root">Корень дерева.</param>
    /// <param name="headOfRoot">Глава тяжёлого пути, начинающегося в корне.</param>
    /// <remarks>
    /// <para>
    /// Итеративный обход: для каждой вершины спускаемся по тяжёлому ребёнку
    /// до листа, добавляя вершины в линейный массив. Лёгкие дети
    /// обрабатываются через стек как начала новых тяжёлых путей.
    /// </para>
    /// <para>
    /// <b>Ключевое свойство:</b> вершины одного тяжёлого пути занимают
    /// непрерывный отрезок позиций. Это гарантируется тем, что мы
    /// «спускаемся» по тяжёлой цепочке до конца, прежде чем обрабатывать
    /// лёгких детей.
    /// </para>
    /// </remarks>
    private void Decompose(T root, T headOfRoot)
    {
        var stack = new Stack<(T Node, T Head)>();
        stack.Push((root, headOfRoot));

        while (stack.Count > 0)
        {
            var (start, head) = stack.Pop();

            // Проходим всю тяжёлую цепочку от start вниз
            T current = start;
            while (true)
            {
                _head[current] = head;
                _position[current] = _verticesByPosition.Count;
                _verticesByPosition.Add(current);

                bool hasHeavy = _heavyChild.TryGetValue(current, out var heavy);
                bool hasParent = _parent.TryGetValue(current, out var parent);

                // Лёгкие дети — начала новых тяжёлых путей.
                // Складываем их в стек (обрабатываются после текущей цепочки).
                foreach (var edge in _tree.Neighbors(current))
                {
                    if (hasParent && EqualityComparer<T>.Default.Equals(edge.To, parent))
                        continue;
                    if (hasHeavy && EqualityComparer<T>.Default.Equals(edge.To, heavy))
                        continue;

                    stack.Push((edge.To, edge.To));
                }

                // Спускаемся по тяжёлому ребёнку, если он есть
                if (!hasHeavy)
                    break;

                current = heavy!;
            }
        }
    }
}