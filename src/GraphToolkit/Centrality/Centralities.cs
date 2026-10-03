using GraphToolkit.Core;
using GraphToolkit.ShortestPaths;

namespace GraphToolkit.Centrality;

/// <summary>
/// Метрики центральности вершин графа.
/// </summary>
/// <remarks>
/// Все методы возвращают словарь «вершина → значение центральности».
/// Значения нормализованы в [0, 1] там, где это имеет смысл.
/// </remarks>
public static class Centralities
{
    /// <summary>
    /// <b>Degree centrality</b> — нормированная степень вершины.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Граф.</param>
    /// <returns>Словарь «вершина → центральность» в диапазоне [0, 1].</returns>
    /// <remarks>
    /// Для графа на V вершинах максимальная степень — V - 1.
    /// </remarks>
    public static Dictionary<T, double> Degree<T>(IGraph<T> graph)
        where T : notnull
    {
        int n = graph.VertexCount;
        if (n <= 1)
            return graph.Vertices.ToDictionary(v => v, _ => 0.0);

        return graph.Vertices.ToDictionary(
            v => v,
            v => (double)graph.Neighbors(v).Count() / (n - 1));
    }

    /// <summary>
    /// <b>Closeness centrality</b> — обратная сумма расстояний до всех остальных.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Взвешенный граф с неотрицательными весами.</param>
    /// <returns>Словарь «вершина → центральность».</returns>
    /// <remarks>
    /// <para>
    /// Формула: <c>C(v) = (n - 1) / Σ_{u ≠ v} d(v, u)</c>.
    /// </para>
    /// <para>
    /// <b>Сложность:</b> O(V · (V + E) log V) — V запусков Дейкстры.
    /// Для больших графов используйте приближённые версии.
    /// </para>
    /// </remarks>
    public static Dictionary<T, double> Closeness<T>(IGraph<T> graph)
        where T : notnull
    {
        var vertices = graph.Vertices.ToList();
        int n = vertices.Count;
        var result = new Dictionary<T, double>(n);

        foreach (var v in vertices)
        {
            var (dist, _) = Dijkstra.Compute(graph, v);
            double sum = 0;
            int reachable = 0;

            foreach (var u in vertices)
            {
                if (EqualityComparer<T>.Default.Equals(u, v)) continue;
                double d = dist[u];
                if (!double.IsPositiveInfinity(d))
                {
                    sum += d;
                    reachable++;
                }
            }

            result[v] = sum > 0 && reachable > 0
                ? reachable / sum
                : 0.0;
        }

        return result;
    }

    /// <summary>
    /// <b>Betweenness centrality</b> — доля кратчайших путей, проходящих через вершину.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Взвешенный граф.</param>
    /// <returns>Словарь «вершина → центральность» (нормированная).</returns>
    /// <remarks>
    /// <para>
    /// Алгоритм Брандеса. <b>Сложность:</b> O(V · (V + E) log V).
    /// </para>
    /// <para>
    /// Для неориентированных графов нормировка — <c>2 / ((n-1)(n-2))</c>,
    /// для ориентированных — <c>1 / ((n-1)(n-2))</c>.
    /// </para>
    /// </remarks>
    public static Dictionary<T, double> Betweenness<T>(IGraph<T> graph)
        where T : notnull
    {
        var vertices = graph.Vertices.ToList();
        int n = vertices.Count;
        var cb = vertices.ToDictionary(v => v, _ => 0.0);

        foreach (var s in vertices)
        {
            var stack = new Stack<T>();
            var predecessors = vertices.ToDictionary(v => v, _ => new List<T>());
            var sigma = vertices.ToDictionary(v => v, _ => 0.0);
            var dist = vertices.ToDictionary(v => v, _ => -1.0);
            var delta = vertices.ToDictionary(v => v, _ => 0.0);

            sigma[s] = 1.0;
            dist[s] = 0.0;

            var queue = new Queue<T>();
            queue.Enqueue(s);

            while (queue.Count > 0)
            {
                var v = queue.Dequeue();
                stack.Push(v);

                foreach (var e in graph.Neighbors(v))
                {
                    var w = e.To;
                    if (dist[w] < 0)
                    {
                        dist[w] = dist[v] + e.Weight;
                        queue.Enqueue(w);
                    }

                    if (Math.Abs(dist[w] - (dist[v] + e.Weight)) < 1e-9)
                    {
                        sigma[w] += sigma[v];
                        predecessors[w].Add(v);
                    }
                }
            }

            while (stack.Count > 0)
            {
                var w = stack.Pop();
                foreach (var v in predecessors[w])
                    delta[v] += sigma[v] / sigma[w] * (1 + delta[w]);

                if (!EqualityComparer<T>.Default.Equals(w, s))
                    cb[w] += delta[w];
            }
        }

        // Нормировка
        double scale = graph.IsDirected
            ? 1.0 / ((n - 1) * (n - 2))
            : 2.0 / ((n - 1) * (n - 2));

        if (n <= 2) scale = 1.0;

        foreach (var v in vertices)
            cb[v] *= scale;

        return cb;
    }

    /// <summary>
    /// <b>Eigenvector centrality</b> — центральность по собственному вектору
    /// матрицы смежности.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Граф.</param>
    /// <param name="iterations">Число итераций степенного метода.</param>
    /// <param name="tolerance">Порог сходимости.</param>
    /// <returns>Словарь «вершина → центральность», нормированный (L2 = 1).</returns>
    /// <remarks>
    /// Использует метод степенных итераций: <c>x_{k+1} = A · x_k / ||A · x_k||</c>.
    /// </remarks>
    public static Dictionary<T, double> Eigenvector<T>(
        IGraph<T> graph,
        int iterations = 100,
        double tolerance = 1e-6) where T : notnull
    {
        var vertices = graph.Vertices.ToList();
        int n = vertices.Count;
        if (n == 0) return new Dictionary<T, double>();

        double initial = 1.0 / Math.Sqrt(n);
        var x = vertices.ToDictionary(v => v, _ => initial);
        var next = new Dictionary<T, double>(n);

        for (int iter = 0; iter < iterations; iter++)
        {
            double norm = 0;
            foreach (var v in vertices)
            {
                double sum = 0;
                foreach (var e in graph.Neighbors(v))
                    sum += x[e.To] * e.Weight;
                next[v] = sum;
                norm += sum * sum;
            }

            norm = Math.Sqrt(norm);
            if (norm < 1e-15) break;

            double diff = 0;
            foreach (var v in vertices)
            {
                double newVal = next[v] / norm;
                diff += Math.Abs(newVal - x[v]);
                x[v] = newVal;
            }

            if (diff < tolerance) break;
        }

        return x;
    }

    /// <summary>
    /// <b>Katz centrality</b> — взвешенная сумма всех путей между вершинами.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Граф.</param>
    /// <param name="alpha">
    /// Коэффициент затухания (обычно меньше 1 / λ_max, где λ_max — максимальное
    /// собственное значение матрицы смежности).
    /// </param>
    /// <param name="beta">Свободный член (обычно 1).</param>
    /// <param name="iterations">Число итераций.</param>
    /// <returns>Словарь «вершина → центральность».</returns>
    /// <remarks>
    /// Формула: <c>x_{k+1} = α · A · x_k + β · 1</c>.
    /// </remarks>
    public static Dictionary<T, double> Katz<T>(
        IGraph<T> graph,
        double alpha = 0.1,
        double beta = 1.0,
        int iterations = 100) where T : notnull
    {
        var vertices = graph.Vertices.ToList();
        var x = vertices.ToDictionary(v => v, _ => 0.0);
        var next = new Dictionary<T, double>(vertices.Count);

        for (int iter = 0; iter < iterations; iter++)
        {
            foreach (var v in vertices)
            {
                double sum = 0;
                foreach (var e in graph.Neighbors(v))
                    sum += x[e.To] * e.Weight;
                next[v] = alpha * sum + beta;
            }

            foreach (var v in vertices)
                x[v] = next[v];
        }

        return x;
    }
}
