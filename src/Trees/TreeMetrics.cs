using GraphToolkit.Core;
using GraphToolkit.ShortestPaths;

namespace GraphToolkit.Trees
{
    /// <summary>
    /// Метрики дерева: диаметр, центроид.
    /// </summary>
    public static class TreeMetrics
    {
        /// <summary>
        /// Результат поиска диаметра дерева.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="Distance">Длина диаметра.</param>
        /// <param name="Path">Список вершин на диаметре.</param>
        public sealed record DiameterResult<T>(double Distance, List<T> Path);

        /// <summary>
        /// Находит диаметр взвешенного дерева (самый длинный путь).
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="tree">Дерево.</param>
        /// <returns>Длина диаметра и путь.</returns>
        /// <remarks>Сложность: O(V log V) через две Дейкстры.</remarks>
        public static DiameterResult<T> Diameter<T>(IGraph<T> tree) where T : notnull
        {
            var start = tree.Vertices.First();

            (T Far, Dictionary<T, double> Dist, Dictionary<T, T> Prev) Explore(T from)
            {
                var (dist, prev) = Dijkstra.Compute(tree, from);
                var far = dist.OrderByDescending(kv => kv.Value).First().Key;
                return (far, dist, prev);
            }

            var (a, _, _) = Explore(start);
            var (b, distB, prevB) = Explore(a);

            var path = new List<T>();
            var cur = b;
            while (!EqualityComparer<T>.Default.Equals(cur, a))
            {
                path.Add(cur);
                cur = prevB[cur];
            }
            path.Add(a);
            path.Reverse();
            return new DiameterResult<T>(distB[b], path);
        }

        /// <summary>
        /// Находит центроид дерева — вершину, удаление которой минимизирует 
        /// максимальный размер оставшихся компонент.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="tree">Дерево.</param>
        /// <returns>Вершина-центроид.</returns>
        public static T Centroid<T>(IGraph<T> tree) where T : notnull
        {
            var vertices = tree.Vertices.ToList();
            int n = vertices.Count;
            var subtreeSize = new Dictionary<T, int>();
            var visited = new HashSet<T>();

            int Dfs(T u, T? p)
            {
                visited.Add(u);
                int size = 1;
                foreach (var e in tree.Neighbors(u))
                    if (!EqualityComparer<T>.Default.Equals(e.To, p) &&
                        !visited.Contains(e.To))
                        size += Dfs(e.To, u);
                subtreeSize[u] = size;
                return size;
            }

            Dfs(vertices[0], default);

            T Find(T u, T? p)
            {
                foreach (var e in tree.Neighbors(u))
                {
                    if (EqualityComparer<T>.Default.Equals(e.To, p)) continue;
                    if (subtreeSize[e.To] > n / 2)
                        return Find(e.To, u);
                }
                return u;
            }

            return Find(vertices[0], default);
        }
    }
}