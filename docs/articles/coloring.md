---
uid: articles.coloring
title: Раскраска графа
---

# Раскраска графа

**Правильная раскраска** — назначение цветов вершинам так, чтобы
смежные вершины имели разные цвета.

**Хроматическое число χ(G)** — минимальное число цветов.

## Жадная раскраска (Уэлш-Пауэлл)

Сортирует вершины по убыванию степени и присваивает минимальный
доступный цвет.

**Сложность:** O(V² + E).

```csharp
using GraphToolkit.Coloring;
using GraphToolkit.Utils;

var graph = new GraphBuilder<string>(isDirected: false)
    .AddEdge("A", "B")
    .AddEdge("A", "C")
    .AddEdge("B", "C")
    .AddEdge("B", "D")
    .AddEdge("C", "D")
    .AddEdge("D", "E")
    .Build();

var colors = GraphColoring.Greedy(graph);
foreach (var (v, c) in colors)
    Console.WriteLine($"{v}: цвет {c}");
// A: 0
// B: 1
// C: 2
// D: 0
// E: 1
```

## Точная раскраска (DSATUR)

Backtracking с эвристикой насыщенности. Даёт хроматическое число.

**Сложность:** экспоненциальная.

```csharp
var (colors, chi) = GraphColoring.Exact(graph);
Console.WriteLine($"Хроматическое число: {chi}");
// Хроматическое число: 3

foreach (var (v, c) in colors)
    Console.WriteLine($"  {v}: цвет {c}");
```

## Проверка двудольности

**Сложность:** O(V + E).

```csharp
if (GraphColoring.IsBipartite(graph, out var coloring))
{
    Console.WriteLine("Граф двудольный");
    foreach (var (v, c) in coloring)
        Console.WriteLine($"{v}: часть {c}");
}
else
{
    Console.WriteLine("Граф не двудольный");
}
```

## Применения

- Составление расписаний (экзамены, занятия)
- Распределение регистров в компиляторах
- Раскраска карт
- Распределение частот в сетях

## См. также

- [Паросочетания](matching.md)
- [Компоненты связности](components.md)