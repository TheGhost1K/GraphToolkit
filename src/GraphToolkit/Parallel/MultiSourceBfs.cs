using GraphToolkit.Core;
using System.Collections.Concurrent;

namespace GraphToolkit.Parallel;

/// <summary>
/// BFS от нескольких источников одновременно (multi-source BFS).
/// </summary>
/// <remarks>
/// <para>
/// Все источники обрабатываются параллельно на каждом уровне BFS.
/// Эффективен для задач «ближайший из N центров»: склады, больницы,
/// заправки, магазины.
/// </para>
/// <para>
/// <b>Сложность:</b> O((V + E) / cores).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var (dist, nearest) = MultiSourceBfs.Compute(graph, new[] { "Склад1", "Склад2" });
/// Console.WriteLine($"До ближайшего склада: {dist["Магазин"]}");
/// Console.WriteLine($"Это {nearest["Магазин"]}");
/// </code>
/// </example>
public static class MultiSourceBfs
{
    /// <summary>
    /// Находит расстояния до ближайшего из источников.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Неориентированный или ориентированный граф.</param>
    /// <param name="sources">Множество источников.</param>
    /// <returns>
    /// Кортеж: словарь «вершина → расстояние до ближайшего источника»
    /// и словарь «вершина → сам ближайший источник».
    /// </returns>
    public static (Dictionary<T, int> Distances, Dictionary<T, T> Sources)
        Compute<T>(IGraph<T> graph, IEnumerable<T> sources) where T : notnull
    {
        var dist = new ConcurrentDictionary<T, int>();
        var src = new ConcurrentDictionary<T, T>();
        var sourcesList = sources.ToList();

        var currentLevel = new List<T>();

        foreach (var s in sourcesList)
        {
            if (dist.TryAdd(s, 0))
            {
                src[s] = s;
                currentLevel.Add(s);
            }
        }

        int depth = 0;
        while (currentLevel.Count > 0)
        {
            depth++;
            var nextLevel = new ConcurrentBag<T>();

            System.Threading.Tasks.Parallel.ForEach(currentLevel, v =>
            {
                foreach (var e in graph.Neighbors(v))
                {
                    if (dist.TryAdd(e.To, depth))
                    {
                        src[e.To] = src[v];
                        nextLevel.Add(e.To);
                    }
                }
            });

            currentLevel = nextLevel.ToList();
        }

        return (new Dictionary<T, int>(dist), new Dictionary<T, T>(src));
    }
}
