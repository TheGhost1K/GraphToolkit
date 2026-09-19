---
uid: articles.core-concepts
title: Ключевые концепции
---

# Ключевые концепции

## Тип вершины `T`

Все графы и алгоритмы параметризованы типом `T`:

```csharp
var g1 = new Graph<string>();   // вершины — строки
var g2 = new Graph<int>();      // вершины — целые
var g3 = new Graph<City>();     // вершины — пользовательский тип
```

Ограничение `where T : notnull` защищает от null-вершин.

> [!IMPORTANT]
> Тип `T` должен корректно реализовывать `Equals` и `GetHashCode`.
> Для пользовательских типов используйте `record` или переопределяйте оба метода.

## Ориентированность

```csharp
var directed = new Graph<string>(isDirected: true);
directed.AddEdge("A", "B");    // A → B (обратного ребра нет)

var undirected = new Graph<string>(isDirected: false);
undirected.AddEdge("A", "B");  // A — B (эквивалентно B — A)
```

## Веса рёбер

```csharp
graph.AddEdge("A", "B");           // вес по умолчанию = 1.0
graph.AddEdge("A", "B", 4.5);      // явный вес
graph.AddEdge("A", "B", -2);       // допустимо; не все алгоритмы поддерживают
```

> [!WARNING]
> Дейкстра и A* требуют неотрицательных весов. Для отрицательных
> используйте [Беллмана-Форда](shortest-paths.md).

## Интерфейс `IGraph<T>`

```csharp
public interface IGraph<T>
{
    bool IsDirected { get; }
    IEnumerable<T> Vertices { get; }
    IReadOnlyList<Edge<T>> Edges { get; }
    int VertexCount { get; }
    int EdgeCount { get; }
    IEnumerable<Edge<T>> Neighbors(T vertex);
}
```

Все алгоритмы принимают `IGraph<T>`, что делает их независимыми
от конкретной реализации. Вы можете реализовать свой граф
(например, на матрице смежности) и использовать все алгоритмы без изменений.

## Специализированные типы

### `Edge<T>`

Ребро с полями `From`, `To`, `Weight`. Реализует `IComparable`, `IEquatable`.

### `FlowNetwork<T>`

Сеть для задач максимального потока. **Модифицируется** при работе
алгоритмов — передавайте копию, если нужен исходный граф.

### `CostFlowNetwork<T>`

Сеть с пропускной способностью и стоимостью для min-cost flow.

### `UnionFind<T>`

Система непересекающихся множеств с ранговой эвристикой и сжатием путей.

## Рекомендации

1. **Вершины-значения**: используйте `int`, `string`, `Guid` или `record`.
2. **Один граф — один экземпляр**: `Graph<T>` не потокобезопасен.
3. **Не мутируйте во время обхода**.
4. **Проверяйте `null`**: методы поиска пути возвращают `null`, если путь не существует.

## См. также

- [Быстрый старт](getting-started.md)
- [Обходы графа](traversal.md)
- [Производительность](performance.md)