using GraphToolkit.Components;
using GraphToolkit.Traversal;
using GraphToolkit.Utils;

namespace SocialNetwork;

/// <summary>
/// Пример: анализ социального графа.
/// Демонстрирует поиск компонент связности, сильно связных компонент,
/// точек сочленения и мостов.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== Анализ социального графа ===\n");

        // Граф подписок: ребро u → v означает «u подписан на v».
        var follows = new GraphBuilder<string>(isDirected: true)
            .AddEdge("Алиса", "Борис")
            .AddEdge("Борис", "Алиса")   // взаимная подписка
            .AddEdge("Борис", "Вика")
            .AddEdge("Вика", "Борис")
            .AddEdge("Вика", "Гриша")
            .AddEdge("Гриша", "Вика")
            .AddEdge("Даша", "Егор")
            .AddEdge("Егор", "Даша")
            .AddEdge("Егор", "Жанна")
            .AddEdge("Жанна", "Егор")
            .Build();

        Console.WriteLine($"Пользователей: {follows.VertexCount}, подписок: {follows.EdgeCount}\n");

        // --- 1. Слабые компоненты (игнорируя направление) ---
        var weak = ConnectedComponents.Find(follows);
        Console.WriteLine($"[Связность] Найдено {weak.Count} групп пользователей:");
        foreach (var comp in weak)
            Console.WriteLine($"  {{{string.Join(", ", comp)}}}");
        Console.WriteLine();

        // --- 2. Сильные компоненты (взаимные подписки) ---
        var sccs = StronglyConnectedComponents.Find(follows);
        Console.WriteLine($"[SCC] Найдено {sccs.Count} сильно связных групп:");
        foreach (var scc in sccs)
            Console.WriteLine($"  {{{string.Join(", ", scc)}}}");
        Console.WriteLine();

        // --- 3. Мосты и точки сочленения ---
        // Преобразуем в неориентированный граф для анализа связности.
        var undirected = new GraphBuilder<string>(isDirected: false)
            .AddEdge("Алиса", "Борис")
            .AddEdge("Борис", "Вика")
            .AddEdge("Вика", "Гриша")
            .AddEdge("Даша", "Егор")
            .AddEdge("Егор", "Жанна")
            .AddEdge("Гриша", "Даша")   // соединяем две группы
            .Build();

        var analysis = BridgesAndArticulation.Find(undirected);

        Console.WriteLine("[Мосты] Критические связи (удаление разрывает сеть):");
        if (analysis.Bridges.Count == 0)
            Console.WriteLine("  нет");
        else
            foreach (var (u, v) in analysis.Bridges)
                Console.WriteLine($"  {u} — {v}");
        Console.WriteLine();

        Console.WriteLine("[Точки сочленения] Ключевые пользователи:");
        if (analysis.ArticulationPoints.Count == 0)
            Console.WriteLine("  нет");
        else
            foreach (var user in analysis.ArticulationPoints)
                Console.WriteLine($"  {user}");
        Console.WriteLine();

        // --- 4. Достижимость: кто подписан на кого транзитивно ---
        Console.WriteLine("[Достижимость] Кто кому «доступен» транзитивно:");
        var closure = GraphToolkit.Closure.TransitiveClosure.ComputeDict(follows);
        foreach (var (user, reachable) in closure.OrderBy(kv => kv.Key))
        {
            var others = reachable.Where(r => r != user).ToList();
            if (others.Count > 0)
                Console.WriteLine($"  {user,-8} → {string.Join(", ", others)}");
        }
        Console.WriteLine();

        // --- 5. Топологическая сортировка невозможна (есть циклы) ---
        var topo = TopologicalSort.Sort(follows);
        if (topo == null)
            Console.WriteLine("[Топосорт] Граф содержит циклы (взаимные подписки) — сортировка невозможна");
        else
            Console.WriteLine("[Топосорт] " + string.Join(" → ", topo));
    }
}