namespace GraphToolkit.Core
{
    /// <summary>
    /// Базовый интерфейс графа, поддерживающего обход и доступ к рёбрам.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины.</typeparam>
    public interface IGraph<T>
    {
        /// <summary>
        /// Признак ориентированности графа.
        /// </summary>
        /// <value>
        /// <c>true</c>, если граф ориентированный; иначе <c>false</c>.
        /// </value>
        bool IsDirected { get; }

        /// <summary>
        /// Коллекция всех вершин графа.
        /// </summary>
        IEnumerable<T> Vertices { get; }

        /// <summary>
        /// Коллекция всех рёбер графа.
        /// </summary>
        /// <remarks>
        /// Для неориентированного графа каждое ребро хранится один раз 
        /// (в прямом направлении).
        /// </remarks>
        IReadOnlyList<Edge<T>> Edges { get; }

        /// <summary>
        /// Количество вершин в графе.
        /// </summary>
        int VertexCount { get; }

        /// <summary>
        /// Количество рёбер в графе.
        /// </summary>
        int EdgeCount { get; }

        /// <summary>
        /// Возвращает рёбра, исходящие из указанной вершины.
        /// </summary>
        /// <param name="vertex">Вершина, для которой запрашиваются соседи.</param>
        /// <returns>Коллекция исходящих рёбер; пустая, если вершина отсутствует.</returns>
        IEnumerable<Edge<T>> Neighbors(T vertex);
    }
}