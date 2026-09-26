---
uid: articles.cuts
title: Минимальный разрез
---

# Глобальный минимальный разрез

**Алгоритм:** Штёра-Вагнера.
**Сложность:** O(V³).
**Применимо к:** неориентированный взвешенный граф.

Не требует указания источника и стока — сам находит лучший разрез.

## Пример

```csharp
using GraphToolkit.Cut;
using GraphToolkit.Utils;

var graph = new GraphBuilder<string>(isDirected: false)
    .AddEdge("A", "B", 2)
    .AddEdge("A", "C", 3)
    .AddEdge("B", "C", 1)
    .AddEdge("B", "D", 4)
    .AddEdge("C", "D", 5)
    .AddEdge("D", "E", 6)
    .Build();

var result = StoerWagner.Compute(graph);

Console.WriteLine($"Вес разреза: {result.MinCut}");
Console.WriteLine($"Сторона A: {string.Join(",", result.PartitionA)}");
Console.WriteLine($"Сторона B: {string.Join(",", result.PartitionB)}");
```

Пример вывода:

```
Вес разреза: 6
Сторона A: E
Сторона B: A,B,C,D
```

---

## Отличие от `Dinic.MinCut`

| | `Dinic.MinCut` | `StoerWagner.Compute` |
|---|---|---|
| Тип графа | Ориентированный | Неориентированный |
| Источник/сток | Задаются | Не нужны |
| Сложность | O(V²E) | O(V³) |

## Все пары минимальных разрезов (Гомори-Ху)

Алгоритм **Гомори-Ху** строит дерево на V вершинах, в котором
минимальный разрез между любыми двумя вершинами равен минимальному
весу ребра на пути между ними. Это позволяет ответить на любой
запрос `MinCut(u, v)` за O(V) без дополнительных запусков max-flow.

```csharp
using GraphToolkit.Cut;

var g = new GraphBuilder<string>(isDirected: false)
    .AddEdge("A", "B", 1)
    .AddEdge("B", "C", 1)
    .AddEdge("A", "C", 3)
    .Build();

var tree = GomoryHu.Compute(g);

Console.WriteLine(tree.MinCut("A", "C"));   // 1
Console.WriteLine(tree.MinCut("A", "B"));   // 2
```

### Сравнение с другими алгоритмами

| | `Dinic.MinCut` | `StoerWagner.Compute` | `GomoryHu.Compute` |
|---|---|---|---|
| Тип графа | Ориентированный | Неориентированный | Неориентированный |
| Источник/сток | Задаются | Не нужны | Не нужны |
| Результат | Один min-cut | Один глобальный | Все пары |
| Сложность | O(V² · E) | O(V³) | O(V · MaxFlow) |

## Применения

- Анализ надёжности сетей (все пары «узких мест»)
- Сообщества в графах (разбиение на кластеры)
- Обработка изображений (сегментация)
- Задачи, где нужны min-cut между многими парами вершин

## См. также

- [Максимальный поток](flows.md)
- [Компоненты связности](components.md)