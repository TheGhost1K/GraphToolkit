---
uid: articles.hamiltonian-tsp
title: Гамильтоновы циклы и TSP
---

# Гамильтоновы циклы и задача коммивояжёра

**Гамильтонов цикл** — цикл, проходящий через каждую вершину ровно один раз.
Задача NP-полная.

## Поиск гамильтонова цикла

Backtracking. Применимо для V ≲ 15–20.

```csharp
using GraphToolkit.Hamiltonian;

var graph = new GraphBuilder<int>(isDirected: false)
    .AddEdge(0, 1)
    .AddEdge(1, 2)
    .AddEdge(2, 3)
    .AddEdge(3, 0)
    .AddEdge(0, 2)
    .Build();

var cycle = HamiltonianCycle.FindCycle(graph);
Console.WriteLine(cycle != null
    ? string.Join(" -> ", cycle)
    : "Цикл не найден");
// 0 -> 1 -> 2 -> 3 -> 0
```

## TSP: точное решение

Метод ветвей и границ. Оптимальный результат, но экспоненциальная сложность.

```csharp
using GraphToolkit.Hamiltonian;

var tsp = new GraphBuilder<string>(isDirected: false)
    .AddEdge("A", "B", 10)
    .AddEdge("A", "C", 15)
    .AddEdge("A", "D", 20)
    .AddEdge("B", "C", 35)
    .AddEdge("B", "D", 25)
    .AddEdge("C", "D", 30)
    .Build();

var result = Tsp.BranchAndBound(tsp, "A");
Console.WriteLine($"Точный: {string.Join(" -> ", result.Path)} = {result.Cost}");
// Точный: A -> B -> D -> C -> A = 80
```

## TSP: приближённое решение

Ближайший сосед + улучшение 2-opt. O(V²). Обычно 5–25% от оптимума.

```csharp
var approx = Tsp.NearestNeighbor(tsp, "A");
Console.WriteLine($"Приближённое: {string.Join(" -> ", approx.Path)} = {approx.Cost}");
// Приближённое: A -> B -> D -> C -> A = 80
```

## Сравнение подходов

| Подход | Сложность | Оптимальность |
|---|---|---|
| Ветви и границы | O(V² · 2^V) | Точный |
| Ближайший сосед + 2-opt | O(V²) | ~5–25% от оптимума |

## Практические ограничения

- **Ветви и границы**: до ~20 вершин
- **Ближайший сосед**: до ~10⁵ вершин (с 2-opt)
- Для больших задач используйте метаэвристики (генетические, ACO)

## См. также

- [Эйлеровы пути](eulerian.md)
- [Кратчайшие пути](shortest-paths.md)