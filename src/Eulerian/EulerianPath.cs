using System.Collections.Generic;
using System.Linq;
using GraphToolkit.Core;

namespace GraphToolkit.Eulerian
{
    /// <summary>
    /// Проверка и построение эйлеровых путей и циклов.
    /// </summary>
    public static class EulerianPath
    {
        /// <summary>
        /// Результат проверки наличия эйлерова пути.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="Exists">Существует ли эйлеров путь.</param>
        /// <param name="HasEndpoints">
        /// Заданы ли конкретные начальная и конечная вершины.
        /// <c>false</c>, если путь — цикл (концы не определены) или путь не существует.
        /// </param>
        /// <param name="Start">Начальная вершина. Актуально только если <paramref name="HasEndpoints"/>.</param>
        /// <param name="End">Конечная вершина. Актуально только если <paramref name="HasEndpoints"/>.</param>
        public record PathCheckResult<T>(bool Exists, bool HasEndpoints, T Start, T End)
            where T : notnull;

        /// <summary>
        /// Проверяет, содержит ли граф эйлеров цикл.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <returns><c>true</c>, если существует замкнутый эйлеров путь.</returns>
        public static bool HasCycle<T>(IGraph<T> graph) where T : notnull
        {
            if (graph.VertexCount == 0) return false;
            if (!IsConnectedIgnoringIsolated(graph)) return false;

            if (graph.IsDirected)
            {
                var inDeg = graph.Vertices.ToDictionary(v => v, _ => 0);
                var outDeg = graph.Vertices.ToDictionary(v => v, _ => 0);
                foreach (var e in graph.Edges) { outDeg[e.From]++; inDeg[e.To]++; }
                return graph.Vertices.All(v => inDeg[v] == outDeg[v]);
            }
            else
            {
                return graph.Vertices.All(v => graph.Neighbors(v).Count() % 2 == 0);
            }
        }

        /// <summary>
        /// Проверяет, содержит ли граф эйлеров путь (не цикл).
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <returns>
        /// Результат с флагом <see cref="PathCheckResult{T}.Exists"/>
        /// и (если путь есть) начальной и конечной вершинами.
        /// Для цикла <see cref="PathCheckResult{T}.Start"/> и
        /// <see cref="PathCheckResult{T}.End"/> равны <c>default</c>.
        /// </returns>
        public static PathCheckResult<T> HasPath<T>(IGraph<T> graph)
            where T : notnull
        {
            if (graph.VertexCount == 0)
                return new PathCheckResult<T>(false, false, default!, default!);

            if (!IsConnectedIgnoringIsolated(graph))
                return new PathCheckResult<T>(false, false, default!, default!);

            if (graph.IsDirected)
            {
                var inDeg = graph.Vertices.ToDictionary(v => v, _ => 0);
                var outDeg = graph.Vertices.ToDictionary(v => v, _ => 0);
                foreach (var e in graph.Edges) { outDeg[e.From]++; inDeg[e.To]++; }

                T start = default!;
                T end = default!;
                int startCount = 0, endCount = 0;

                foreach (var v in graph.Vertices)
                {
                    int diff = outDeg[v] - inDeg[v];
                    if (diff == 1) { start = v; startCount++; }
                    else if (diff == -1) { end = v; endCount++; }
                    else if (diff != 0)
                        return new PathCheckResult<T>(false, false, default!, default!);
                }

                if (startCount == 0 && endCount == 0)
                    return new PathCheckResult<T>(true, false, default!, default!);  // цикл
                if (startCount == 1 && endCount == 1)
                    return new PathCheckResult<T>(true, true, start, end);           // путь

                return new PathCheckResult<T>(false, false, default!, default!);
            }
            else
            {
                var odd = graph.Vertices
                    .Where(v => graph.Neighbors(v).Count() % 2 == 1).ToList();

                if (odd.Count == 0)
                    return new PathCheckResult<T>(true, false, default!, default!);   // цикл
                if (odd.Count == 2)
                    return new PathCheckResult<T>(true, true, odd[0], odd[1]);        // путь

                return new PathCheckResult<T>(false, false, default!, default!);
            }
        }

        /// <summary>
        /// Строит эйлеров путь или цикл алгоритмом Хиерхольцера.
        /// </summary>
        /// <typeparam name="T">Тип данных вершины.</typeparam>
        /// <param name="graph">Граф.</param>
        /// <returns>
        /// Список вершин пути (замкнутый, если путь — цикл),
        /// или <c>null</c>, если эйлеров путь не существует.
        /// </returns>
        public static List<T>? Find<T>(IGraph<T> graph) where T : notnull
        {
            if (graph.EdgeCount == 0) return new List<T>();

            var check = HasPath(graph);
            T start;

            if (check.Exists && check.HasEndpoints)
            {
                start = check.Start;
            }
            else if (HasCycle(graph))
            {
                start = graph.Vertices.First();
            }
            else
            {
                return null;
            }

            var adj = new Dictionary<T, LinkedList<T>>();
            foreach (var v in graph.Vertices) adj[v] = new LinkedList<T>();

            if (graph.IsDirected)
            {
                foreach (var e in graph.Edges) adj[e.From].AddLast(e.To);
            }
            else
            {
                foreach (var v in graph.Vertices)
                    foreach (var e in graph.Neighbors(v))
                        adj[v].AddLast(e.To);
            }

            var stack = new Stack<T>();
            var circuit = new List<T>();
            stack.Push(start);

            while (stack.Count > 0)
            {
                var v = stack.Peek();
                if (adj[v].Count > 0)
                {
                    var u = adj[v].First!.Value;
                    adj[v].RemoveFirst();
                    if (!graph.IsDirected) adj[u].Remove(v);
                    stack.Push(u);
                }
                else
                {
                    circuit.Add(stack.Pop());
                }
            }

            circuit.Reverse();
            return circuit;
        }

        private static bool IsConnectedIgnoringIsolated<T>(IGraph<T> graph) where T : notnull
        {
            var active = graph.Vertices.Where(v =>
                graph.Neighbors(v).Any() ||
                graph.Edges.Any(e => EqualityComparer<T>.Default.Equals(e.To, v))).ToList();
            if (active.Count == 0) return true;

            var visited = new HashSet<T>();
            var stack = new Stack<T>();
            stack.Push(active[0]);

            while (stack.Count > 0)
            {
                var v = stack.Pop();
                if (!visited.Add(v)) continue;
                foreach (var e in graph.Neighbors(v))
                    if (!visited.Contains(e.To)) stack.Push(e.To);

                if (!graph.IsDirected)
                    foreach (var e in graph.Edges.Where(e =>
                        EqualityComparer<T>.Default.Equals(e.To, v)))
                        if (!visited.Contains(e.From)) stack.Push(e.From);
            }
            return active.All(v => visited.Contains(v));
        }
    }
}