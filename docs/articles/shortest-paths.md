---
uid: articles.shortest-paths
title: Кратчайшие пути
---

# Кратчайшие пути

Библиотека реализует четыре классических алгоритма поиска кратчайших путей.

## Сравнение

| Алгоритм — что решает | Время | Отриц. веса | Все пары | Источник |
|---|---|---|---|---|
| Дейкстра — от одного источника | O((V+E) log V) | ❌ | ❌ | Один |
| Беллман-Форд — с отрицательными весами | O(V · E) | ✅ | ❌ | Один |
| A* — эвристический поиск | Зависит от эвристики | ❌ | ❌ | Один |
| Флойд-Уоршелл — все пары | O(V³) | ✅ | ✅ | — |

**Обозначения:**
- **Отриц. веса** — поддерживает ли алгоритм отрицательные веса рёбер
- **Все пары** — вычисляет ли сразу расстояния между всеми парами
- **Источник** — нужен ли конкретный начальный узел

### Быстрый выбор

```
Отрицательные веса?
├─ Да → Флойд-Уоршелл (все пары)
│       или Беллман-Форд (один источник)
└─ Нет → Нужны все пары?
        ├─ Да → Флойд-Уоршелл
        └─ Нет → Есть эвристика?
                ├─ Да → A*
                └─ Нет → Дейкстра
```

---

## Дейкстра

Классический алгоритм для графов с **неотрицательными** весами.

```csharp
using GraphToolkit.ShortestPaths;
using GraphToolkit.Utils;

var graph = new GraphBuilder<string>(isDirected: true)
    .AddEdge("A", "B", 4)
    .AddEdge("A", "C", 2)
    .AddEdge("B", "C", 5)
    .AddEdge("B", "D", 10)
    .AddEdge("C", "E", 3)
    .AddEdge("E", "D", 4)
    .Build();

var path = Dijkstra.FindPath(graph, "A", "D");
Console.WriteLine(string.Join(" -> ", path));
// A -> C -> E -> D

var (distances, predecessors) = Dijkstra.Compute(graph, "A");
foreach (var (v, d) in distances)
    Console.WriteLine($"{v}: {d}");
```

**Реализация:** использует `PriorityQueue<TElement, TPriority>` (двоичная
куча) — O((V + E) log V).

---

## Беллман-Форд

Поддерживает **отрицательные веса** и **детектирует отрицательные циклы**.

```csharp
var graph = new GraphBuilder<string>(isDirected: true)
    .AddEdge("A", "B", 4)
    .AddEdge("A", "C", 2)
    .AddEdge("B", "C", -3)  // отрицательный вес
    .AddEdge("C", "D", 1)
    .Build();

var (dist, prev, hasCycle) = BellmanFord.Compute(graph, "A");

if (hasCycle)
    Console.WriteLine("Обнаружен отрицательный цикл!");
else
    foreach (var (v, d) in dist)
        Console.WriteLine($"{v}: {d}");
```

> [!WARNING]
> Если обнаружен отрицательный цикл, достижимый из источника,
> кратчайшие расстояния не определены.

---

## A*

Эвристический поиск — как Дейкстра, но с оценкой расстояния до цели.

```csharp
using GraphToolkit.ShortestPaths;

var coords = new Dictionary<string, (double X, double Y)>
{
    ["A"] = (0, 0),
    ["B"] = (1, 1),
    ["C"] = (2, 0),
    ["D"] = (3, 1)
};

double Heuristic(string a, string b)
{
    var (ax, ay) = coords[a];
    var (bx, by) = coords[b];
    return Math.Sqrt((ax - bx) * (ax - bx) + (ay - by) * (ay - by));
}

var path = AStar.FindPath(graph, "A", "D", Heuristic);
```

> [!IMPORTANT]
> Эвристика должна быть допустимой (admissible): не переоценивать
> реальное расстояние до цели.

---

## Флойд-Уоршелл

Все пары кратчайших путей сразу.

```csharp
var (dist, next, vertices) = FloydWarshall.Compute(graph);

var path = FloydWarshall.FindPath(graph, "A", "D", out double distance);
Console.WriteLine($"{string.Join(" -> ", path)} ({distance})");
// A -> C -> E -> D (9)
```

**Когда использовать:**
- Нужны расстояния между всеми парами вершин
- Граф плотный (E ≈ V²)
- Есть отрицательные веса (но без отрицательных циклов)
- V невелико (< 500)

---

## Практические рекомендации

### Выбор алгоритма

```
Отрицательные веса?
├─ Да → Флойд-Уоршелл (все пары) или Беллман-Форд (один источник)
└─ Нет → Один источник?
         ├─ Да → Есть эвристика? → A* / Дейкстра
         └─ Нет → Флойд-Уоршелл
```

### Работа с недостижимыми вершинами

```csharp
var (dist, _) = Dijkstra.Compute(graph, "A");
if (double.IsPositiveInfinity(dist["Z"]))
    Console.WriteLine("Вершина Z недостижима");
```

## См. также

- [Топологическая сортировка](topological-sort.md)
- [MST](minimum-spanning-tree.md)
- [Производительность](performance.md)