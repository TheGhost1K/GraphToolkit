using GraphToolkit.Core;

namespace GraphToolkit.Community;

/// <summary>
/// Взвешенный алгоритм Label Propagation для обнаружения сообществ.
/// </summary>
/// <remarks>
/// <para>
/// Отличается от базового <see cref="LabelPropagation"/> тем, что
/// учитывает веса рёбер при выборе метки соседа. Это приводит к более
/// качественному разбиению на графах с сильно различающимися весами.
/// </para>
/// <para>
/// При равных весах выигрыш не наблюдается — используйте базовую версию.
/// </para>
/// <para>
/// <b>Сложность:</b> O(iterations · E).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var communities = WeightedLabelPropagation.Compute(graph, seed: 42);
/// </code>
/// </example>
public static class WeightedLabelPropagation
{
    /// <summary>
    /// Находит сообщества с учётом весов рёбер.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Неориентированный взвешенный граф.</param>
    /// <param name="maxIterations">
    /// Максимальное число итераций. По умолчанию 30.
    /// </param>
    /// <param name="seed">
    /// Seed для воспроизводимости. <c>null</c> — случайный.
    /// </param>
    /// <returns>Список сообществ.</returns>
    public static List<List<T>> Compute<T>(
        IGraph<T> graph,
        int maxIterations = 30,
        int? seed = null) where T : notnull
    {
        var vertices = graph.Vertices.ToList();
        if (vertices.Count == 0) return new List<List<T>>();

        var rnd = seed.HasValue ? new Random(seed.Value) : new Random();
        var labels = vertices.ToDictionary(v => v, v => v);

        var neighbors = vertices.ToDictionary(
            v => v,
            v => graph.Neighbors(v).ToList());

        for (int iter = 0; iter < maxIterations; iter++)
        {
            bool changed = false;
            var order = vertices.OrderBy(_ => rnd.Next()).ToList();

            foreach (var v in order)
            {
                var ns = neighbors[v];
                if (ns.Count == 0) continue;

                // Взвешенная частота меток
                var weightSum = new Dictionary<T, double>();
                foreach (var e in ns)
                {
                    var label = labels[e.To];
                    weightSum[label] = weightSum.GetValueOrDefault(label) + e.Weight;
                }

                double maxWeight = weightSum.Values.Max();
                var candidates = weightSum
                    .Where(kv => Math.Abs(kv.Value - maxWeight) < 1e-9)
                    .Select(kv => kv.Key)
                    .ToList();
                var chosen = candidates[rnd.Next(candidates.Count)];

                if (!EqualityComparer<T>.Default.Equals(chosen, labels[v]))
                {
                    labels[v] = chosen;
                    changed = true;
                }
            }

            if (!changed) break;
        }

        return labels
            .GroupBy(kv => kv.Value)
            .Select(g => g.Select(kv => kv.Key).ToList())
            .ToList();
    }
}
