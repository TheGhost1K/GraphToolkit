---
uid: articles.minimum-spanning-tree
title: Минимальные остовные деревья
---

# Минимальные остовные деревья (MST)

MST — подграф связного неориентированного графа, который содержит
все вершины, является деревом (V − 1 рёбер) и имеет минимально
возможный суммарный вес.

## Реализованные алгоритмы

| Алгоритм | Сложность | Когда использовать |
|---|---|---|
| Краскал | O(E log E) | Разреженные графы |
| Прим | O((V+E) log V) | Плотные графы |
| Борувка | O(E log V) | Разреженные, параллелизуемые |

---

## Краскал

```csharp
using GraphToolkit.MinimumSpanningTree;
using GraphToolkit.Utils;

var graph = new GraphBuilder<string>(isDirected: false)
    .AddEdge("A", "B", 4)
    .AddEdge("A", "C", 3)
    .AddEdge("B", "C", 1)
    .AddEdge("B", "D", 2)
    .AddEdge("C", "D", 5)
    .AddEdge("D", "E", 7)
    .Build();

var mst = Kruskal.Compute(graph);
double total = 0;

foreach (var e in mst)
{
    Console.WriteLine(e);
    total += e.Weight;
}

Console.WriteLine($"Итого: {total}");
// B -> C (1)
// B -> D (2)
// A -> C (3)
// D -> E (7)
// Итого: 13
```

Использует `UnionFind<T>` для проверки циклов за O(α(n)) ≈ O(1).

---

## Прим

Растит дерево от стартовой вершины, добавляя минимальное исходящее ребро.

```csharp
var mst = Prim.Compute(graph, "A");
```

---

## Борувка

На каждой итерации каждая компонента добавляет минимальное исходящее ребро.

```csharp
var mst = Boruvka.Compute(graph);
```

Преимущества: хорошо распараллеливается, простая логика.

---

## Пример: кластеризация через MST

Удаление k − 1 самых тяжёлых рёбер из MST даёт k кластеров.

```csharp
using GraphToolkit.Core;
using GraphToolkit.Components;

List<List<T>> Clusterize<T>(IGraph<T> graph, int k) where T : notnull
{
    var mst = Kruskal.Compute(graph);
    var sorted = mst.OrderByDescending(e => e.Weight).ToList();
    var removed = sorted.Take(k - 1).ToHashSet();

    var clustered = new Graph<T>(isDirected: false);
    foreach (var v in graph.Vertices) clustered.AddVertex(v);
    foreach (var e in mst)
        if (!removed.Contains(e))
            clustered.AddEdge(e.From, e.To, e.Weight);

    return ConnectedComponents.Find(clustered);
}
```

---

## Проверка свойств

```csharp
// MST всегда содержит V - 1 рёбер
Debug.Assert(mst.Count == graph.VertexCount - 1);

// Суммарный вес минимален
double total = mst.Sum(e => e.Weight);
```

## См. также

- [Компоненты связности](components.md)
- [Кратчайшие пути](shortest-paths.md)
- [Производительность](performance.md)