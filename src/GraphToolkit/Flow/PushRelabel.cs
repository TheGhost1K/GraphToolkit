using GraphToolkit.Core;

namespace GraphToolkit.Flow;

/// <summary>
/// Алгоритм Push-Relabel (Goldberg-Tarjan) для задачи максимального потока.
/// </summary>
/// <remarks>
/// <para>
/// В отличие от Диница, который ищет увеличивающие пути, Push-Relabel
/// работает с «предпотоком»: поддерживает избыток в каждой вершине и
/// постепенно «проталкивает» его к стоку.
/// </para>
/// <para>
/// <b>Преимущества:</b> часто быстрее Диница на плотных графах.
/// <b>Сложность:</b> O(V³) в базовой реализации, O(V²·√E) с
/// эвристикой highest-label.
/// </para>
/// <para>
/// Использованная эвристика: разрядка вершин с наибольшей высотой
/// (highest-label) — одна из самых эффективных на практике.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var net = new FlowNetwork&lt;string&gt;();
/// net.AddEdge("S", "A", 16);
/// net.AddEdge("A", "T", 10);
/// double maxFlow = PushRelabel.Compute(net, "S", "T");
/// </code>
/// </example>
public static class PushRelabel
{
    /// <summary>
    /// Вычисляет максимальный поток от источника к стоку.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="network">
    /// Сеть с пропускными способностями. Не модифицируется —
    /// все изменения выполняются в локальных копиях.
    /// </param>
    /// <param name="source">Источник.</param>
    /// <param name="sink">Сток.</param>
    /// <returns>Величина максимального потока.</returns>
    /// <exception cref="ArgumentNullException">
    /// Если <paramref name="network"/>, <paramref name="source"/> 
    /// или <paramref name="sink"/> равны <c>null</c>.
    /// </exception>
    public static double Compute<T>(FlowNetwork<T> network, T source, T sink)
        where T : notnull
    {
        var vertices = network.Vertices.ToList();
        int n = vertices.Count;
        var index = vertices.Select((v, i) => (v, i))
            .ToDictionary(x => x.v, x => x.i);

        var capacity = new double[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                capacity[i, j] = 0;

        foreach (var u in vertices)
            foreach (var v in network.Neighbors(u))
                capacity[index[u], index[v]] = network.GetCapacity(u, v);

        int s = index[source], t = index[sink];

        var height = new int[n];
        var excess = new double[n];

        // Инициализация
        height[s] = n;
        for (int v = 0; v < n; v++)
        {
            if (v == s || v == t) continue;
            excess[v] = capacity[s, v];
            capacity[v, s] += capacity[s, v];
            capacity[s, v] = 0;
        }

        // Push операции
        void Push(int u, int v)
        {
            double d = Math.Min(excess[u], capacity[u, v]);
            if (d <= 1e-12) return;
            capacity[u, v] -= d;
            capacity[v, u] += d;
            excess[u] -= d;
            excess[v] += d;
        }

        // Relabel
        void Relabel(int u)
        {
            int minH = int.MaxValue;
            for (int v = 0; v < n; v++)
                if (capacity[u, v] > 1e-12)
                    minH = Math.Min(minH, height[v]);
            if (minH < int.MaxValue)
                height[u] = minH + 1;
        }

        // Discharge
        void Discharge(int u)
        {
            while (excess[u] > 1e-12)
            {
                bool pushed = false;
                for (int v = 0; v < n && excess[u] > 1e-12; v++)
                {
                    if (capacity[u, v] > 1e-12 && height[u] == height[v] + 1)
                    {
                        Push(u, v);
                        pushed = true;
                    }
                }
                if (!pushed) Relabel(u);
            }
        }

        // Highest-label
        while (true)
        {
            int u = -1;
            int maxH = -1;
            for (int v = 0; v < n; v++)
            {
                if (v == s || v == t) continue;
                if (excess[v] > 1e-12 && height[v] > maxH)
                {
                    maxH = height[v];
                    u = v;
                }
            }
            if (u == -1) break;
            Discharge(u);
        }

        return excess[t];
    }
}
