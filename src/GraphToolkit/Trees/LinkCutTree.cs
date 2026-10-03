namespace GraphToolkit.Trees;

/// <summary>
/// Link-Cut Tree — структура данных для динамического леса.
/// </summary>
/// <typeparam name="T">Тип данных вершины.</typeparam>
/// <remarks>
/// <para>
/// Поддерживает операции над лесным графом за амортизированное O(log V):
/// <list type="bullet">
///   <item><see cref="AddVertex"/> — добавить изолированную вершину.</item>
///   <item><see cref="SetValue"/> — задать вес вершины.</item>
///   <item><see cref="MakeRoot"/> — сделать вершину корнем её дерева.</item>
///   <item><see cref="Link"/> — добавить ребро между двумя деревьями.</item>
///   <item><see cref="Cut"/> — удалить ребро.</item>
///   <item><see cref="Connected"/> — проверить связность двух вершин.</item>
///   <item><see cref="Lca"/> — наименьший общий предок.</item>
///   <item><see cref="PathSum"/> — сумма весов на пути между двумя вершинами.</item>
/// </list>
/// </para>
/// <para>
/// <b>Вес вершин</b> задаётся через <see cref="SetValue"/> и агрегируется
/// операцией <c>sum</c>. Для других агрегатов (min, max, произвольный моноид)
/// адаптируйте реализацию: замените поле <c>Sum</c> и метод <c>Update</c>.
/// </para>
/// </remarks>
public sealed class LinkCutTree<T> where T : notnull
{
    private sealed class Node
    {
        public T Value = default!;
        public Node? Left, Right, Parent;
        public bool Reversed;
        public double Sum;
        public double OwnValue;
    }

    private readonly Dictionary<T, Node> _nodes = new();

    /// <summary>
    /// Создаёт вершину. Если она уже существует — ничего не делает.
    /// </summary>
    /// <param name="value">Значение вершины.</param>
    public void AddVertex(T value)
    {
        if (_nodes.ContainsKey(value)) return;
        _nodes[value] = new Node { Value = value };
    }

    /// <summary>
    /// Устанавливает вес вершины.
    /// </summary>
    /// <param name="v">Вершина.</param>
    /// <param name="ownValue">Собственный вес.</param>
    public void SetValue(T v, double ownValue)
    {
        var node = GetNode(v);
        Access(node);
        Splay(node);
        node.OwnValue = ownValue;
        Update(node);
    }

    /// <summary>
    /// Делает вершину корнем её дерева.
    /// </summary>
    public void MakeRoot(T v)
    {
        var node = GetNode(v);
        Access(node);
        Splay(node);
        ApplyReverse(node);
    }

    /// <summary>
    /// Добавляет ребро между вершинами разных деревьев.
    /// </summary>
    /// <returns><c>true</c>, если ребро добавлено.</returns>
    public bool Link(T u, T v)
    {
        var nu = GetNode(u);
        var nv = GetNode(v);
        if (Connected(u, v)) return false;

        MakeRoot(u);
        nu.Parent = nv;
        return true;
    }

    /// <summary>
    /// Удаляет ребро между вершинами.
    /// </summary>
    /// <returns><c>true</c>, если ребро было и удалено.</returns>
    public bool Cut(T u, T v)
    {
        if (!Connected(u, v)) return false;

        MakeRoot(u);
        var nv = GetNode(v);
        Access(nv);
        Splay(nv);

        // После MakeRoot(u) и Access(v): v — корень splay-дерева,
        // его левый ребёнок — u (или цепочка, ведущая к u).
        // Проверяем, что u — действительно прямой сосед.
        if (nv.Left is null) return false;
        nv.Left.Parent = null;
        nv.Left = null;
        Update(nv);
        return true;
    }

    /// <summary>
    /// Проверяет связность двух вершин.
    /// </summary>
    public bool Connected(T u, T v)
    {
        if (Equals(u, v)) return _nodes.ContainsKey(u);
        if (!_nodes.ContainsKey(u) || !_nodes.ContainsKey(v)) return false;

        var nu = GetNode(u);
        var nv = GetNode(v);
        Access(nu);
        Splay(nu);
        Access(nv);
        Splay(nv);
        return nu.Parent is not null || Equals(nu, nv);
    }

    /// <summary>
    /// Находит LCA двух вершин (в дереве, после <see cref="MakeRoot"/>).
    /// </summary>
    public T? Lca(T u, T v)
    {
        if (!Connected(u, v)) return default;
        var nu = GetNode(u);
        var nv = GetNode(v);
        Access(nu);
        Splay(nu);
        var result = Access(nv);   // возвращает последний узел, до которого был доступ
        return result is null ? default : result.Value;
    }

    /// <summary>
    /// Сумма весов вершин на пути между u и v.
    /// </summary>
    public double PathSum(T u, T v)
    {
        MakeRoot(u);
        var nv = GetNode(v);
        Access(nv);
        Splay(nv);
        return nv.Sum;
    }

    /// <summary>
    /// Проверяет, существует ли вершина.
    /// </summary>
    public bool Contains(T v) => _nodes.ContainsKey(v);

    // ---------- Splay + LCT ----------

    private Node GetNode(T v)
    {
        if (!_nodes.TryGetValue(v, out var node))
            throw new ArgumentException($"Вершина {v} не добавлена.", nameof(v));
        return node;
    }

    private bool IsRoot(Node x)
    {
        return x.Parent is null
            || (x.Parent.Left != x && x.Parent.Right != x);
    }

    private void Update(Node x)
    {
        x.Sum = x.OwnValue
            + (x.Left?.Sum ?? 0)
            + (x.Right?.Sum ?? 0);
    }

    private void ApplyReverse(Node x)
    {
        (x.Left, x.Right) = (x.Right, x.Left);
        x.Reversed = !x.Reversed;
    }

    private void PushDown(Node x)
    {
        if (x.Reversed)
        {
            if (x.Left is not null) ApplyReverse(x.Left);
            if (x.Right is not null) ApplyReverse(x.Right);
            x.Reversed = false;
        }
    }

    private void Rotate(Node x)
    {
        var p = x.Parent!;
        var g = p.Parent;

        if (p.Left == x)
        {
            p.Left = x.Right;
            x.Right?.Parent = p;
            x.Right = p;
        }
        else
        {
            p.Right = x.Left;
            x.Left?.Parent = p;
            x.Left = p;
        }
        p.Parent = x;
        x.Parent = g;

        if (g is not null)
        {
            if (g.Left == p) g.Left = x;
            else if (g.Right == p) g.Right = x;
        }
        Update(p);
        Update(x);
    }

    private void Splay(Node x)
    {
        var stack = new Stack<Node>();
        var y = x;
        stack.Push(y);
        while (!IsRoot(y) && y.Parent is not null)
        {
            y = y.Parent;
            stack.Push(y);
        }
        while (stack.Count > 0) PushDown(stack.Pop());

        while (!IsRoot(x))
        {
            var p = x.Parent!;
            var g = p.Parent;
            if (!IsRoot(p) && g is not null)
            {
                bool zigzig = (p.Left == x) == (g.Left == p);
                if (zigzig) Rotate(p);
                else Rotate(x);
            }
            Rotate(x);
        }
    }

    /// <summary>
    /// Доступ к вершине x — делает её корнем своего represented tree
    /// в splay-структуре. Возвращает последний узел, через который прошли
    /// (для LCA — это ответ).
    /// </summary>
    private Node? Access(Node x)
    {
        Node? last = null;
        var y = x;
        while (y is not null)
        {
            Splay(y);
            y.Right = last;
            Update(y);
            last = y;
            y = y.Parent;
        }
        Splay(x);
        return last;
    }
}