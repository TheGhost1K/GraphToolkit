---
uid: articles.eulerian
title: Эйлеровы пути
---

# Эйлеровы пути и циклы

**Эйлеров путь** — путь, проходящий по каждому ребру ровно один раз.
**Эйлеров цикл** — эйлеров путь, начинающийся и заканчивающийся в одной вершине.

## Условия существования

| Тип графа | Эйлеров цикл | Эйлеров путь |
|---|---|---|
| Неориентированный | Все степени чётные | Ровно 0 или 2 вершины нечётной степени |
| Ориентированный | in-degree = out-degree для всех | Одна вершина с out-in=1, одна с in-out=1 |

## Проверка

```csharp
using GraphToolkit.Eulerian;

var graph = new GraphBuilder<string>(isDirected: false)
    .AddEdge("A", "B")
    .AddEdge("B", "C")
    .AddEdge("C", "D")
    .AddEdge("D", "A")
    .AddEdge("A", "C")
    .Build();

bool hasCycle = EulerianPath.HasCycle(graph);
Console.WriteLine($"Эйлеров цикл: {hasCycle}");

bool hasPath = EulerianPath.HasPath(graph, out var start, out var end);
Console.WriteLine($"Эйлеров путь: {hasPath} ({start} → {end})");
```

## Построение пути (Хиерхольцер)

**Сложность:** O(V + E).

```csharp
var path = EulerianPath.Find(graph);
Console.WriteLine(string.Join(" -> ", path!));
// A -> C -> D -> A -> B (зависит от порядка рёбер)
```

## Применения

- Задача о кёнигсбергских мостах
- Почтальон (обход всех дорог один раз)
- Сборка генома (Эйлеровы пути в графах де Брёйна)
- Проверка схем на наличие замкнутых маршрутов

## См. также

- [Гамильтоновы циклы](hamiltonian-tsp.md) — «брат» задачи, но NP-полный
- [Обходы графа](traversal.md)