using GraphToolkit.Matching;
using GraphToolkit.Utils;

namespace GraphToolkit.Samples.Matching;

/// <summary>
/// Пример: паросочетания и задача о назначениях.
/// </summary>
/// <remarks>
/// <para>
/// Демонстрирует три алгоритма:
/// </para>
/// <list type="bullet">
///   <item><b>Куна</b> — максимальное паросочетание в двудольном графе.</item>
///   <item><b>Blossom</b> — максимальное паросочетание в общем графе.</item>
///   <item><b>Венгерский</b> — оптимальное назначение (минимизация стоимости).</item>
/// </list>
/// </remarks>
public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        KuhnExample();
        BlossomExample();
        HungarianExample();
        RealWorldExample();
    }

    // ============================================================
    //  1. Куна — двудольный граф
    // ============================================================

    private static void KuhnExample()
    {
        Section("1. Куна — паросочетание в двудольном графе");

        // Задача: распределить студентов по курсам.
        // Студент может быть записан только на один курс,
        // каждый курс — только один студент.

        var students = new[] { "Алиса", "Борис", "Вика", "Гриша" };

        var preferences = new Dictionary<string, string[]>
        {
            ["Алиса"] = new[] { "Математика", "Физика" },
            ["Борис"] = new[] { "Математика" },
            ["Вика"] = new[] { "Физика", "Химия" },
            ["Гриша"] = new[] { "Математика", "Химия" }
        };

        Console.WriteLine("Предпочтения студентов:");
        foreach (var (student, courses) in preferences)
            Console.WriteLine($"  {student,-8} → {string.Join(", ", courses)}");
        Console.WriteLine();

        var matching = Kuhn.Compute(students, s => preferences[s]);

        Console.WriteLine($"Найдено пар: {matching.Count} из {students.Length}");
        foreach (var (student, course) in matching.OrderBy(kv => kv.Key))
            Console.WriteLine($"  {student,-8} ↔ {course}");
        Console.WriteLine();

        // Проверка максимальности
        int maxPossible = Math.Min(students.Length, preferences.Values.SelectMany(v => v).Distinct().Count());
        Console.WriteLine($"Максимум возможно: {maxPossible} (число уникальных курсов)");
    }

    // ============================================================
    //  2. Blossom — общий граф
    // ============================================================

    private static void BlossomExample()
    {
        Section("2. Blossom — паросочетание в общем графе");

        // Задача: разбить сотрудников на пары для совместных проектов.
        // Граф не двудольный — есть треугольники.

        var colleagues = new GraphBuilder<string>(isDirected: false)
            .AddEdge("Алиса", "Борис")
            .AddEdge("Борис", "Вика")
            .AddEdge("Вика", "Алиса")     // треугольник
            .AddEdge("Вика", "Гриша")
            .AddEdge("Гриша", "Даша")
            .AddEdge("Даша", "Егор")
            .AddEdge("Егор", "Гриша")     // второй треугольник
            .Build();

        Console.WriteLine($"Вершин: {colleagues.VertexCount}, " +
                          $"рёбер: {colleagues.EdgeCount}");
        Console.WriteLine();

        var matching = Blossom.Compute(colleagues);

        Console.WriteLine($"Найдено пар: {matching.Count}");
        foreach (var (a, b) in matching.OrderBy(m => m.Item1))
            Console.WriteLine($"  {a,-8} ↔ {b}");

        int covered = matching.Count * 2;
        Console.WriteLine($"\nПокрыто вершин: {covered} из {colleagues.VertexCount}");
        Console.WriteLine($"Совершенное паросочетание: " +
                          $"{Blossom.HasPerfectMatching(colleagues)}");
    }

    // ============================================================
    //  3. Венгерский — оптимальное назначение
    // ============================================================

    private static void HungarianExample()
    {
        Section("3. Венгерский — оптимальное назначение");

        // Задача: 4 разработчика, 4 задачи.
        // В таблице — время выполнения (часы).

        var timeMatrix = new double[,]
        {
            { 9, 2, 7, 8 },   // Иванов
            { 6, 4, 3, 7 },   // Петров
            { 5, 8, 1, 8 },   // Сидоров
            { 7, 6, 9, 4 }    // Кузнецов
        };

        var developers = new[] { "Иванов", "Петров", "Сидоров", "Кузнецов" };
        var tasks = new[] { "Backend", "Frontend", "DevOps", "QA" };

        Console.WriteLine("Матрица времени (часы):");
        Console.WriteLine("              " + string.Join("  ", tasks.Select(t => t.PadLeft(8))));
        for (int i = 0; i < developers.Length; i++)
        {
            Console.Write($"{developers[i],-12}");
            for (int j = 0; j < tasks.Length; j++)
                Console.Write($"{timeMatrix[i, j],10}");
            Console.WriteLine();
        }
        Console.WriteLine();

        var result = HungarianAlgorithm.Solve(timeMatrix);

        Console.WriteLine("Оптимальное назначение:");
        foreach (var (row, col) in result.Assignments.OrderBy(a => a.Row))
        {
            Console.WriteLine($"  {developers[row],-10} → {tasks[col],-10} " +
                              $"({timeMatrix[row, col]} ч)");
        }
        Console.WriteLine($"\nОбщее время: {result.TotalCost} часов");
        Console.WriteLine();

        // Наивное распределение (по индексу) для сравнения
        double naiveTime = 0;
        for (int i = 0; i < developers.Length; i++)
            naiveTime += timeMatrix[i, i];

        Console.WriteLine($"Наивное (диагональное): {naiveTime} часов");
        Console.WriteLine($"Экономия: {naiveTime - result.TotalCost} часов " +
                          $"({(naiveTime - result.TotalCost) / naiveTime:P0})");
    }

    // ============================================================
    //  4. Реальный пример: распределение рейсов
    // ============================================================

    private static void RealWorldExample()
    {
        Section("4. Реальный пример: распределение рейсов");

        // Авиакомпания: 5 пилотов, 5 рейсов.
        // Стоимость (условные единицы) — компенсация пилоту за рейс.

        var costMatrix = new double[,]
        {
            {  850, 1200,  960, 1100,  780 },   // Пилот 1
            { 1050,  920, 1300,  840, 1000 },   // Пилот 2
            {  900,  980,  870, 1250,  950 },   // Пилот 3
            { 1150,  880,  920,  900, 1080 },   // Пилот 4
            {  800, 1100, 1050,  970,  890 }    // Пилот 5
        };

        var pilots = Enumerable.Range(1, 5).Select(i => $"Пилот {i}").ToArray();
        var flights = new[] { "SU-101", "SU-102", "SU-103", "SU-104", "SU-105" };

        var result = HungarianAlgorithm.Solve(costMatrix);

        Console.WriteLine("Оптимальное распределение рейсов:");
        foreach (var (row, col) in result.Assignments.OrderBy(a => a.Row))
        {
            Console.WriteLine($"  {pilots[row],-10} → {flights[col],-8} " +
                              $"({costMatrix[row, col]} у.е.)");
        }
        Console.WriteLine($"\nМинимальная суммарная компенсация: {result.TotalCost} у.е.");
        Console.WriteLine();

        // Обратная задача: максимизация прибыли
        var profitMatrix = new double[,]
        {
            { 500, 800, 400, 700, 600 },
            { 750, 450, 900, 350, 700 },
            { 600, 650, 400, 950, 550 },
            { 850, 400, 500, 450, 800 },
            { 400, 700, 750, 600, 500 }
        };

        var maxResult = HungarianAlgorithm.SolveMaximization(profitMatrix);

        Console.WriteLine("Максимизация прибыли:");
        foreach (var (row, col) in maxResult.Assignments.OrderBy(a => a.Row))
        {
            Console.WriteLine($"  {pilots[row],-10} → {flights[col],-8} " +
                              $"({profitMatrix[row, col]} у.е.)");
        }
        Console.WriteLine($"\nМаксимальная суммарная прибыль: {maxResult.TotalCost} у.е.");

        // Прямоугольная матрица: пилотов меньше, чем рейсов
        Console.WriteLine();
        Console.WriteLine("Прямоугольная матрица (3 пилота, 5 рейсов):");

        var rectangular = new double[,]
        {
            {  850, 1200,  960, 1100,  780 },
            { 1050,  920, 1300,  840, 1000 },
            {  900,  980,  870, 1250,  950 }
        };

        var rectResult = HungarianAlgorithm.SolveRectangular(rectangular);

        Console.WriteLine($"Распределено пар: {rectResult.Assignments.Count}");
        foreach (var (row, col) in rectResult.Assignments.OrderBy(a => a.Row))
        {
            Console.WriteLine($"  {pilots[row],-10} → {flights[col],-8} " +
                              $"({rectangular[row, col]} у.е.)");
        }
        Console.WriteLine($"\nСуммарная компенсация: {rectResult.TotalCost} у.е.");
        Console.WriteLine("\nОставшиеся рейсы: " +
            string.Join(", ", Enumerable.Range(0, flights.Length)
                .Except(rectResult.Assignments.Select(a => a.Col))
                .Select(i => flights[i])));
    }

    // ============================================================
    //  Утилиты
    // ============================================================

    private static void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 60));
        Console.WriteLine($"  {title}");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine();
    }
}
