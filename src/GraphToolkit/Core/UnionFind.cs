namespace GraphToolkit.Core
{
    /// <summary>
    /// Структура данных "Система непересекающихся множеств" (Disjoint Set Union)
    /// с эвристиками объединения по рангу и сжатия путей.
    /// </summary>
    /// <typeparam name="T">Тип элемента множества.</typeparam>
    /// <remarks>
    /// Амортизированная сложность операций <see cref="Find"/> и <see cref="Union"/> — 
    /// O(α(n)), где α — обратная функция Аккермана (практически константа).
    /// </remarks>
    public sealed class UnionFind<T> where T : notnull
    {
        private readonly Dictionary<T, T> _parent = new();
        private readonly Dictionary<T, int> _rank = new();

        /// <summary>
        /// Создаёт новое множество, содержащее единственный элемент.
        /// </summary>
        /// <param name="x">Элемент.</param>
        public void MakeSet(T x)
        {
            if (!_parent.ContainsKey(x))
            {
                _parent[x] = x;
                _rank[x] = 0;
            }
        }

        /// <summary>
        /// Находит представителя множества, содержащего элемент.
        /// </summary>
        /// <param name="x">Элемент.</param>
        /// <returns>Представитель множества.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Если элемент не был добавлен через <see cref="MakeSet"/>.
        /// </exception>
        public T Find(T x)
        {
            if (!EqualityComparer<T>.Default.Equals(_parent[x], x))
                _parent[x] = Find(_parent[x]);
            return _parent[x];
        }

        /// <summary>
        /// Объединяет два множества.
        /// </summary>
        /// <param name="x">Первый элемент.</param>
        /// <param name="y">Второй элемент.</param>
        /// <returns>
        /// <c>true</c>, если множества были объединены; 
        /// <c>false</c>, если элементы уже были в одном множестве.
        /// </returns>
        public bool Union(T x, T y)
        {
            var rx = Find(x);
            var ry = Find(y);
            if (EqualityComparer<T>.Default.Equals(rx, ry)) return false;

            if (_rank[rx] < _rank[ry]) _parent[rx] = ry;
            else if (_rank[rx] > _rank[ry]) _parent[ry] = rx;
            else { _parent[ry] = rx; _rank[rx]++; }
            return true;
        }
    }
}