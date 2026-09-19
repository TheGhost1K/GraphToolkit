---
uid: articles.topological-sort
title: Топологическая сортировка
---

# Топологическая сортировка

**Алгоритм:** Кана (BFS по входящим степеням)
**Сложность:** O(V + E)
**Применимо к:** DAG (Directed Acyclic Graph)

## Что это

Линейный порядок вершин, при котором для каждого ребра `u → v`
вершина `u` идёт **до** `v`.

## Пример

```csharp
using GraphToolkit.Traversal;
using GraphToolkit.Utils;

var dag = new GraphBuilder<string>(isDirected: true)
    .AddEdge("рубашка", "галстук")
    .AddEdge("галстук", "пиджак")
    .AddEdge("носки", "ботинки")
    .AddEdge("брюки", "ботинки")
    .AddEdge("брюки", "пиджак")
    .Build();

var order = TopologicalSort.Sort(dag);
Console.WriteLine(string.Join(" -> ", order));
```

## Обнаружение циклов

Метод возвращает `null`, если граф содержит цикл:

```csharp
var cyclic = new GraphBuilder<int>(isDirected: true)
    .AddEdge(1, 2)
    .AddEdge(2, 3)
    .AddEdge(3, 1)  // цикл!
    .Build();

var order = TopologicalSort.Sort(cyclic);
if (order == null)
    Console.WriteLine("Граф содержит цикл");
```

## Топологические уровни

```csharp
var levels = TopologicalSort.ComputeLevels(dag);
foreach (var (v, level) in levels)
    Console.WriteLine($"{v}: уровень {level}");
```

## Применения

- Планирование задач с зависимостями
- Порядок сборки модулей / пакетов
- Учебный план с пререквизитами
- Компиляция исходников (Makefile, MSBuild)

## См. также

- [Обходы графа](traversal.md)
- [Кратчайшие пути](shortest-paths.md)
- [Транзитивное замыкание](closure.md)