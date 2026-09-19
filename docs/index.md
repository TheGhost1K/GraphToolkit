---
uid: home
title: GraphToolkit
---

# GraphToolkit

> Комплексная библиотека графовых алгоритмов для .NET 10+

## Основные возможности

| Категория | Алгоритмы | Сложность |
|-----------|-----------|-----------|
| **Обходы** | BFS, DFS, топологическая сортировка | O(V+E) |
| **Кратчайшие пути** | Дейкстра, Беллман-Форд, A*, Флойд-Уоршелл | O((V+E)logV) — O(V³) |
| **MST** | Краскал, Прим, Борувка | O(E log E) |
| **Компоненты** | Связные, SCC, мосты, точки сочленения | O(V+E) |
| **Потоки** | Форд-Фалкерсон, Эдмондс-Карп, Диниц | O(V²E) |
| **Min-cost flow** | SPFA, Дейкстра с потенциалами | O(F·E·logV) |
| **Разрезы** | Штёр-Вагнер | O(V³) |
| **Паросочетания** | Куна, Blossom | O(VE) — O(V³) |
| **Эйлеровы** | Проверка + Хиерхольцер | O(V+E) |
| **TSP** | Ветви и границы, ближайший сосед + 2-opt | O(2ⁿV²) / O(V²) |
| **Раскраска** | Жадная, DSATUR | O(V²+E) — экспон. |
| **Деревья** | LCA, диаметр, центроид | O(V log V) |
| **Замыкания** | Транзитивное замыкание и сокращение | O(V³) |
| **Визуализация** | DOT, Mermaid, матрица/список смежности | — |

## Быстрый старт

```csharp
using GraphToolkit.Utils;
using GraphToolkit.ShortestPaths;

var graph = new GraphBuilder<string>(isDirected: true)
    .AddEdge("A", "B", 4)
    .AddEdge("A", "C", 2)
    .AddEdge("C", "D", 3)
    .Build();

var path = Dijkstra.FindPath(graph, "A", "D");
Console.WriteLine(string.Join(" -> ", path)); // A -> C -> D
```

## Установка

```bash
dotnet add package GraphToolkit
```

## Разделы документации

- [Установка](articles/installation.md)
- [Быстрый старт](articles/getting-started.md)
- [Ключевые концепции](articles/core-concepts.md)

## Лицензия

Проект распространяется под лицензией **MIT**.