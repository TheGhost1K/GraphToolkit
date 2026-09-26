---
uid: articles.io-and-conversion
title: Преобразования графа
---

# Преобразования графа

Библиотека предоставляет готовые методы для изменения представления
графа: копирование, смена направленности, транспонирование.

## Клонирование

`Clone()` создаёт полную независимую копию:

```csharp
var original = new GraphBuilder<string>(isDirected: true)
    .AddEdge("A", "B", 4)
    .AddEdge("B", "C", 2)
    .Build();

var copy = original.Clone();
copy.AddEdge("X", "Y");   // оригинал не меняется

Console.WriteLine(original.VertexCount);   // 3
Console.WriteLine(copy.VertexCount);        // 5
```

**Сложность:** O(V + E). **Память:** O(V + E).

## Смена направленности

### `ToUndirected()` — из ориентированного в неориентированный

Каждое направленное ребро `u → v` становится неориентированным `u — v`.

```csharp
var directed = new Graph<string>(isDirected: true);
directed.AddEdge("A", "B", 1);
directed.AddEdge("B", "A", 5);   // обратное ребро

var undirected = directed.ToUndirected();
Console.WriteLine(undirected.EdgeCount);   // 1 (дедуплицировано)
```

**Особенности:**
- Дублирующиеся рёбра `u → v` и `v → u` схлопываются в одно `u — v`
- Вес берётся из **первого** добавленного ребра
- Если граф уже неориентированный — эквивалентно `Clone()`

### `ToDirected()` — из неориентированного в ориентированный

Каждое ребро `u — v` превращается в два: `u → v` и `v → u`.

```csharp
var undirected = new Graph<string>();
undirected.AddEdge("A", "B", 3);

var directed = undirected.ToDirected();
Console.WriteLine(directed.EdgeCount);   // 2
Console.WriteLine(directed.Neighbors("A").Count());   // 1 (A→B)
Console.WriteLine(directed.Neighbors("B").Count());   // 1 (B→A)
```

## Транспонирование

`Transpose()` меняет направление **всех** рёбер:

```csharp
var g = new Graph<string>(isDirected: true);
g.AddEdge("A", "B", 5);

var t = g.Transpose();
Console.WriteLine(t.Neighbors("A").Count());   // 0
Console.WriteLine(t.Neighbors("B").Count());   // 1, B → A с весом 5
```

Используется в алгоритме Косараю для поиска SCC.

## Сравнение методов

| Метод | Направленность | Число рёбер | Назначение |
|---|---|---|---|
| `Clone()` | та же | то же | независимая копия |
| `Transpose()` | та же | то же | обратные рёбра |
| `ToUndirected()` | → неориентированный | ≤ исходного | симплификация |
| `ToDirected()` | → ориентированный | ×2 от исходного | разворот направлений |

## См. также

- [Ключевые концепции](core-concepts.md)
- [Матрица смежности](adjacency-matrix.md)