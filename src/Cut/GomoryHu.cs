using GraphToolkit.Core;
using GraphToolkit.Flow;

namespace GraphToolkit.Cut;

/// <summary>
/// Алгоритм Гомори-Ху для поиска всех пар минимальных разрезов
/// в неориентированном взвешенном графе.
/// </summary>
/// <remarks>
/// <para>
/// Строит <b>дерево Гомори-Ху</b>: дерево на V вершинах, в котором
/// минимальный разрез между любыми двумя вершинами равен минимальному
/// весу ребра на пути между ними в этом дереве.
/// </para>
/// <para>
/// <b>Сложность:</b> O(V) запусков алгоритма max-flow, то есть
/// O(V · MaxFlow(V, E)). Для плотных графов это существенно быстрее,
/// чем V² отдельных max-flow.
/// </para>
/// <para>
/// <b>Применимо к:</b> неориентированным графам с неотрицательными весами.
/// </para>
/// <para>
/// <b>Реализация:</b> классический алгоритм Гомори-Ху.
/// На каждом шаге выбирается очередная вершина <c>s</c>, вычисляется
/// минимальный разрез между <c>s</c> и её текущим родителем <c>t</c>,
/// затем обновляется структура дерева.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var g = new GraphBuilder&lt;string&gt;(isDirected: false)
///     .AddEdge("A", "B", 1)
///     .AddEdge("A", "C", 7)
///     .AddEdge("B", "C", 1)
///     .Build();
/// 
/// var tree = GomoryHu.Compute(g);
/// double minCutAB = tree.MinCut("A", "B");
/// </code>
/// </example>
public static class GomoryHu
{
    /// <summary>
    /// Результат работы алгоритма Гомори-Ху.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    public sealed class Result<T> where T : notnull
    {
        private readonly IReadOnlyList<(T U, T V, double Weight)> _edges;

        internal Result(IReadOnlyList<(T U, T V, double Weight)> edges)
        {
            _edges = edges;
        }

        /// <summary>
        /// Все рёбра дерева Гомори-Ху с весами.
        /// </summary>
        /// <value>
        /// Список кортежей <c>(U, V, Weight)</c> — неориентированные рёбра.
        /// Количество рёбер = V - 1 (или 0, если вершин меньше 2).
        /// </value>
        public IReadOnlyList<(T U, T V, double Weight)> Edges => _edges;

        /// <summary>
        /// Возвращает минимальный разрез между двумя вершинами.
        /// </summary>
        /// <param name="u">Первая вершина.</param>
        /// <param name="v">Вторая вершина.</param>
        /// <returns>
        /// Вес минимального разреза — минимум весов рёбер на пути
        /// между <paramref name="u"/> и <paramref name="v"/> в дереве
        /// Гомори-Ху.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Если <paramref name="u"/> и <paramref name="v"/> совпадают,
        /// возвращает <see cref="double.PositiveInfinity"/>.
        /// </para>
        /// <para>
        /// Если вершины в разных компонентах связности, возвращает <c>0</c>.
        /// </para>
        /// <para>
        /// Сложность: <b>O(V)</b> — BFS по дереву.
        /// </para>
        /// </remarks>
        public double MinCut(T u, T v)
        {
            if (EqualityComparer<T>.Default.Equals(u, v))
                return double.PositiveInfinity;

            // BFS по дереву от u до v
            var visited = new HashSet<T> { u };
            var queue = new Queue<T>();
            queue.Enqueue(u);
            var parent = new Dictionary<T, T>();

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (EqualityComparer<T>.Default.Equals(current, v))
                    break;

                foreach (var (a, b, _) in _edges)
                {
                    T next;
                    if (EqualityComparer<T>.Default.Equals(a, current))
                        next = b;
                    else if (EqualityComparer<T>.Default.Equals(b, current))
                        next = a;
                    else
                        continue;

                    if (visited.Add(next))
                    {
                        parent[next] = current;
                        queue.Enqueue(next);
                    }
                }
            }

            // Идём по дереву от v к u, ищем минимальный вес ребра
            double minWeight = double.PositiveInfinity;
            var node = v;

            while (!EqualityComparer<T>.Default.Equals(node, u))
            {
                if (!parent.TryGetValue(node, out var p))
                    return 0;   // разные компоненты связности

                // Ищем ребро (p, node) в списке
                double w = double.PositiveInfinity;
                foreach (var (a, b, weight) in _edges)
                {
                    if ((EqualityComparer<T>.Default.Equals(a, p) &&
                         EqualityComparer<T>.Default.Equals(b, node)) ||
                        (EqualityComparer<T>.Default.Equals(b, p) &&
                         EqualityComparer<T>.Default.Equals(a, node)))
                    {
                        w = weight;
                        break;
                    }
                }

                minWeight = Math.Min(minWeight, w);
                node = p;
            }

            return minWeight;
        }
    }

    /// <summary>
    /// Строит дерево Гомори-Ху для неориентированного графа.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Неориентированный взвешенный граф.</param>
    /// <returns>
    /// Дерево Гомори-Ху в виде <see cref="Result{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Если граф ориентированный (<c>graph.IsDirected == true</c>).
    /// </exception>
    /// <example>
    /// <code>
    /// var g = new GraphBuilder&lt;string&gt;(isDirected: false)
    ///     .AddEdge("A", "B", 1)
    ///     .AddEdge("B", "C", 1)
    ///     .AddEdge("A", "C", 3)
    ///     .Build();
    /// 
    /// var tree = GomoryHu.Compute(g);
    /// Console.WriteLine(tree.MinCut("A", "C"));   // 2
    /// </code>
    /// </example>
    public static Result<T> Compute<T>(IGraph<T> graph) where T : notnull
    {
        if (graph.IsDirected)
            throw new ArgumentException(
                "Алгоритм Гомори-Ху применим только к неориентированным графам.",
                nameof(graph));

        var vertices = graph.Vertices.ToList();
        int n = vertices.Count;

        // Менее двух вершин — тривиальный случай
        if (n < 2)
            return new Result<T>(Array.Empty<(T, T, double)>());

        // Индекс вершины в vertices
        var index = vertices
            .Select((v, i) => (v, i))
            .ToDictionary(x => x.v, x => x.i);

        // parent[v] — родитель v в дереве Гомори-Ху.
        // Изначально все родители — vertices[0], кроме самого vertices[0].
        var parent = new T[n];
        for (int i = 1; i < n; i++)
            parent[i] = vertices[0];

        // weight[v] — вес ребра (v — parent[v]) в дереве,
        // установленный на итерации, где v был source.
        // Для корня и ещё не обработанных вершин — 0.
        var weight = new double[n];

        // Основной цикл: обрабатываем вершины vertices[1], vertices[2], ..., vertices[n-1]
        for (int s = 1; s < n; s++)
        {
            var source = vertices[s];
            var sink = parent[s];

            // Шаг 1: строим сеть и запускаем max-flow
            var net = BuildNetwork(graph);
            double flow = Dinic.Compute(net, source, sink);

            // Шаг 2: находим сторону источника в минимальном разрезе
            var sourceSide = FindSourceSide(net, source);

            // Шаг 3: обновляем родителей
            // Для всех w != s, где w в sourceSide и parent[w] == sink:
            //   parent[w] = source
            for (int i = 0; i < n; i++)
            {
                if (i == s) continue;
                if (sourceSide.Contains(vertices[i]) &&
                    EqualityComparer<T>.Default.Equals(parent[i], sink))
                {
                    parent[i] = source;
                }
            }

            // Спец-случай: если sink в sourceSide, обменять родителей
            // parent[s] = parent[sink_index]
            // parent[sink_index] = source
            if (sourceSide.Contains(sink))
            {
                int tIndex = index[sink];
                parent[s] = parent[tIndex];
                parent[tIndex] = source;
            }

            // Шаг 4: сохраняем вес ребра, установленного для s
            // (актуальный parent[s] уже мог измениться — см. выше)
            weight[s] = flow;
        }

        // Собираем рёбра дерева: для каждой вершины v (кроме корня)
        // ребро (v, parent[v]) с весом weight[v].
        //
        // Корнем считается vertices[0] — у него нет родителя.
        var edges = new List<(T, T, double)>();
        for (int i = 1; i < n; i++)
        {
            edges.Add((vertices[i], parent[i], weight[i]));
        }

        return new Result<T>(edges);
    }

    /// <summary>
    /// Создаёт <see cref="FlowNetwork{T}"/> из неориентированного графа:
    /// каждое ребро дублируется в оба направления с одинаковой capacity.
    /// </summary>
    private static FlowNetwork<T> BuildNetwork<T>(IGraph<T> graph) where T : notnull
    {
        var net = new FlowNetwork<T>();

        foreach (var v in graph.Vertices)
            net.AddVertex(v);

        foreach (var e in graph.Edges)
        {
            net.AddEdge(e.From, e.To, e.Weight);
            net.AddEdge(e.To, e.From, e.Weight);
        }

        return net;
    }

    /// <summary>
    /// BFS по остаточному графу от источника — определяет множество
    /// вершин на стороне источника в минимальном разрезе.
    /// </summary>
    private static HashSet<T> FindSourceSide<T>(
        FlowNetwork<T> net, T source) where T : notnull
    {
        var reachable = new HashSet<T> { source };
        var queue = new Queue<T>();
        queue.Enqueue(source);

        while (queue.Count > 0)
        {
            var u = queue.Dequeue();
            foreach (var v in net.Neighbors(u))
            {
                if (!reachable.Contains(v) && net.GetCapacity(u, v) > 1e-12)
                {
                    reachable.Add(v);
                    queue.Enqueue(v);
                }
            }
        }

        return reachable;
    }
}