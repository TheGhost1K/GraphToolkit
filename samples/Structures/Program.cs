using GraphToolkit.Structures;
using System.Diagnostics;

namespace GraphToolkit.Samples.Structures;

/// <summary>
/// Пример: Segment Tree и Fenwick Tree для запросов на отрезках.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        SumExample();
        MinMaxExample();
        RangeUpdateExample();
        FenwickExample();
        PerformanceComparison();
    }

    // ============================================================
    //  1. Segment Tree: сумма
    // ============================================================

    private static void SumExample()
    {
        Section("1. Segment Tree: сумма на отрезке");

        var data = new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var st = new SegmentTree<long>(
            data,
            combine: (a, b) => a + b,
            identity: 0);

        Console.WriteLine("Массив: [" + string.Join(", ", data) + "]");
        Console.WriteLine();

        Console.WriteLine("Запросы:");
        Console.WriteLine($"  Query(0, 10) = {st.Query(0, 10)}");   // сумма всех = 55
        Console.WriteLine($"  Query(2, 5)  = {st.Query(2, 5)}");    // 3+4+5 = 12
        Console.WriteLine($"  Query(7, 10) = {st.Query(7, 10)}");   // 8+9+10 = 27
        Console.WriteLine();

        st.Update(4, 100);   // меняем 5 на 100
        Console.WriteLine("После Update(4, 100):");
        Console.WriteLine($"  Query(0, 10) = {st.Query(0, 10)}");   // 55 - 5 + 100 = 150
        Console.WriteLine($"  Query(2, 5)  = {st.Query(2, 5)}");    // 3+4+100 = 107
    }

    // ============================================================
    //  2. Segment Tree: min / max
    // ============================================================

    private static void MinMaxExample()
    {
        Section("2. Segment Tree: min / max");

        var data = new[] { 5, 3, 7, 1, 4, 8, 2, 6 };

        var minTree = new SegmentTree<int>(
            data,
            combine: Math.Min,
            identity: int.MaxValue);

        var maxTree = new SegmentTree<int>(
            data,
            combine: Math.Max,
            identity: int.MinValue);

        Console.WriteLine("Массив: [" + string.Join(", ", data) + "]");
        Console.WriteLine();

        Console.WriteLine("Минимум на отрезках:");
        Console.WriteLine($"  Min[0, 8) = {minTree.Query(0, 8)}");   // 1
        Console.WriteLine($"  Min[0, 4) = {minTree.Query(0, 4)}");   // 1 (5,3,7,1)
        Console.WriteLine($"  Min[4, 8) = {minTree.Query(4, 8)}");   // 2 (4,8,2,6)
        Console.WriteLine();

        Console.WriteLine("Максимум на отрезках:");
        Console.WriteLine($"  Max[0, 8) = {maxTree.Query(0, 8)}");   // 8
        Console.WriteLine($"  Max[0, 4) = {maxTree.Query(0, 4)}");   // 7
        Console.WriteLine($"  Max[4, 8) = {maxTree.Query(4, 8)}");   // 8
    }

    // ============================================================
    //  3. Segment Tree с range update
    // ============================================================

    private static void RangeUpdateExample()
    {
        Section("3. Range Update (присвоить значение на отрезке)");

        var data = new[] { 5, 3, 7, 1, 4, 8, 2, 6 };

        // Min-дерево с операцией «присвоить значение всем элементам отрезка»
        var st = new SegmentTree<int>(
            data,
            combine: Math.Min,
            identity: int.MaxValue,
            applyLazy: (_, newValue) => newValue,
            composeLazy: (_, newer) => newer);

        Console.WriteLine("Массив: [" + string.Join(", ", data) + "]");
        Console.WriteLine();

        // Присваиваем 10 отрезку [2, 6) — позиции 2, 3, 4, 5
        st.RangeUpdate(2, 6, 10);
        Console.WriteLine("После RangeUpdate(2, 6, 10):");
        for (int i = 0; i < 8; i++)
            Console.Write($"{st.GetAt(i),4}");
        Console.WriteLine();
        Console.WriteLine();

        Console.WriteLine($"  Min[0, 8) = {st.Query(0, 8)}");   // min(5,3,10,10,10,10,2,6) = 2
        Console.WriteLine($"  Min[2, 6) = {st.Query(2, 6)}");   // 10
        Console.WriteLine();

        // Ещё одно обновление
        st.RangeUpdate(0, 8, 20);
        Console.WriteLine("После RangeUpdate(0, 8, 20):");
        Console.WriteLine($"  Min[0, 8) = {st.Query(0, 8)}");   // 20
    }

    // ============================================================
    //  4. Fenwick Tree
    // ============================================================

    private static void FenwickExample()
    {
        Section("4. Fenwick Tree: быстрые суммы");

        var data = new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var bit = new FenwickTree<long>(
            data,
            add: (a, b) => a + b,
            identity: 0,
            subtract: (a, b) => a - b);

        Console.WriteLine("Массив: [" + string.Join(", ", data) + "]");
        Console.WriteLine();

        Console.WriteLine("Префиксные суммы:");
        Console.WriteLine($"  Prefix(0)  = {bit.PrefixAggregate(0)}");   // 0
        Console.WriteLine($"  Prefix(3)  = {bit.PrefixAggregate(3)}");   // 1+2+3 = 6
        Console.WriteLine($"  Prefix(10) = {bit.PrefixAggregate(10)}"); // 55
        Console.WriteLine();

        Console.WriteLine("Сумма на отрезке [l, r):");
        Console.WriteLine($"  Range(0, 10) = {bit.RangeAggregate(0, 10)}");  // 55
        Console.WriteLine($"  Range(2, 5)  = {bit.RangeAggregate(2, 5)}");   // 3+4+5 = 12
        Console.WriteLine($"  Range(7, 10) = {bit.RangeAggregate(7, 10)}");  // 8+9+10 = 27
        Console.WriteLine();

        // Обновление
        bit.Add(4, 100);   // добавляем 100 к позиции 4 (там было 5)
        Console.WriteLine("После Add(4, 100):");
        Console.WriteLine($"  Range(2, 5)  = {bit.RangeAggregate(2, 5)}");   // 3+4+105 = 112
        Console.WriteLine($"  Range(0, 10) = {bit.RangeAggregate(0, 10)}");  // 55 + 100 = 155
    }

    // ============================================================
    //  5. Сравнение производительности
    // ============================================================

    private static void PerformanceComparison()
    {
        Section("5. Segment Tree vs Fenwick: производительность");

        const int n = 100_000;
        const int queries = 100_000;

        var data = new long[n];
        var rnd = new Random(42);
        for (int i = 0; i < n; i++) data[i] = rnd.Next(1000);

        // Segment Tree
        var sw = Stopwatch.StartNew();
        var st = new SegmentTree<long>(
            data,
            combine: (a, b) => a + b,
            identity: 0);
        long stSum = 0;
        for (int i = 0; i < queries; i++)
        {
            int l = rnd.Next(n - 1);
            int r = rnd.Next(l + 1, n);
            stSum += st.Query(l, r);
        }
        sw.Stop();
        var stTime = sw.Elapsed;

        // Fenwick Tree
        sw.Restart();
        var bit = new FenwickTree<long>(
            data,
            add: (a, b) => a + b,
            identity: 0,
            subtract: (a, b) => a - b);
        long bitSum = 0;
        for (int i = 0; i < queries; i++)
        {
            int l = rnd.Next(n - 1);
            int r = rnd.Next(l + 1, n);
            bitSum += bit.RangeAggregate(l, r);
        }
        sw.Stop();
        var bitTime = sw.Elapsed;

        Console.WriteLine($"Размер массива: {n:N0}");
        Console.WriteLine($"Запросов:       {queries:N0}");
        Console.WriteLine();
        Console.WriteLine($"Segment Tree: {stTime.TotalMilliseconds,8:F2} мс  " +
                          $"(контрольная сумма: {stSum})");
        Console.WriteLine($"Fenwick Tree: {bitTime.TotalMilliseconds,8:F2} мс  " +
                          $"(контрольная сумма: {bitSum})");
        Console.WriteLine();

        if (bitTime < stTime)
            Console.WriteLine($"Fenwick быстрее в {stTime / bitTime:F2}×");
        else
            Console.WriteLine($"Segment Tree быстрее в {bitTime / stTime:F2}×");
        Console.WriteLine();
        Console.WriteLine("Вывод: для суммы и точечных обновлений Fenwick проще " +
                          "и быстрее.");
        Console.WriteLine("Для min/max и range updates — только Segment Tree.");
    }

    private static void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 60));
        Console.WriteLine($"  {title}");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine();
    }
}
