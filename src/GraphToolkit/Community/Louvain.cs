using GraphToolkit.Core;

namespace GraphToolkit.Community;

/// <summary>
/// Обнаружение сообществ алгоритмом Лувена.
/// </summary>
/// <remarks>
/// <para>
/// Алгоритм Лувена максимизирует <b>модулярность</b> — метрику качества
/// разбиения графа на сообщества. Работает в два этапа:
/// </para>
/// <list type="number">
///   <item>
///     Локальная оптимизация: каждая вершина пытается перейти в сообщество
///     соседа, если это увеличивает модулярность.
///   </item>
///   <item>
///     Агрегация: сообщества сжимаются в супервершины, процесс повторяется.
///   </item>
/// </list>
/// <para>
/// <b>Сложность:</b> O(V log V · E) на практике.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var communities = Louvain.Compute(graph);
/// Console.WriteLine($"Модулярность: {Louvain.Modularity(graph, communities):F4}");
/// </code>
/// </example>
public static class Louvain
{
    /// <summary>
    /// Находит сообщества алгоритмом Лувена.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Неориентированный взвешенный граф.</param>
    /// <param name="maxPasses">Максимум проходов агрегации (по умолчанию 10).</param>
    /// <returns>Список сообществ.</returns>
    public static List<List<T>> Compute<T>(
        IGraph<T> graph,
        int maxPasses = 10) where T : notnull
    {
        var vertices = graph.Vertices.ToList();
        int n = vertices.Count;
        if (n == 0) return new List<List<T>>();

        var index = vertices
            .Select((v, i) => (v, i))
            .ToDictionary(x => x.v, x => x.i);

        var weight = new Dictionary<(int, int), double>();
        var degree = new double[n];
        double totalWeight = 0;

        foreach (var e in graph.Edges)
        {
            int u = index[e.From], v = index[e.To];
            if (u == v) continue;

            var key = u < v ? (u, v) : (v, u);
            weight[key] = weight.GetValueOrDefault(key) + e.Weight;
            degree[u] += e.Weight;
            degree[v] += e.Weight;
            totalWeight += e.Weight;
        }

        if (totalWeight == 0)
            return vertices.Select(v => new List<T> { v }).ToList();

        // Инициализация: каждая вершина — своё сообщество
        var community = Enumerable.Range(0, n).ToArray();
        var communityDegree = (double[])degree.Clone();

        // nodeToSuper[i] = индекс супервершины на текущем уровне,
        // в которую входит исходная вершина i
        var nodeToSuper = Enumerable.Range(0, n).ToArray();
        int originalN = n;

        const int MAX_LOCAL_ITERATIONS = 100;

        for (int pass = 0; pass < maxPasses; pass++)
        {
            bool improved = true;
            double m2 = 2 * totalWeight;
            int localIterations = 0;

            // Локальная оптимизация
            while (improved && localIterations < MAX_LOCAL_ITERATIONS)
            {
                improved = false;
                localIterations++;
                int moved = 0;

                for (int i = 0; i < n; i++)
                {
                    int oldComm = community[i];
                    var neighborWeights = new Dictionary<int, double>();

                    foreach (var ((u, v), w) in weight)
                    {
                        if (u == i) AddWeight(neighborWeights, community[v], w);
                        if (v == i) AddWeight(neighborWeights, community[u], w);
                    }

                    double bestGain = 0;
                    int bestComm = oldComm;

                    foreach (var (candidateComm, wToComm) in neighborWeights)
                    {
                        if (candidateComm == oldComm) continue;

                        double gain = wToComm
                            - communityDegree[candidateComm] * degree[i] / m2;

                        if (gain > bestGain)
                        {
                            bestGain = gain;
                            bestComm = candidateComm;
                        }
                    }

                    if (bestComm != oldComm)
                    {
                        communityDegree[oldComm] -= degree[i];
                        communityDegree[bestComm] += degree[i];
                        community[i] = bestComm;
                        improved = true;
                        moved++;
                    }
                }

                if (moved == 0) break;
            }

            // Агрегация
            var uniqueCommunities = community.Distinct().OrderBy(c => c).ToList();

            // Защита от бесконечного цикла
            if (uniqueCommunities.Count == n || uniqueCommunities.Count == 1)
                break;

            var commToSuper = uniqueCommunities
                .Select((c, i) => (c, i))
                .ToDictionary(x => x.c, x => x.i);

            // Обновляем отображение: исходная вершина → супервершина нового уровня.
            // nodeToSuper[i] содержит индекс вершины на текущем уровне (не метку!).
            // Сначала смотрим, в каком сообществе она находится (community[nodeToSuper[i]]),
            // затем — в какую супервершину это сообщество отображается (commToSuper[...]).
            for (int i = 0; i < originalN; i++)
                nodeToSuper[i] = commToSuper[community[nodeToSuper[i]]];

            // Строим новую матрицу весов между супервершинами
            var newWeight = new Dictionary<(int, int), double>();
            var newDegree = new double[uniqueCommunities.Count];

            foreach (var ((u, v), w) in weight)
            {
                int cu = commToSuper[community[u]];
                int cv = commToSuper[community[v]];
                if (cu == cv) continue;

                var key = cu < cv ? (cu, cv) : (cv, cu);
                newWeight[key] = newWeight.GetValueOrDefault(key) + w;
                newDegree[cu] += w;
                newDegree[cv] += w;
            }

            // Переходим на новый уровень
            weight = newWeight;
            degree = newDegree;
            n = uniqueCommunities.Count;
            community = Enumerable.Range(0, n).ToArray();
            communityDegree = (double[])degree.Clone();
        }

        // Собираем финальные сообщества по исходным вершинам
        var groups = new Dictionary<int, List<T>>();
        for (int i = 0; i < originalN; i++)
        {
            int c = nodeToSuper[i];
            if (!groups.TryGetValue(c, out var list))
                groups[c] = list = new List<T>();
            list.Add(vertices[i]);
        }

        return groups.Values.ToList();
    }

    /// <summary>
    /// Вычисляет модулярность разбиения на сообщества.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Неориентированный взвешенный граф.</param>
    /// <param name="communities">Список сообществ.</param>
    /// <returns>Значение модулярности в диапазоне [-0.5, 1].</returns>
    /// <remarks>
    /// Модулярность <c>Q = (1 / 2m) · Σ_{ij} (A_ij − k_i k_j / 2m) · δ(c_i, c_j)</c>.
    /// Чем выше, тем лучше разбиение.
    /// </remarks>
    public static double Modularity<T>(
        IGraph<T> graph,
        List<List<T>> communities) where T : notnull
    {
        var vertexToComm = new Dictionary<T, int>();
        for (int c = 0; c < communities.Count; c++)
            foreach (var v in communities[c])
                vertexToComm[v] = c;

        double totalWeight = 0;
        var degree = graph.Vertices.ToDictionary(v => v, _ => 0.0);

        foreach (var e in graph.Edges)
        {
            if (EqualityComparer<T>.Default.Equals(e.From, e.To)) continue;
            totalWeight += e.Weight;
            degree[e.From] += e.Weight;
            degree[e.To] += e.Weight;
        }

        if (totalWeight == 0) return 0;

        double m2 = 2 * totalWeight;
        double q = 0;

        foreach (var e in graph.Edges)
        {
            if (!vertexToComm.TryGetValue(e.From, out int cu)) continue;
            if (!vertexToComm.TryGetValue(e.To, out int cv)) continue;
            if (cu != cv) continue;

            q += e.Weight - degree[e.From] * degree[e.To] / m2;
        }

        return q / totalWeight;
    }

    private static void AddWeight<TKey>(
        Dictionary<TKey, double> dict, TKey key, double value)
        where TKey : notnull => dict[key] = dict.GetValueOrDefault(key) + value;
}
