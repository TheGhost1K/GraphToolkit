using GraphToolkit.Core;

namespace GraphToolkit.Centrality;

/// <summary>
/// Алгоритм PageRank для ранжирования вершин ориентированного графа.
/// </summary>
/// <remarks>
/// <para>
/// PageRank — итерационный алгоритм, присваивающий каждой вершине
/// число, отражающее её «важность» в графе на основе структуры ссылок.
/// Изначально предложен Ларри Пейджем и Сергеем Брином для поисковой
/// системы Google.
/// </para>
/// <para>
/// <b>Формула:</b>
/// <c>PR(v) = (1 - d) / V + d · Σ_{u → v} PR(u) / outdeg(u)</c>,
/// где <c>d</c> — damping factor (обычно 0.85).
/// </para>
/// <para>
/// <b>Сложность:</b> O(iterations · (V + E)).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var ranks = PageRank.Compute(graph, damping: 0.85, iterations: 100);
/// foreach (var (v, r) in ranks.OrderByDescending(kv => kv.Value))
///     Console.WriteLine($"{v}: {r:F4}");
/// </code>
/// </example>
public static class PageRank
{
    /// <summary>
    /// Вычисляет PageRank для всех вершин графа.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Ориентированный (или неориентированный) граф.</param>
    /// <param name="damping">
    /// Коэффициент затухания. По умолчанию 0.85. Должен быть в диапазоне [0, 1).
    /// </param>
    /// <param name="iterations">
    /// Число итераций. По умолчанию 100 — обычно достаточно для сходимости.
    /// </param>
    /// <param name="tolerance">
    /// Порог сходимости. Если максимальное изменение ранга на итерации
    /// меньше <paramref name="tolerance"/>, алгоритм останавливается.
    /// </param>
    /// <returns>Словарь «вершина → значение PageRank». Сумма значений ≈ 1.</returns>
    /// <exception cref="ArgumentException">
    /// Если <paramref name="damping"/> не в диапазоне [0, 1).
    /// </exception>
    public static Dictionary<T, double> Compute<T>(
        IGraph<T> graph,
        double damping = 0.85,
        int iterations = 100,
        double tolerance = 1e-6) where T : notnull
    {
        if (damping < 0 || damping >= 1)
            throw new ArgumentException(
                "Damping factor должен быть в диапазоне [0, 1).",
                nameof(damping));

        var vertices = graph.Vertices.ToList();
        int n = vertices.Count;
        if (n == 0) return new Dictionary<T, double>();

        double initial = 1.0 / n;
        var ranks = vertices.ToDictionary(v => v, _ => initial);
        var newRanks = new Dictionary<T, double>(n);

        // Исходящие степени
        var outDegree = vertices.ToDictionary(
            v => v,
            v => graph.Neighbors(v).Count());

        // Входящие рёбра (для эффективности — один раз)
        var incoming = vertices.ToDictionary(v => v, _ => new List<T>());
        foreach (var u in vertices)
            foreach (var e in graph.Neighbors(u))
                incoming[e.To].Add(u);

        for (int iter = 0; iter < iterations; iter++)
        {
            double baseRank = (1 - damping) / n;
            double diff = 0;

            foreach (var v in vertices)
            {
                double rank = baseRank;
                foreach (var u in incoming[v])
                {
                    int deg = outDegree[u];
                    if (deg > 0)
                        rank += damping * ranks[u] / deg;
                }
                newRanks[v] = rank;
                diff += Math.Abs(rank - ranks[v]);
            }

            // Обмен
            foreach (var v in vertices)
                ranks[v] = newRanks[v];

            if (diff < tolerance)
                break;
        }

        return ranks;
    }
}
