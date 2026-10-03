# Structures — Segment Tree и Fenwick Tree

Пример демонстрирует две структуры данных для запросов на отрезках:

1. **Segment Tree** — запросы `sum`/`min`/`max` + range update
   с lazy propagation.
2. **Fenwick Tree** — быстрые суммы с точечными обновлениями.

## Сравнение

| Операция | Segment Tree | Fenwick |
|---|---|---|
| Точечное обновление | O(log n) | O(log n) |
| Запрос на отрезке | O(log n) | O(log n) |
| Range update | O(log n) | — |
| Min/max | ✅ | ❌ |
| Память | 4n | n + 1 |
| Константа | Больше | Меньше |

## Запуск

```bash
cd samples/Structures
dotnet run
```

## Что демонстрируется

### 1. Сумма на отрезке

```csharp
var st = new SegmentTree<long>(
    data,
    combine: (a, b) => a + b,
    identity: 0);

st.Query(2, 5);      // сумма на [2, 5)
st.Update(4, 100);   // замена элемента
```

### 2. Min / max

```csharp
var minTree = new SegmentTree<int>(
    data,
    combine: Math.Min,
    identity: int.MaxValue);

minTree.Query(0, 8);  // минимум на всём массиве
```

### 3. Range update

```csharp
var st = new SegmentTree<int>(
    data,
    combine: Math.Min,
    identity: int.MaxValue,
    applyLazy: (_, newValue) => newValue,   // присвоить
    composeLazy: (_, newer) => newer);      // новое перекрывает

st.RangeUpdate(2, 6, 10);   // присвоить 10 позициям 2..5
```

### 4. Fenwick

```csharp
var bit = new FenwickTree<long>(
    data,
    add: (a, b) => a + b,
    identity: 0,
    subtract: (a, b) => a - b);

bit.PrefixAggregate(5);      // сумма первых 5
bit.RangeAggregate(2, 7);    // сумма на [2, 7)
bit.Add(3, 100);             // добавить 100 к позиции 3
```

### 5. Производительность

Пример запускает 100 000 запросов на массиве 100 000 элементов
и сравнивает время через `Stopwatch`. Обычно Fenwick быстрее
в 1.5–3× для операций суммы.

## Когда использовать

- **Только сумма + точечные обновления** → Fenwick
- **Min/max, range updates** → Segment Tree
- **Нужны оба типа** → Segment Tree (или две Fenwick-структуры)

## Применения

- Range queries в массивах
- HLD + Segment Tree → запросы на путях в дереве
- Разреженные таблицы (альтернатива для offline-запросов)
- Динамические суммы / min / max

## См. также

- [Деревья отрезков и Fenwick](../../docs/articles/structures.md)
- [Продвинутые алгоритмы на деревьях](../../docs/articles/advanced-trees.md)
