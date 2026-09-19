using GraphToolkit.Core;

namespace GraphToolkit.Components
{
    /// <summary>
    /// Поиск компонент сильной связности (SCC) алгоритмом Косараю.
    /// </summary>
    /// <remarks>
    /// Сложность: O(V + E). Требует транспонирования графа.
    /// </remarks>
    public static class StronglyConnectedComponents
    {
        /// <summary>
        /// Находит все SCC в ориентированном графе.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Ориентированный граф.</param>
        /// <returns>Список компонент сильной связности.</returns>
        public static List<List<T>> Find<T>(IGraph<T> graph) where T : notnull
        {
            var visited = new HashSet<T>();
            var order = new Stack<T>();

            void Dfs1(T v)
            {
                visited.Add(v);
                foreach (var e in graph.Neighbors(v))
                    if (!visited.Contains(e.To)) Dfs1(e.To);
                order.Push(v);
            }

            foreach (var v in graph.Vertices)
                if (!visited.Contains(v)) Dfs1(v);

            var transposed = graph is Graph<T> g
                ? g.Transpose()
                : TransposeGraph(graph);

            visited.Clear();
            var result = new List<List<T>>();

            void Dfs2(T v, List<T> comp)
            {
                visited.Add(v);
                comp.Add(v);
                foreach (var e in transposed.Neighbors(v))
                    if (!visited.Contains(e.To)) Dfs2(e.To, comp);
            }

            while (order.Count > 0)
            {
                var v = order.Pop();
                if (visited.Contains(v)) continue;
                var comp = new List<T>();
                Dfs2(v, comp);
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
}