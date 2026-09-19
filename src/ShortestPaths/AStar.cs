using GraphToolkit.Core;

namespace GraphToolkit.ShortestPaths
{
    /// <summary>
    /// Алгоритм A* — эвристический поиск кратчайшего пути.
    /// </summary>
    /// <remarks>
    /// При допустимой (admissible) эвристике находит оптимальный путь. 
    /// Сложность в среднем существенно ниже Дейкстры.
    /// </remarks>
    public static class AStar
    {
        /// <summary>
        /// Находит путь от <paramref name="start"/> до <paramref name="goal"/> 
        /// с использованием эвристики.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <param name="start">Начальная вершина.</param>
        /// <param name="goal">Целевая вершина.</param>
        /// <param name="heuristic">
        /// Функция оценки расстояния между двумя вершинами. 
        /// Должна быть допустимой (не переоценивать реальное расстояние).
        /// </param>
        /// <returns>Список вершин пути или <c>null</c>, если путь не найден.</returns>
        public static List<T>? FindPath<T>(
            IGraph<T> graph, T start, T goal,
            Func<T, T, double> heuristic) where T : notnull
        {
            var openSet = new PriorityQueue<T, double>();
            var gScore = new Dictionary<T, double> { [start] = 0 };
            var fScore = new Dictionary<T, double> { [start] = heuristic(start, goal) };
            var cameFrom = new Dictionary<T, T>();
            var closed = new HashSet<T>();

            openSet.Enqueue(start, fScore[start]);

            while (openSet.TryDequeue(out var current, out _))
            {
                if (EqualityComparer<T>.Default.Equals(current, goal))
                    return ReconstructPath(cameFrom, start, goal);

                if (!closed.Add(current)) continue;

                foreach (var edge in graph.Neighbors(current))
                {
                    double tentativeG = gScore[current] + edge.Weight;
                    if (!gScore.TryGetValue(edge.To, out var existing) || tentativeG < existing)
                    {
                        cameFrom[edge.To] = current;
                        gScore[edge.To] = tentativeG;
                        fScore[edge.To] = tentativeG + heuristic(edge.To, goal);
                        openSet.Enqueue(edge.To, fScore[edge.To]);
                    }
                }
            }
            return null;
        }

        private static List<T> ReconstructPath<T>(
            Dictionary<T, T> parent, T start, T goal) where T : notnull
        {
            var path = new List<T> { goal };
            var current = goal;
            while (!EqualityComparer<T>.Default.Equals(current, start))
            {
                if (!parent.TryGetValue(current, out var p)) return new List<T>();
                current = p;
                path.Add(current);
            }
            path.Reverse();
            return path;
        }
    }
}