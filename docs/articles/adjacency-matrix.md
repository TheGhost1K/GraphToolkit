---
uid: articles.adjacency-matrix
title: Матрица смежности
---

# Матрица смежности (`AdjacencyMatrixGraph<T>`)

Альтернативная реализация `IGraph<T>` — на матрице смежности.
Подходит для **плотных** графов, где число рёбер близко к V².

## Создание

```csharp
using GraphToolkit.Core;

var g = new AdjacencyMatrixGraph<string>(isDirected: false);
g.AddEdge("A", "B", 4);
g.AddEdge("B", "C", 2);
g.AddEdge("A", "C", 5);
```

## Преимущества

- **O(1)** проверка наличия ребра: `HasEdge(u, v)`
- **O(1)** получение веса: `GetWeight(u, v)`
- Компактное представление для плотных графов

## Недостатки

- Память O(V²), независимо от числа рёбер
- Обход соседей всегда O(V), даже если их мало
- Плохо работает для V > 10⁴

## Сравнение с `Graph<T>`

| Операция | `Graph<T>` | `AdjacencyMatrixGraph<T>` |
|---|---|---|
| `AddEdge` | O(1) | O(1) |
| `HasEdge` | O(deg(u)) | **O(1)** |
| `GetWeight` | O(deg(u)) | **O(1)** |
| `Neighbors(u)` | O(deg(u)) | O(V) |
| Память | O(V + E) | O(V²) |

**Выбор:**
- **Разреженный граф** (E ≈ V) → `Graph<T>`
- **Плотный граф** (E ≈ V²) → `AdjacencyMatrixGraph<T>`

## Совместимость с алгоритмами

Все алгоритмы GraphToolkit принимают `IGraph<T>` — обе реализации
работают без изменений:

```csharp
using GraphToolkit.ShortestPaths;

var g = new AdjacencyMatrixGraph<string>(isDirected: true);
g.AddEdge("A", "B", 4);
g.AddEdge("A", "C", 2);
g.AddEdge("C", "B", 1);

var path = Dijkstra.FindPath(g, "A", "B");
// A -> C -> B
```

## Автоматическое расширение

Матрица растёт автоматически при добавлении новых вершин:

```csharp
var g = new AdjacencyMatrixGraph<int>(initialCapacity: 4);

for (int i = 0; i < 100; i++)
    g.AddEdge(i, i + 1);

Console.WriteLine(g.VertexCount);   // 101
```

Начальная ёмкость — рекомендация, а не ограничение.

## Когда использовать

- ✅ Плотные графы (≥ 50% рёбер)
- ✅ Частые проверки `HasEdge` / `GetWeight`
- ✅ Фиксированное число вершин (V ≤ 5000)
- ❌ Разреженные графы
- ❌ Большие графы (V > 10⁴)
- ❌ Динамически меняющаяся структура

## См. также

- [Ключевые концепции](core-concepts.md)
- [Преобразования графа](io-and-conversion.md)