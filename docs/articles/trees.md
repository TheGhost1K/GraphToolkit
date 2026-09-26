---
uid: articles.trees
title: Алгоритмы на деревьях
---

# Алгоритмы на деревьях

## LCA — наименьший общий предок

**Предобработка:** O(V log V).
**Запрос:** O(log V).

```csharp
using GraphToolkit.Trees;
using GraphToolkit.Utils;

var tree = new GraphBuilder<string>(isDirected: false)
    .AddEdge("A", "B")
    .AddEdge("A", "C")
    .AddEdge("B", "D")
    .AddEdge("B", "E")
    .AddEdge("C", "F")
    .AddEdge("F", "G")
    .Build();

var lca = new Lca<string>(tree, root: "A");

Console.WriteLine(lca.Query("D", "G")); // A
Console.WriteLine(lca.Query("E", "F")); // A
Console.WriteLine(lca.Query("D", "E")); // B
```

**Применения:**
- Расстояние между вершинами в дереве
- Проверка предковости
- Запросы на путях в деревьях

---

## Диаметр дерева

Самый длинный путь во взвешенном дереве.

**Сложность:** O(V log V) через две Дейкстры.

```csharp
var diameter = TreeMetrics.Diameter(tree);
Console.WriteLine($"Диаметр: {diameter.Distance}");
Console.WriteLine($"Путь: {string.Join(" -> ", diameter.Path)}");
```

---

## Центроид дерева

Вершина, удаление которой минимизирует максимальный размер
оставшихся компонент.

**Сложность:** O(V).

```csharp
var centroid = TreeMetrics.Centroid(tree);
Console.WriteLine($"Центроид: {centroid}");
```

**Применения:**
- Centroid decomposition (быстрые запросы на путях)
- Балансировка деревьев

---

## Проверка: является ли граф деревом

```csharp
using GraphToolkit.Utils;

if (graph.IsTree())
    Console.WriteLine("Граф — дерево");
```

Условие: связный и содержит V − 1 рёбер.

## Дополнительные алгоритмы

Помимо LCA, диаметра и центроида, библиотека предоставляет:

- **Центроидная декомпозиция** — разложение дерева по центроидам за O(V log V)
- **Heavy-Light Decomposition** — разбиение на тяжёлые пути за O(V)
- **Link-Cut Tree** — динамический лес с операциями за O(log V)
- **HLD + запросы на путях** — обёртка HLD с деревом отрезков

Все они вынесены в отдельную статью [«Продвинутые алгоритмы на деревьях»](advanced-trees.md),
чтобы эта статья оставалась обзорной.

## См. также

- [Обходы графа](traversal.md)
- [MST](minimum-spanning-tree.md)