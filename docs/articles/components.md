---
uid: articles.components
title: Компоненты связности
---

# Компоненты связности

## Связные компоненты (неориентированный граф)

```csharp
using GraphToolkit.Components;

var components = ConnectedComponents.Find(graph);
Console.WriteLine($"Найдено компонент: {components.Count}");

foreach (var comp in components)
    Console.WriteLine($"  {{{string.Join(", ", comp)}}}");
```

**Сложность:** O(V + E).

---

## Компоненты сильной связности (SCC)

SCC — максимальное множество вершин, где каждая достижима из каждой.

**Алгоритм:** Косараю (два DFS + транспонирование).
**Сложность:** O(V + E).

```csharp
var sccs = StronglyConnectedComponents.Find(directedGraph);

foreach (var scc in sccs)
    Console.WriteLine("SCC: " + string.Join(", ", scc));
```

### Пример

```csharp
var dg = new GraphBuilder<int>(isDirected: true)
    .AddEdge(0, 1)
    .AddEdge(1, 2)
    .AddEdge(2, 0)  // цикл 0-1-2
    .AddEdge(2, 3)
    .AddEdge(3, 4)
    .AddEdge(4, 5)
    .AddEdge(5, 3)  // цикл 3-4-5
    .Build();

var sccs = StronglyConnectedComponents.Find(dg);
// SCC: {0, 2, 1}
// SCC: {3, 5, 4}
```

### Применения

- Компиляторы (анализ зависимостей модулей)
- Веб-анализ (сообщества в графе ссылок)
- Сжатие графа (замена SCC на одну вершину)

---

## Мосты и точки сочленения

**Алгоритм:** Тарьян.
**Сложность:** O(V + E).

- **Мост** — ребро, удаление которого увеличивает число компонент
- **Точка сочленения** — вершина с тем же свойством

```csharp
var result = BridgesAndArticulation.Find(graph);

Console.WriteLine("Мосты:");
foreach (var (u, v) in result.Bridges)
    Console.WriteLine($"  {u} — {v}");

Console.WriteLine("Точки сочленения:");
foreach (var v in result.ArticulationPoints)
    Console.WriteLine($"  {v}");
```

### Пример

```csharp
var g = new GraphBuilder<string>(isDirected: false)
    .AddEdge("A", "B")
    .AddEdge("B", "C")
    .AddEdge("C", "A")   // треугольник (нет мостов)
    .AddEdge("C", "D")   // мост
    .AddEdge("D", "E")
    .AddEdge("D", "F")
    .Build();

var r = BridgesAndArticulation.Find(g);
// Мосты: C — D
// Точки сочленения: C, D
```

### Применения

- Надёжность сетей (критические связи)
- Дорожные сети (мосты — узкие места)
- Социальные сети

## См. также

- [Обходы графа](traversal.md)
- [MST](minimum-spanning-tree.md)