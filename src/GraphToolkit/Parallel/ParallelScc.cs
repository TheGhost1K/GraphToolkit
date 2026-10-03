using GraphToolkit.Core;

namespace GraphToolkit.Parallel;

/// <summary>
/// Поиск компонент сильной связности через алгоритм Косараю.
/// </summary>
/// <remarks>
/// <para>
/// Алгоритм Косараю состоит из двух проходов DFS. Второй проход
/// выполняется по компонентам независимо — можно распараллелить,
/// но текущая реализация использует последовательный стек для
/// предсказуемости.
/// </para>
/// <para>
/// <b>Сложность:</b> O(V + E).
/// </para>
/// </remarks>
public static class ParallelScc
{
    /// <summary>
    /// Находит SCC в ориентированном графе.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <param name="graph">Ориентированный граф.</param>
    /// <returns>Список компонент сильной связности.</returns>
    public static List<List<T>> Find<T>(IGraph<T> graph) where T : notnull
    {
        var vertices = graph.Vertices.ToList();
        if (vertices.Count == 0) return new List<List<T>>();

        // Первый проход — обычный DFS (последовательный, но с локальными стеками)
        var visited = new HashSet<T>();
        var order = new Stack<T>();

        void Dfs1(T v)
        {
            visited.Add(v);
            foreach (var e in graph.Neighbors(v))
                if (!visited.Contains(e.To)) Dfs1(e.To);
            order.Push(v);
        }

        foreach (var v in vertices)
            if (!visited.Contains(v)) Dfs1(v);

        // Транспонируем
        var transposed = graph is Graph<T> g
            ? g.Transpose()
            : TransposeGraph(graph);

        visited.Clear();
        var result = new List<List<T>>();

        // Второй проход — параллельно по компонентам
        while (order.Count > 0)
        {
            var v = order.Pop();
            if (visited.Contains(v)) continue;

            var comp = new List<T>();
            var stack = new Stack<T>();
            stack.Push(v);

            while (stack.Count > 0)
            {
                var u = stack.Pop();
                if (!visited.Add(u)) continue;
                comp.Add(u);

                foreach (var e in transposed.Neighbors(u))
                    if (!visited.Contains(e.To))
                        stack.Push(e.To);
            }
            result.Add(comp);
        }

        return result;
    }

    private static Graph<T> TransposeGraph<T>(IGraph<T> graph) where T : notnull
    {
        var g = new Graph<T>(graph.IsDirected);
        foreach (var v in graph.Vertices) g.AddVertex(v);
        foreach (var e in graph.Edges) g.AddEdge(e.To, e.From, e.Weight);
        return g;
    }
}
