using GraphToolkit.Core;

namespace GraphToolkit.Components
{
    /// <summary>
    /// Поиск компонент связности в неориентированном графе.
    /// </summary>
    public static class ConnectedComponents
    {
        /// <summary>
        /// Находит все компоненты связности.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <returns>Список компонент, каждая — список вершин.</returns>
        /// <remarks>Сложность: O(V + E).</remarks>
        public static List<List<T>> Find<T>(IGraph<T> graph) where T : notnull
        {
            var visited = new HashSet<T>();
            var components = new List<List<T>>();

            foreach (var v in graph.Vertices)
            {
                if (visited.Contains(v)) continue;

                var comp = new List<T>();
                var stack = new Stack<T>();
                stack.Push(v);

                while (stack.Count > 0)
                {
                    var cur = stack.Pop();
                    if (!visited.Add(cur)) continue;
                    comp.Add(cur);
                    foreach (var e in graph.Neighbors(cur))
                        if (!visited.Contains(e.To))
                            stack.Push(e.To);
                }
                components.Add(comp);
            }
            return components;
        }
    }
}