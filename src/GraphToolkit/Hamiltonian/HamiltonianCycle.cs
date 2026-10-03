using GraphToolkit.Core;

namespace GraphToolkit.Hamiltonian
{
    /// <summary>
    /// Поиск гамильтоновых циклов и путей (метод перебора с возвратом).
    /// </summary>
    /// <remarks>
    /// Задача NP-полная; сложность O(V!). Практически применимо 
    /// только к графам с небольшим числом вершин (обычно до 15–20).
    /// </remarks>
    public static class HamiltonianCycle
    {
        /// <summary>
        /// Ищет гамильтонов цикл.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <returns>Список вершин цикла или <c>null</c>, если цикла нет.</returns>
        public static List<T>? FindCycle<T>(IGraph<T> graph) where T : notnull
        {
            var vertices = graph.Vertices.ToList();
            if (vertices.Count == 0) return null;

            var path = new List<T>();
            var visited = new HashSet<T>();

            bool Backtrack(T current, T start)
            {
                path.Add(current);
                visited.Add(current);

                if (path.Count == vertices.Count)
                {
                    if (graph.Neighbors(current).Any(e =>
                        EqualityComparer<T>.Default.Equals(e.To, start)))
                        return true;
                }
                else
                {
                    foreach (var e in graph.Neighbors(current))
                        if (!visited.Contains(e.To) && Backtrack(e.To, start))
                            return true;
                }

                path.RemoveAt(path.Count - 1);
                visited.Remove(current);
                return false;
            }

            return Backtrack(vertices[0], vertices[0]) ? path : null;
        }

        /// <summary>
        /// Ищет гамильтонов путь (без возврата в начало).
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <returns>Список вершин пути или <c>null</c>, если пути нет.</returns>
        public static List<T>? FindPath<T>(IGraph<T> graph) where T : notnull
        {
            var vertices = graph.Vertices.ToList();
            if (vertices.Count == 0) return null;

            var path = new List<T>();
            var visited = new HashSet<T>();

            bool Backtrack(T current)
            {
                path.Add(current);
                visited.Add(current);

                if (path.Count == vertices.Count) return true;

                foreach (var e in graph.Neighbors(current))
                    if (!visited.Contains(e.To) && Backtrack(e.To))
                        return true;

                path.RemoveAt(path.Count - 1);
                visited.Remove(current);
                return false;
            }

            foreach (var start in vertices)
            {
                if (Backtrack(start)) return path;
                path.Clear();
                visited.Clear();
            }
            return null;
        }
    }
}