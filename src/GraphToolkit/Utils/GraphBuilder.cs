using GraphToolkit.Core;

namespace GraphToolkit.Utils
{
    /// <summary>
    /// Fluent-построитель графа для лаконичного создания.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    /// <example>
    /// <code>
    /// var graph = new GraphBuilder&lt;string&gt;(isDirected: true)
    ///     .AddEdge("A", "B", 4)
    ///     .AddEdge("B", "C", 2)
    ///     .Build();
    /// </code>
    /// </example>
    public sealed class GraphBuilder<T> where T : notnull
    {
        private readonly Graph<T> _graph;

        /// <summary>
        /// Инициализирует построитель.
        /// </summary>
        /// <param name="isDirected">Признак ориентированности.</param>
        public GraphBuilder(bool isDirected = false)
        {
            _graph = new Graph<T>(isDirected);
        }

        /// <summary>
        /// Добавляет вершину.
        /// </summary>
        /// <param name="vertex">Вершина.</param>
        /// <returns>Построитель для цепочки вызовов.</returns>
        public GraphBuilder<T> AddVertex(T vertex)
        {
            _graph.AddVertex(vertex);
            return this;
        }

        /// <summary>
        /// Добавляет ребро.
        /// </summary>
        /// <param name="from">Источник.</param>
        /// <param name="to">Приёмник.</param>
        /// <param name="weight">Вес.</param>
        /// <returns>Построитель для цепочки вызовов.</returns>
        public GraphBuilder<T> AddEdge(T from, T to, double weight = 1.0)
        {
            _graph.AddEdge(from, to, weight);
            return this;
        }

        /// <summary>
        /// Добавляет несколько рёбер.
        /// </summary>
        /// <param name="edges">Перечисление рёбер (from, to, weight).</param>
        /// <returns>Построитель для цепочки вызовов.</returns>
        public GraphBuilder<T> AddEdges(
            IEnumerable<(T From, T To, double Weight)> edges)
        {
            foreach (var (from, to, weight) in edges)
                _graph.AddEdge(from, to, weight);
            return this;
        }

        /// <summary>
        /// Возвращает построенный граф.
        /// </summary>
        /// <returns>Граф с добавленными вершинами и рёбрами.</returns>
        public Graph<T> Build() => _graph;
    }
}