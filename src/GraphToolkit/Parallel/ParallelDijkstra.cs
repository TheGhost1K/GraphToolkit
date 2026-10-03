using GraphToolkit.Core;

namespace GraphToolkit.Parallel;

/// <summary>
/// Параллельный Дейкстра через алгоритм delta-stepping.
/// </summary>
/// <remarks>
/// <para>
/// Идея delta-stepping: вершины разделяются на «корзины» по расстоянию.
/// Внутри одной корзины релаксация рёбер может выполняться параллельно.
/// </para>
/// <para>
/// Параметр <c>delta</c> управляет размером корзины:
/// </para>
/// <list type="bullet">
///   <item>При <c>delta → 0</c> алгоритм вырождается в обычную Дейкстру.</item>
///   <item>При <c>delta → ∞</c> — в Беллман-Форд.</item>
///   <item>Оптимальное значение зависит от графа и обычно подбирается эмпирически.</item>
/// </list>
/// <para>
/// <b>Сложность:</b> O((V + E) / cores · log V) в среднем.
/// </para>
/// </remarks>
public static class ParallelDijkstra
{
    /// <summary>
    /// Вычисляет кратчайшие расстояния через delta-stepping.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Взвешенный граф с неотрицательными весами.</param>
    /// <param name="source">Начальная вершина.</param>
    /// <param name="delta">Размер корзины. По умолчанию 1.0.</param>
    /// <returns>Расстояния и предшественники.</returns>
    public static (Dictionary<T, double> Distances, Dictionary<T, T> Predecessors)
        DeltaStepping<T>(IGraph<T> graph, T source, double delta = 1.0)
        where T : notnull
    {
        var vertices = graph.Vertices.ToList();
        var dist = vertices.ToDictionary(v => v, _ => double.PositiveInfinity);
        var prev = new Dictionary<T, T>();
        dist[source] = 0;

        if (delta <= 0) delta = 1.0;

        // Корзины: bucket[i] — вершины с dist ∈ [i*delta, (i+1)*delta)
        var buckets = new Dictionary<int, HashSet<T>>();
        var bucketOf = new Dictionary<T, int>();

        void AddToBucket(T v, double d)
        {
            int b = (int)(d / delta);
            if (!buckets.TryGetValue(b, out var set))
                buckets[b] = set = new HashSet<T>();
            set.Add(v);
            bucketOf[v] = b;
        }

        AddToBucket(source, 0);

        int currentBucket = 0;

        while (buckets.Count > 0)
        {
            // Находим минимальную непустую корзину
            var nonEmpty = buckets.Where(kv => kv.Value.Count > 0)
                .OrderBy(kv => kv.Key).ToList();
            if (nonEmpty.Count == 0) break;

            currentBucket = nonEmpty.First().Key;
            var bucket = buckets[currentBucket];

            while (bucket.Count > 0)
            {
                var nodes = bucket.ToList();
                bucket.Clear();

                // Релаксация рёбер параллельно
                var updates = new System.Collections.Concurrent.ConcurrentBag<(T V, double NewDist, T From)>();

                System.Threading.Tasks.Parallel.ForEach(nodes, u =>
                {
                    foreach (var e in graph.Neighbors(u))
                    {
                        double nd = dist[u] + e.Weight;
                        updates.Add((e.To, nd, u));
                    }
                });

                foreach (var (v, nd, from) in updates)
                {
                    if (nd < dist[v])
                    {
                        dist[v] = nd;
                        prev[v] = from;

                        // Переносим в правильную корзину
                        if (bucketOf.TryGetValue(v, out int oldB) &&
                            buckets.TryGetValue(oldB, out var oldSet))
                        {
                            oldSet.Remove(v);
                        }
                        AddToBucket(v, nd);
                    }
                }
            }
        }

        return (dist, prev);
    }

    /// <summary>
    /// Упрощённый вариант: обычная Дейкстра с параллельной релаксацией.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Взвешенный граф.</param>
    /// <param name="source">Начальная вершина.</param>
    /// <returns>Расстояния и предшественники.</returns>
    public static (Dictionary<T, double> Distances, Dictionary<T, T> Predecessors)
        Compute<T>(IGraph<T> graph, T source) where T : notnull => DeltaStepping(graph, source, delta: 1.0);
}
