using GraphToolkit.Core;

namespace GraphToolkit.Community;

/// <summary>
/// Обнаружение сообществ методом распространения меток (Label Propagation).
/// </summary>
/// <remarks>
/// <para>
/// Простой и быстрый алгоритм: каждая вершина итеративно принимает метку
/// своего самого частого соседа. За несколько итераций граф распадается
/// на сообщества.
/// </para>
/// <para>
/// <b>Сложность:</b> O(iterations · E). Обычно сходится за 5–10 итераций.
/// </para>
/// <para>
/// <b>Не детерминирован</b>: результат может отличаться между запусками.
/// Для воспроизводимости передайте фиксированный <c>seed</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var communities = LabelPropagation.Compute(graph, seed: 42);
/// // { { 1, 2, 3 }, { 4, 5, 6 } }
/// </code>
/// </example>
public static class LabelPropagation
{
    /// <summary>
    /// Находит сообщества методом распространения меток.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Неориентированный граф.</param>
    /// <param name="maxIterations">Максимум итераций (по умолчанию 20).</param>
    /// <param name="seed">Seed для воспроизводимости. <c>null</c> — случайный.</param>
    /// <returns>Список сообществ (каждое — список вершин).</returns>
    public static List<List<T>> Compute<T>(
        IGraph<T> graph,
        int maxIterations = 20,
        int? seed = null) where T : notnull
    {
        var vertices = graph.Vertices.ToList();
        int n = vertices.Count;
        if (n == 0) return new List<List<T>>();

        var rnd = seed.HasValue ? new Random(seed.Value) : new Random();

        // Инициализация: уникальные метки, перемешанные
        var labels = vertices.ToDictionary(v => v, v => v);

        // Предпосчитываем взвешенных соседей
        var weightedNeighbors = vertices.ToDictionary(
            v => v,
            v => graph.Neighbors(v).ToList());

        for (int iter = 0; iter < maxIterations; iter++)
        {
            bool changed = false;
            var order = vertices.OrderBy(_ => rnd.Next()).ToList();

            foreach (var v in order)
            {
                var neighbors = weightedNeighbors[v]
                    .Where(e => !EqualityComparer<T>.Default.Equals(e.To, v))
                    .ToList();

                if (neighbors.Count == 0) continue;

                // Взвешенная частота меток
                var weight = new Dictionary<T, double>();
                foreach (var e in neighbors)
                {
                    var label = labels[e.To];
                    weight[label] = weight.GetValueOrDefault(label) + e.Weight;
                }

                // Среди лидирующих — случайный выбор для устойчивости
                double maxWeight = weight.Values.Max();
                var candidates = weight
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

        // Группируем по метке
        return labels
            .GroupBy(kv => kv.Value)
            .Select(g => g.Select(kv => kv.Key).ToList())
            .ToList();
    }
}
