namespace GraphToolkit.Core
{
    /// <summary>
    /// Представляет взвешенное ребро графа, соединяющее две вершины.
    /// </summary>
    /// <typeparam name="T">Тип данных вершины. Должен корректно реализовывать 
    /// <see cref="object.Equals(object)"/> и <see cref="object.GetHashCode"/> 
    /// или использоваться со стандартными типами.</typeparam>
    /// <remarks>
    /// Ребро содержит начальную вершину <see cref="From"/>, конечную <see cref="To"/>
    /// и вес <see cref="Weight"/>. Для неориентированных графов ребро 
    /// <c>u — v</c> эквивалентно <c>v — u</c>, однако объекты <see cref="Edge{T}"/> 
    /// хранятся отдельно для каждой вершины.
    /// </remarks>
    /// <example>
    /// <code>
    /// var edge = new Edge&lt;string&gt;("A", "B", 3.5);
    /// Console.WriteLine(edge); // A -> B (3.5)
    /// </code>
    /// </example>
    public sealed class Edge<T> : IComparable<Edge<T>>, IEquatable<Edge<T>>
    {
        /// <summary>
        /// Начальная вершина ребра (источник).
        /// </summary>
        public T From { get; }

        /// <summary>
        /// Конечная вершина ребра (приёмник).
        /// </summary>
        public T To { get; }

        /// <summary>
        /// Вес ребра. По умолчанию равен <c>1.0</c>.
        /// </summary>
        /// <remarks>
        /// Может быть отрицательным для алгоритмов, поддерживающих отрицательные веса
        /// (например, <c>BellmanFord</c>). Некоторые алгоритмы (например, <c>Dijkstra</c>) 
        /// требуют неотрицательных весов.
        /// </remarks>
        public double Weight { get; }

        /// <summary>
        /// Инициализирует новый экземпляр ребра.
        /// </summary>
        /// <param name="from">Начальная вершина.</param>
        /// <param name="to">Конечная вершина.</param>
        /// <param name="weight">Вес ребра. По умолчанию <c>1.0</c>.</param>
        /// <exception cref="ArgumentNullException">
        /// Если <paramref name="from"/> или <paramref name="to"/> равны <c>null</c>.
        /// </exception>
        public Edge(T from, T to, double weight = 1.0)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            To = to ?? throw new ArgumentNullException(nameof(to));
            Weight = weight;
        }

        /// <inheritdoc/>
        public int CompareTo(Edge<T>? other)
        {
            if (other is null) return 1;
            return Weight.CompareTo(other.Weight);
        }

        /// <inheritdoc/>
        public bool Equals(Edge<T>? other)
        {
            if (other is null) return false;
            return EqualityComparer<T>.Default.Equals(From, other.From)
                && EqualityComparer<T>.Default.Equals(To, other.To)
                && Weight.Equals(other.Weight);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as Edge<T>);

        /// <inheritdoc/>
        public override int GetHashCode() =>
            HashCode.Combine(From, To, Weight);

        /// <summary>
        /// Возвращает строковое представление ребра в формате <c>"From -> To (Weight)"</c>.
        /// </summary>
        /// <returns>Строка вида <c>"A -> B (3.5)"</c>.</returns>
        public override string ToString() => $"{From} -> {To} ({Weight})";
    }
}