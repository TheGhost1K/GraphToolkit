---
uid: articles.closure
title: Транзитивное замыкание
---

# Транзитивное замыкание

**Транзитивное замыкание** — граф, в котором есть ребро `u → v`,
если в исходном графе существует путь из `u` в `v`.

## Вычисление

**Алгоритм:** Флойда-Уоршелла в булевом варианте.
**Сложность:** O(V³).

```csharp
using GraphToolkit.Closure;
using GraphToolkit.Utils;

var graph = new GraphBuilder<int>(isDirected: true)
    .AddEdge(0, 1)
    .AddEdge(1, 2)
    .AddEdge(2, 3)
    .Build();

var closure = TransitiveClosure.ComputeDict(graph);

foreach (var (v, reachable) in closure)
    Console.WriteLine($"{v} → {{{string.Join(",", reachable)}}}");
// 0 → {0,1,2,3}
// 1 → {1,2,3}
// 2 → {2,3}
// 3 → {3}
```

## Матрица достижимости

```csharp
bool[,] reach = TransitiveClosure.Compute(graph);
int n = graph.VertexCount;

for (int i = 0; i < n; i++)
{
    for (int j = 0; j < n; j++)
        Console.Write(reach[i, j] ? "1 " : "0 ");
    Console.WriteLine();
}
```

## Транзитивное сокращение

Минимальный DAG, дающий то же замыкание.

```csharp
var dag = new GraphBuilder<int>(isDirected: true)
    .AddEdge(0, 1)
    .AddEdge(1, 2)
    .AddEdge(0, 2)  // избыточное: 0 → 1 → 2
    .Build();

var reduced = TransitiveClosure.Reduce(dag);
foreach (var e in reduced.Edges)
    Console.WriteLine(e);
// 0 -> 1
// 1 -> 2
```

> [!IMPORTANT]
> Транзитивное сокращение работает **только для DAG**.

## Применения

- Анализ зависимостей (что от чего зависит транзитивно)
- Проверка достижимости в сетях Петри
- Оптимизация графа (удаление избыточных рёбер)

## См. также

- [Топологическая сортировка](topological-sort.md)
- [Компоненты связности](components.md)