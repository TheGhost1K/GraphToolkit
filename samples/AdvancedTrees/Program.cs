using GraphToolkit.Trees;
using GraphToolkit.Utils;

namespace GraphToolkit.Samples.AdvancedTrees;

/// <summary>
/// Пример: продвинутые алгоритмы на деревьях — HLD, Link-Cut Tree
/// и Centroid Decomposition.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        CentroidDecompositionExample();
        HldExample();
        HldPathQueriesExample();
        LinkCutTreeExample();
        ComparisonSummary();
    }

    // ============================================================
    //  1. Centroid Decomposition
    // ============================================================

    private static void CentroidDecompositionExample()
    {
        Section("1. Centroid Decomposition — расстояния между вершинами");

        // Путь из 7 вершин.
        var tree = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2, 1)
            .AddEdge(2, 3, 1)
            .AddEdge(3, 4, 1)
            .AddEdge(4, 5, 1)
            .AddEdge(5, 6, 1)
            .AddEdge(6, 7, 1)
            .Build();

        var cd = new CentroidDecomposition<int>(tree);

        Console.WriteLine($"Вершин в дереве: {tree.VertexCount}");
        Console.WriteLine($"Корень центроидной декомпозиции: {cd.Root}");
        Console.WriteLine();

        Console.WriteLine("Расстояния между парами вершин:");
        var pairs = new[] { (1, 7), (2, 6), (3, 5), (1, 4), (4, 7) };
        foreach (var (u, v) in pairs)
            Console.WriteLine($"  Distance({u}, {v}) = {cd.Distance(u, v)}");

        Console.WriteLine();
        Console.WriteLine("Глубины вершин в дереве центроидов:");
        foreach (var v in cd.Traverse())
            Console.WriteLine($"  {v} (глубина {cd.Depth(v)})");

        Console.WriteLine();
        Console.WriteLine($"Высота дерева центроидов — O(log V). " +
                          $"Для V=7 log₂V ≈ {Math.Log2(7):F1}");
    }

    // ============================================================
    //  2. Heavy-Light Decomposition
    // ============================================================

    private static void HldExample()
    {
        Section("2. Heavy-Light Decomposition — LCA и пути");

        //       1
        //      / \
        //     2   3
        //    / \   \
        //   4   5   6
        //  /
        // 7
        var tree = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2)
            .AddEdge(1, 3)
            .AddEdge(2, 4)
            .AddEdge(2, 5)
            .AddEdge(3, 6)
            .AddEdge(4, 7)
            .Build();

        var hld = new HeavyLightDecomposition<int>(tree, root: 1);

        Console.WriteLine("Построение:");
        foreach (var v in new[] { 1, 2, 3, 4, 5, 6, 7 })
        {
            Console.WriteLine($"  {v,-2} позиция={hld.Position(v)}, " +
                              $"head={hld.Head(v)}, depth={hld.Depth(v)}");
        }
        Console.WriteLine();

        Console.WriteLine("LCA для разных пар:");
        var lcaPairs = new[] { (4, 5), (4, 6), (7, 3), (7, 6) };
        foreach (var (u, v) in lcaPairs)
            Console.WriteLine($"  LCA({u}, {v}) = {hld.Lca(u, v)}");
        Console.WriteLine();

        Console.WriteLine("Путь 7 → 6 (все вершины по порядку):");
        var path = hld.PathVertices(7, 6);
        Console.WriteLine("  " + string.Join(" → ", path));
        Console.WriteLine();

        Console.WriteLine("Разбиение пути 7 → 6 на отрезки (для дерева отрезков):");
        foreach (var (left, right) in hld.PathSegments(7, 6))
            Console.WriteLine($"  отрезок [{left}, {right}]");
    }

    // ============================================================
    //  3. HLD + запросы на путях
    // ============================================================

    private static void HldPathQueriesExample()
    {
        Section("3. HLD + запросы на путях (дерево отрезков)");

        // Взвешенное дерево: у каждой вершины — вес (например, количество
        // заказов, обработанных курьером).
        var tree = new GraphBuilder<string>(isDirected: false)
            .AddEdge("A", "B").AddEdge("A", "C")
            .AddEdge("B", "D").AddEdge("B", "E")
            .AddEdge("C", "F")
            .Build();

        var weights = new Dictionary<string, long>
        {
            ["A"] = 10,
            ["B"] = 20,
            ["C"] = 30,
            ["D"] = 40,
            ["E"] = 50,
            ["F"] = 60
        };

        // Суммы на путях
        var sumQueries = new HldPathQueries<string, long>(
            tree, root: "A",
            valueOf: v => weights[v],
            combine: (a, b) => a + b,
            identity: 0);

        Console.WriteLine("Веса вершин:");
        foreach (var (v, w) in weights)
            Console.WriteLine($"  {v} = {w}");
        Console.WriteLine();

        Console.WriteLine("Сумма весов на пути:");
        Console.WriteLine($"  D → E: {sumQueries.Query("D", "E")}");   // 40+20+50 = 110
        Console.WriteLine($"  D → F: {sumQueries.Query("D", "F")}");   // 40+20+10+30+60 = 160
        Console.WriteLine($"  E → F: {sumQueries.Query("E", "F")}");   // 50+20+10+30+60 = 170
        Console.WriteLine();

        // Обновляем вес
        sumQueries.Update("B", 100);
        Console.WriteLine("После Update(B, 100):");
        Console.WriteLine($"  D → E: {sumQueries.Query("D", "E")}");   // 40+100+50 = 190
        Console.WriteLine();

        // Минимум на пути
        var minQueries = new HldPathQueries<string, long>(
            tree, root: "A",
            valueOf: v => weights[v],
            combine: Math.Min,
            identity: long.MaxValue);

        Console.WriteLine("Минимум на пути:");
        Console.WriteLine($"  D → F: {minQueries.Query("D", "F")}");   // min(40,100,10,30,60) = 10
        Console.WriteLine($"  D → E: {minQueries.Query("D", "E")}");   // min(40,100,50) = 40
    }

    // ============================================================
    //  4. Link-Cut Tree
    // ============================================================

    private static void LinkCutTreeExample()
    {
        Section("4. Link-Cut Tree — динамический лес");

        var lct = new LinkCutTree<int>();

        // Добавляем вершины с весами
        for (int i = 1; i <= 6; i++)
        {
            lct.AddVertex(i);
            lct.SetValue(i, i * 10);
        }

        Console.WriteLine("Добавлено 6 вершин с весами 10, 20, 30, 40, 50, 60");
        Console.WriteLine();

        // Строим дерево
        lct.Link(1, 2);
        lct.Link(2, 3);
        lct.Link(3, 4);
        lct.Link(3, 5);
        lct.Link(5, 6);

        Console.WriteLine("После Link (1-2, 2-3, 3-4, 3-5, 5-6):");
        Console.WriteLine($"  Connected(1, 6): {lct.Connected(1, 6)}");
        Console.WriteLine($"  PathSum(1, 6):   {lct.PathSum(1, 6)}");   // 10+20+30+50+60 = 170
        Console.WriteLine($"  PathSum(1, 4):   {lct.PathSum(1, 4)}");   // 10+20+30+40 = 100
        Console.WriteLine();

        // Удаляем ребро
        Console.WriteLine("Cut(3, 5):");
        lct.Cut(3, 5);
        Console.WriteLine($"  Connected(1, 6): {lct.Connected(1, 6)}");   // false
        Console.WriteLine($"  Connected(1, 4): {lct.Connected(1, 4)}");   // true
        Console.WriteLine($"  Connected(3, 6): {lct.Connected(3, 6)}");   // false
        Console.WriteLine();

        // Снова соединяем
        Console.WriteLine("Link(1, 6) — восстановили связь через другую вершину:");
        lct.Link(1, 6);
        Console.WriteLine($"  Connected(1, 5): {lct.Connected(1, 5)}");   // true (1-2-3-5)
        Console.WriteLine($"  Connected(1, 6): {lct.Connected(1, 6)}");   // true (1-6)
        Console.WriteLine($"  PathSum(5, 6):   {lct.PathSum(5, 6)}");     // 50+30+20+10+60 = 170
    }

    // ============================================================
    //  5. Сравнение подходов
    // ============================================================

    private static void ComparisonSummary()
    {
        Section("5. Когда что использовать");

        Console.WriteLine("┌─────────────────────┬──────────┬────────────────────────┐");
        Console.WriteLine("│ Задача              │ Подход   │ Сложность              │");
        Console.WriteLine("├─────────────────────┼──────────┼────────────────────────┤");
        Console.WriteLine("│ Расстояния          │ Centroid │ O(log V) на запрос     │");
        Console.WriteLine("│ LCA и пути          │ HLD      │ O(log V) на запрос     │");
        Console.WriteLine("│ Пути + обновления   │ HLD+ST   │ O(log² V) на запрос    │");
        Console.WriteLine("│ Динамические рёбра  │ LCT      │ O(log V) амортиз.      │");
        Console.WriteLine("└─────────────────────┴──────────┴────────────────────────┘");
        Console.WriteLine();
        Console.WriteLine("Рекомендации:");
        Console.WriteLine("  • Статическое дерево → HLD или Centroid");
        Console.WriteLine("  • Нужны обновления весов → HLD + Segment Tree");
        Console.WriteLine("  • Рёбра меняются → Link-Cut Tree");
        Console.WriteLine("  • Только LCA → обычный LCA (бинарный подъём)");
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
