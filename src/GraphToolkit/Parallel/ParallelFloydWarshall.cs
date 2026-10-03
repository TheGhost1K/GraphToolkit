using GraphToolkit.Core;

namespace GraphToolkit.Parallel;

/// <summary>
/// Параллельная версия алгоритма Флойда-Уоршелла для поиска всех пар
/// кратчайших путей.
/// </summary>
/// <remarks>
/// <para>
/// Внутренние циклы по <c>i</c> независимы при фиксированном <c>k</c>,
/// поэтому распараллеливаем цикл по <c>i</c>.
/// </para>
/// <para>
/// <b>Сложность:</b> O(V³ / cores).
/// </para>
/// <para>
/// <b>Ускорение:</b> 2–4× на многоядерных машинах. Для графов V &lt; 50
/// выигрыш минимален из-за накладных расходов.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var (dist, next, vertices) = ParallelFloydWarshall.Compute(graph);
/// double d = dist[i, j];
/// </code>
/// </example>
public static class ParallelFloydWarshall
{
    /// <summary>
    /// Вычисляет матрицы кратчайших расстояний параллельно.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Взвешенный граф (направленный или нет).</param>
    /// <returns>
    /// Кортеж из трёх элементов: матрица расстояний, матрица следующих
    /// вершин на пути и список вершин, соответствующих индексам матриц.
    /// </returns>
    public static (double[,] Distances, int[,] Next, List<T> Vertices)
        Compute<T>(IGraph<T> graph) where T : notnull
    {
        var vertices = graph.Vertices.ToList();
        var index = vertices
            .Select((v, i) => (v, i))
            .ToDictionary(x => x.v, x => x.i);
        int n = vertices.Count;

        var dist = new double[n, n];
        var next = new int[n, n];

        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                dist[i, j] = i == j ? 0 : double.PositiveInfinity;
                next[i, j] = -1;
            }

        foreach (var e in graph.Edges)
        {
            int u = index[e.From], v = index[e.To];
            if (e.Weight < dist[u, v])
            {
                dist[u, v] = e.Weight;
                next[u, v] = v;
            }
            if (!graph.IsDirected && e.Weight < dist[v, u])
            {
                dist[v, u] = e.Weight;
                next[v, u] = u;
            }
        }

        for (int k = 0; k < n; k++)
        {
            System.Threading.Tasks.Parallel.For(0, n, i =>
            {
                if (double.IsPositiveInfinity(dist[i, k])) return;
                for (int j = 0; j < n; j++)
                {
                    if (double.IsPositiveInfinity(dist[k, j])) continue;
                    double nd = dist[i, k] + dist[k, j];
                    if (nd < dist[i, j])
                    {
                        dist[i, j] = nd;
                        next[i, j] = next[i, k];
                    }
                }
            });
        }

        return (dist, next, vertices);
    }
}
