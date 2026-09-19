using GraphToolkit.Core;

namespace GraphToolkit.MinimumSpanningTree
{
    /// <summary>
    /// Алгоритм Краскала для поиска минимального остовного дерева (MST).
    /// </summary>
    /// <remarks>
    /// Сложность: O(E log E) благодаря сортировке рёбер и использованию 
    /// системы непересекающихся множеств (<see cref="UnionFind{T}"/>).
    /// </remarks>
    public static class Kruskal
    {
        /// <summary>
        /// Строит MST для неориентированного взвешенного графа.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Неориентированный граф.</param>
        /// <returns>Список рёбер MST.</returns>
        public static List<Edge<T>> Compute<T>(IGraph<T> graph) where T : notnull
        {
            var uf = new UnionFind<T>();
            foreach (var v in graph.Vertices) uf.MakeSet(v);

            var sorted = graph.Edges.OrderBy(e => e.Weight).ToList();
            var mst = new List<Edge<T>>();

            foreach (var e in sorted)
                if (uf.Union(e.From, e.To))
                    mst.Add(e);

            return mst;
        }
    }
}