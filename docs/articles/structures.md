---
uid: articles.structures
title: Деревья отрезков и Fenwick
---

# Деревья отрезков и Fenwick

Для запросов на отрезках массива (минимум, максимум, сумма с
обновлениями) библиотека предоставляет две классические структуры
данных. Они также используются внутри `HldPathQueries` для запросов
на путях в дереве.

## Segment Tree — дерево отрезков

Поддерживает три операции:

- **`Query(l, r)`** — агрегат на отрезке `[l, r)` за O(log n)
- **`Update(i, value)`** — точечное обновление за O(log n)
- **`RangeUpdate(l, r, value)`** — обновление отрезка с lazy propagation

### Создание: сумма

```csharp
using GraphToolkit.Structures;

var sumTree = new SegmentTree<long>(
    values: new long[] { 1, 2, 3, 4, 5 },
    combine: (a, b) => a + b,
    identity: 0);

Console.WriteLine(sumTree.Query(0, 5));   // 15
Console.WriteLine(sumTree.Query(1, 4));   // 2 + 3 + 4 = 9

sumTree.Update(2, 10);                    // {1, 2, 10, 4, 5}
Console.WriteLine(sumTree.Query(0, 5));   // 22
```

### Минимум / максимум

```csharp
var minTree = new SegmentTree<int>(
    values: new[] { 5, 3, 7, 1, 4 },
    combine: Math.Min,
    identity: int.MaxValue);

Console.WriteLine(minTree.Query(0, 5));   // 1
Console.WriteLine(minTree.Query(0, 3));   // 3 (min(5,3,7))
```

### Range update с lazy propagation

Для операции «присвоить значение всем элементам отрезка» в min-дереве:

```csharp
var st = new SegmentTree<int>(
    values: new[] { 5, 3, 7, 1, 4 },
    combine: Math.Min,
    identity: int.MaxValue,
    applyLazy: (_, newValue) => newValue,     // присвоить
    composeLazy: (_, newer) => newer);        // новое перекрывает старое

st.RangeUpdate(1, 3, 10);                     // присвоить 10 позициям 1, 2
Console.WriteLine(st.Query(1, 3));            // 10
Console.WriteLine(st.Query(0, 5));            // 1 (позиция 3 всё ещё 1)
```

### Как работает lazy propagation

- **`applyLazy(nodeValue, lazy)`** — как применить отложенное значение
  к агрегату узла.
- **`composeLazy(oldLazy, newLazy)`** — как объединить два отложенных
  значения.

Пример для суммы с прибавлением константы:

```csharp
// Храним (sum, len) в узле, чтобы applyLazy знал, сколько прибавлять
var st = new SegmentTree<(long Sum, int Len)>(
    values: new[] { (1L, 1), (2L, 1), (3L, 1), (4L, 1) },
    combine: (a, b) => (a.Sum + b.Sum, a.Len + b.Len),
    identity: (0L, 0),
    applyLazy: (node, delta) => (node.Sum + delta.Sum * node.Len, node.Len),
    composeLazy: (a, b) => (a.Sum + b.Sum, 0));

st.RangeUpdate(1, 3, (10L, 0));   // +10 к позициям 1, 2
Console.WriteLine(st.Query(0, 4).Sum);   // 1 + 12 + 13 + 4 = 30
```

### Сложность

| Операция | Время | Память |
|---|---|---|
| Построение | O(n) | O(n) |
| Query | O(log n) | — |
| Update (точечное) | O(log n) | — |
| RangeUpdate | O(log n) | — |

---

## Fenwick Tree — дерево Фенвика

Проще и быстрее для операций **суммы** с точечными обновлениями.
Не поддерживает range update без модификаций и не работает с min/max.

### Создание

```csharp
using GraphToolkit.Structures;

var bit = new FenwickTree<long>(
    values: new long[] { 1, 2, 3, 4, 5 },
    add: (a, b) => a + b,
    identity: 0,
    subtract: (a, b) => a - b);     // нужно для RangeAggregate

Console.WriteLine(bit.PrefixAggregate(3));      // 1 + 2 + 3 = 6
Console.WriteLine(bit.RangeAggregate(1, 4));    // 2 + 3 + 4 = 9

bit.Add(2, 10);
Console.WriteLine(bit.RangeAggregate(1, 4));    // 2 + 13 + 4 = 19
```

### Сложность

| Операция | Время | Память |
|---|---|---|
| Построение | O(n log n) | O(n) |
| Add (точечное) | O(log n) | — |
| PrefixAggregate | O(log n) | — |
| RangeAggregate | O(log n) | — |

### Ограничения

- Для `RangeAggregate` обязательно передавать `subtract` — без него
  выбросит `InvalidOperationException`.
- **Не работает с min/max** — нет обратной операции. Используйте Segment Tree.

---

## Сравнение

| Операция | Segment Tree | Fenwick |
|---|---|---|
| Точечное обновление | O(log n) | O(log n) |
| Range query (sum) | O(log n) | O(log n) |
| Range update | O(log n) (с lazy) | — |
| Поддержка min/max | ✅ | ❌ |
| Память | 4n | n + 1 |
| Константа | Больше | Меньше |

**Выбор:**
- Только сумма, только точечные обновления → **Fenwick**
- Нужны min/max, range updates → **Segment Tree**

---

## Применение в HLD

`HldPathQueries<T, TValue>` использует `SegmentTree` под капотом,
чтобы дать запросы на путях в дереве:

```csharp
using GraphToolkit.Trees;
using GraphToolkit.Utils;

var tree = new GraphBuilder<int>(isDirected: false)
    .AddEdge(1, 2).AddEdge(1, 3)
    .AddEdge(2, 4).AddEdge(2, 5)
    .Build();

var weights = new Dictionary<int, long>
{
    [1] = 10, [2] = 20, [3] = 30, [4] = 40, [5] = 50
};

var hld = new HldPathQueries<int, long>(
    tree, root: 1,
    valueOf: v => weights[v],
    combine: (a, b) => a + b,
    identity: 0);

Console.WriteLine(hld.Query(4, 5));   // 40 + 20 + 50 = 110
hld.Update(2, 100);
Console.WriteLine(hld.Query(4, 5));   // 40 + 100 + 50 = 190
```

См. подробнее в статье [«Продвинутые алгоритмы на деревьях»](advanced-trees.md).

## См. также

- [Продвинутые алгоритмы на деревьях](advanced-trees.md)
- [Производительность](performance.md)