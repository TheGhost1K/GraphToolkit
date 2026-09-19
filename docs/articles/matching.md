---
uid: articles.matching
title: Паросочетания
---

# Паросочетания

**Паросочетание** — множество рёбер без общих вершин.

## Куна — двудольный граф

**Сложность:** O(V · E).
**Применимо к:** двудольный граф.

```csharp
using GraphToolkit.Matching;

var left = new[] { "A", "B", "C" };
var right = new[] { "X", "Y", "Z" };

var neighbors = new Dictionary<string, string[]>
{
    ["A"] = new[] { "X", "Y" },
    ["B"] = new[] { "X" },
    ["C"] = new[] { "Y", "Z" }
};

var matching = Kuhn.Compute(left, u => neighbors[u]);

foreach (var (u, v) in matching)
    Console.WriteLine($"{u} ↔ {v}");
// A ↔ Y
// B ↔ X
// C ↔ Z
```

### Когда использовать

- Задача о назначениях
- Сопоставление студентов и курсов
- Рекомендательные системы

---

## Blossom (Эдмондс) — общий граф

**Сложность:** O(V³).
**Применимо к:** произвольный неориентированный граф.

```csharp
using GraphToolkit.Matching;
using GraphToolkit.Utils;

var graph = new GraphBuilder<int>(isDirected: false)
    .AddEdge(0, 1)
    .AddEdge(1, 2)
    .AddEdge(2, 0)  // нечётный цикл (граф не двудольный!)
    .AddEdge(2, 3)
    .Build();

var matching = Blossom.Compute(graph);

foreach (var (u, v) in matching)
    Console.WriteLine($"{u} ↔ {v}");
```

### Проверка совершенного паросочетания

```csharp
if (Blossom.HasPerfectMatching(graph))
    Console.WriteLine("Существует паросочетание, покрывающее все вершины");
```

---

## Сравнение

| | Куна | Blossom |
|---|---|---|
| Тип графа | Двудольный | Общий |
| Сложность | O(V·E) | O(V³) |
| Простота | Высокая | Низкая |

## См. также

- [Максимальный поток](flows.md) — альтернативный подход для двудольных
- [Раскраска](coloring.md) — проверка двудольности