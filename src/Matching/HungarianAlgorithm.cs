namespace GraphToolkit.Matching;

/// <summary>
/// Венгерский алгоритм (алгоритм Куна-Манкреса) для решения задачи
/// о назначениях на взвешенном двудольном графе.
/// </summary>
/// <remarks>
/// <para>
/// Задача: дана матрица стоимостей <c>cost[i, j]</c>.
/// Нужно найти перестановку <c>σ</c>, минимизирующую
/// <c>Σ cost[i, σ(i)]</c>.
/// </para>
/// <para>
/// <b>Сложность:</b> O(n³), где n — размер стороны.
/// </para>
/// <para>
/// Алгоритм предполагает, что матрица квадратная и все значения
/// конечны. Если матрица прямоугольная (m × n, m ≤ n), она
/// дополняется фиктивными строками с большим штрафом.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var cost = new double[,]
/// {
///     { 10, 5, 13 },
///     { 3, 7, 9 },
///     { 6, 8, 4 }
/// };
/// 
/// var result = HungarianAlgorithm.Solve(cost);
/// Console.WriteLine($"Минимум: {result.TotalCost}");
/// foreach (var (i, j) in result.Assignments)
///     Console.WriteLine($"Строка {i} → столбец {j}");
/// </code>
/// </example>
public static class HungarianAlgorithm
{
    /// <summary>
    /// Результат решения задачи о назначениях.
    /// </summary>
    /// <param name="Assignments">
    /// Пары <c>(строка, столбец)</c> — какая строка назначена на какой столбец.
    /// </param>
    /// <param name="TotalCost">Суммарная стоимость назначения.</param>
    public record Result(List<(int Row, int Col)> Assignments, double TotalCost);

    /// <summary>
    /// Решает задачу о назначениях (минимизация).
    /// </summary>
    /// <param name="cost">
    /// Матрица стоимостей <c>[n, n]</c>. Числа должны быть конечными.
    /// </param>
    /// <returns>Оптимальное назначение и его стоимость.</returns>
    /// <exception cref="ArgumentNullException">Если <paramref name="cost"/> равен <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Если матрица не квадратная или содержит NaN/Infinity.
    /// </exception>
    public static Result Solve(double[,] cost)
    {
        ArgumentNullException.ThrowIfNull(cost);
        int n = cost.GetLength(0);
        int m = cost.GetLength(1);
        if (n != m)
            throw new ArgumentException($"Матрица должна быть квадратной: {n}×{m}.", nameof(cost));

        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                if (double.IsNaN(cost[i, j]) || double.IsInfinity(cost[i, j]))
                    throw new ArgumentException(
                        $"Элемент [{i},{j}] не является конечным числом.", nameof(cost));

        if (n == 0) return new Result(new List<(int, int)>(), 0);

        // Копируем матрицу — алгоритм будет модифицировать
        var a = new double[n + 1, n + 1];
        for (int i = 1; i <= n; i++)
            for (int j = 1; j <= n; j++)
                a[i, j] = cost[i - 1, j - 1];

        // Классический венгерский алгоритм (1-based indexing для удобства)
        var u = new double[n + 1];
        var v = new double[n + 1];
        var p = new int[n + 1];      // p[j] = строка, назначенная на столбец j
        var way = new int[n + 1];    // way[j] = предыдущий столбец в augmenting path

        for (int i = 1; i <= n; i++)
        {
            p[0] = i;
            int j0 = 0;
            var minv = new double[n + 1];
            var used = new bool[n + 1];
            for (int j = 0; j <= n; j++) minv[j] = double.PositiveInfinity;

            do
            {
                used[j0] = true;
                int i0 = p[j0], j1 = -1;
                double delta = double.PositiveInfinity;

                for (int j = 1; j <= n; j++)
                {
                    if (used[j]) continue;
                    double cur = a[i0, j] - u[i0] - v[j];
                    if (cur < minv[j])
                    {
                        minv[j] = cur;
                        way[j] = j0;
                    }
                    if (minv[j] < delta)
                    {
                        delta = minv[j];
                        j1 = j;
                    }
                }

                for (int j = 0; j <= n; j++)
                {
                    if (used[j])
                    {
                        u[p[j]] += delta;
                        v[j] -= delta;
                    }
                    else
                    {
                        minv[j] -= delta;
                    }
                }

                j0 = j1;
            } while (p[j0] != 0);

            // Восстанавливаем augmenting path
            do
            {
                int j1 = way[j0];
                p[j0] = p[j1];
                j0 = j1;
            } while (j0 != 0);
        }

        // p[j] = строка, назначенная на столбец j
        var assignments = new List<(int, int)>();
        double totalCost = 0;

        for (int j = 1; j <= n; j++)
        {
            if (p[j] != 0)
            {
                int row = p[j] - 1;
                int col = j - 1;
                assignments.Add((row, col));
                totalCost += cost[row, col];
            }
        }

        assignments.Sort((x, y) => x.Item1.CompareTo(y.Item1));
        return new Result(assignments, totalCost);
    }

    /// <summary>
    /// Решает задачу о назначениях на максимизацию (например, максимизация прибыли).
    /// </summary>
    /// <param name="profit">Матрица прибыли.</param>
    /// <returns>Оптимальное назначение и суммарная прибыль.</returns>
    /// <remarks>
    /// Работает через приведение к задаче минимизации:
    /// <c>cost[i,j] = max(profit) - profit[i,j]</c>.
    /// </remarks>
    public static Result SolveMaximization(double[,] profit)
    {
        ArgumentNullException.ThrowIfNull(profit);
        int n = profit.GetLength(0);
        int m = profit.GetLength(1);
        if (n != m)
            throw new ArgumentException($"Матрица должна быть квадратной: {n}×{m}.", nameof(profit));

        double max = double.NegativeInfinity;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                if (profit[i, j] > max) max = profit[i, j];

        var cost = new double[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                cost[i, j] = max - profit[i, j];

        var minResult = Solve(cost);

        double totalProfit = 0;
        foreach (var (r, c) in minResult.Assignments)
            totalProfit += profit[r, c];

        return new Result(minResult.Assignments, totalProfit);
    }

    /// <summary>
    /// Решает задачу о назначениях на прямоугольной матрице m × n (m ≤ n),
    /// дополняя её фиктивными строками.
    /// </summary>
    /// <param name="cost">Прямоугольная матрица стоимостей.</param>
    /// <param name="fakePenalty">Штраф для фиктивных строк (по умолчанию — максимум × 10).</param>
    /// <returns>Оптимальное назначение и его стоимость.</returns>
    public static Result SolveRectangular(double[,] cost, double? fakePenalty = null)
    {
        ArgumentNullException.ThrowIfNull(cost);
        int rows = cost.GetLength(0);
        int cols = cost.GetLength(1);

        if (rows == cols) return Solve(cost);
        if (rows > cols)
            throw new ArgumentException(
                "Число строк не должно превышать число столбцов. Транспонируйте матрицу.",
                nameof(cost));

        double maxCost = 0;
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                if (cost[i, j] > maxCost) maxCost = cost[i, j];

        double penalty = fakePenalty ?? maxCost * 10 + 1;

        var padded = new double[cols, cols];
        for (int i = 0; i < cols; i++)
            for (int j = 0; j < cols; j++)
                padded[i, j] = i < rows ? cost[i, j] : penalty;

        var result = Solve(padded);

        // Отбрасываем фиктивные строки
        var real = result.Assignments
            .Where(a => a.Row < rows)
            .ToList();

        double totalCost = real.Sum(a => cost[a.Row, a.Col]);
        return new Result(real, totalCost);
    }
}