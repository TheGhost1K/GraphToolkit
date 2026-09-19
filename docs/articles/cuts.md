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

## Применения

- Надёжность сетей (наиболее уязвимое место)
- Сообщества в графах (разбиение на кластеры)
- Обработка изображений (сегментация)

## См. также

- [Максимальный поток](flows.md)
- [Компоненты связности](components.md)