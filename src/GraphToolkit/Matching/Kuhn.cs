namespace GraphToolkit.Matching
{
    /// <summary>
    /// Алгоритм Куна для максимального паросочетания в двудольном графе.
    /// </summary>
    public static class Kuhn
    {
        /// <summary>
        /// Находит максимальное паросочетание.
        /// </summary>
        /// <typeparam name="L">Тип данных левой доли.</typeparam>
        /// <typeparam name="R">Тип данных правой доли.</typeparam>
        /// <param name="left">Вершины левой доли.</param>
        /// <param name="neighbors">Функция, возвращающая соседей в правой доле.</param>
        /// <returns>Словарь "вершина левой доли → парная вершина правой доли".</returns>
        /// <remarks>Сложность: O(V * E).</remarks>
        public static Dictionary<L, R> Compute<L, R>(
            IEnumerable<L> left,
            Func<L, IEnumerable<R>> neighbors)
            where L : notnull where R : notnull
        {
            var matchR = new Dictionary<R, L>();
            var matchL = new Dictionary<L, R>();

            bool TryKuhn(L u, HashSet<R> visited)
            {
                foreach (var v in neighbors(u))
                {
                    if (!visited.Add(v)) continue;
                    if (!matchR.TryGetValue(v, out L value) || TryKuhn(value, visited))
                    {
                        value = u;
                        matchR[v] = value;
                        matchL[u] = v;
                        return true;
                    }
                }
                return false;
            }

            foreach (var u in left)
            {
                var visited = new HashSet<R>();
                TryKuhn(u, visited);
            }
            return matchL;
        }
    }
}