using GraphToolkit.Core;

namespace GraphToolkit.MinimumSpanningTree
{
    /// <summary>
    /// Алгоритм Борувки для поиска MST.
    /// </summary>
    /// <remarks>
    /// Сложность: O(E log V). Эффективен на разреженных графах и 
    /// хорошо параллелизуется.
    /// </remarks>
    public static class Boruvka
    {
        /// <summary>
        /// Строит MST.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Неориентированный граф.</param>
        /// <returns>Список рёбер MST.</returns>
        public static List<Edge<T>> Compute<T>(IGraph<T> graph) where T : notnull
        {
            var uf = new UnionFind<T>();
            foreach (var v in graph.Vertices) uf.MakeSet(v);

            var mst = new List<Edge<T>>();
            var edges = graph.Edges.ToList();
            int components = graph.VertexCount;

            while (components > 1)
            {
                var cheapest = new Dictionary<T, Edge<T>>();

                foreach (var e in edges)
                {
                    var ru = uf.Find(e.From);
                    var rv = uf.Find(e.To);
                    if (EqualityComparer<T>.Default.Equals(ru, rv)) continue;

                    if (!cheapest.TryGetValue(ru, out var cu) || e.Weight < cu.Weight)
                        cheapest[ru] = e;
                    if (!cheapest.TryGetValue(rv, out var cv) || e.Weight < cv.Weight)
                        cheapest[rv] = e;
                }

                if (cheapest.Count == 0) break;

                bool merged = false;
                foreach (var e in cheapest.Values)
                {
                    if (uf.Union(e.From, e.To))
                    {
                        mst.Add(e);
                        components--;
                        merged = true;
                    }
                }
                if (!merged) break;
            }
            return mst;
        }
    }
}