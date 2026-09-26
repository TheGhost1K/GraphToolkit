---
uid: articles.advanced-trees
title: Продвинутые алгоритмы на деревьях
---

# Продвинутые алгоритмы на деревьях

Помимо базовых операций (LCA, диаметр, центроид), библиотека
предоставляет четыре продвинутых инструмента для задач на деревьях:

- **Центроидная декомпозиция** — разложение по центроидам
- **Heavy-Light Decomposition (HLD)** — тяжёлые пути
- **HLD + запросы на путях** — обёртка с деревом отрезков
- **Link-Cut Tree (LCT)** — динамический лес

## Центроидная декомпозиция

Разбивает дерево на уровни по центроидам: корень — центроид всего дерева,
дети — центроиды поддеревьев, полученных удалением корня, и т. д.
Высота дерева центроидов — O(log V).

### Пример

```csharp
using GraphToolkit.Trees;
using GraphToolkit.Utils;

var tree = new GraphBuilder<int>(isDirected: false)
    .AddEdge(1, 2, 1)
    .AddEdge(2, 3, 1)
    .AddEdge(3, 4, 1)
    .AddEdge(4, 5, 1)
    .Build();

var cd = new CentroidDecomposition<int>(tree);

Console.WriteLine($"Корень дерева центроидов: {cd.Root}");   // 3
Console.WriteLine($"Расстояние 1 → 5: {cd.Distance(1, 5)}"); // 4
Console.WriteLine($"Глубина вершины 1: {cd.Depth(1)}");

foreach (var v in cd.Traverse())
    Console.WriteLine($"  {v} (глубина {cd.Depth(v)})");
```

### Сложность

| Операция | Время |
|---|---|
| Построение | O(V log V) |
| `Distance(u, v)` | O(log V) |
| `Parent(v)`, `Depth(v)`, `Children(v)` | O(1) |

### Применения

- **Запросы расстояний** между парами вершин на дереве
- **Подсчёт путей** с заданным свойством
- **Offline-задачи**: найти k-ю вершину на пути, ближайшую «помеченную» и т. п.

### Ограничения

- Работает только для **деревьев** (проверка в конструкторе).
- Требует, чтобы у вершин были предпосчитанные расстояния до центроидных
  предков — это делается автоматически при построении.

---

## Heavy-Light Decomposition (HLD)

Разбивает дерево на «тяжёлые пути» так, что любой путь между двумя
вершинами пересекает O(log V) путей. Позволяет применять одномерные
структуры (дерево отрезков, Fenwick) к путям в дереве.

### Пример

```csharp
var tree = new GraphBuilder<int>(isDirected: false)
    .AddEdge(1, 2).AddEdge(1, 3)
    .AddEdge(2, 4).AddEdge(2, 5)
    .Build();

var hld = new HeavyLightDecomposition<int>(tree, root: 1);

// LCA за O(log V)
Console.WriteLine(hld.Lca(4, 5));   // 2

// Разбиение пути на отрезки для запроса к дереву отрезков
foreach (var (left, right) in hld.PathSegments(4, 3))
    Console.WriteLine($"Отрезок [{left}, {right}]");

// Все вершины на пути
var path = hld.PathVertices(4, 3);
Console.WriteLine(string.Join(" → ", path));   // 4 → 2 → 1 → 3
```

### Сложность

| Операция | Время |
|---|---|
| Построение | O(V) |
| `Lca(u, v)` | O(log V) |
| `PathSegments(u, v)` | O(log V) отрезков |
| `PathVertices(u, v)` | O(log V + размер пути) |

### Применения

- Запросы `min`/`max`/`sum` на путях в дереве
- Динамические обновления весов вершин
- Задачи с путями в дереве и произвольными отрезковыми операциями

### Ключевые свойства

- **Итеративная реализация** — не боится деревьев-цепочек на 10⁵+ вершин.
- **Тяжёлые пути** занимают **непрерывные отрезки** в линейном массиве.
- **Позиция вершины** доступна через `Position(v)`, обратно — `VertexAt(pos)`.

---

## HLD + запросы на путях

Обёртка `HldPathQueries<T, TValue>` объединяет HLD и `SegmentTree`
для запросов на путях «из коробки».

### Пример

```csharp
using GraphToolkit.Trees;

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

// Сумма весов на пути 4 → 5 = 40 + 20 + 50 = 110
Console.WriteLine(hld.Query(4, 5));

// Обновить вес вершины 2
hld.Update(2, 100);
Console.WriteLine(hld.Query(4, 5));   // 190
```

### Сложность

| Операция | Время |
|---|---|
| Построение | O(V) |
| `Query(u, v)` | O(log² V) |
| `Update(v, value)` | O(log V) |

### Применения

- Динамические запросы `sum`/`min`/`max` на путях
- Задачи с обновлениями весов вершин
- Оффлайн-запросы вида «изменить значение и узнать агрегат на пути»

---

## Link-Cut Tree (LCT)

Динамический лес с поддержкой добавления и удаления рёбер за
амортизированное O(log V) на операцию.

### Пример

```csharp
using GraphToolkit.Trees;

var lct = new LinkCutTree<int>();

for (int i = 1; i <= 5; i++)
{
    lct.AddVertex(i);
    lct.SetValue(i, i);   // вес вершины i
}

lct.Link(1, 2);
lct.Link(2, 3);
lct.Link(3, 4);
lct.Link(4, 5);

Console.WriteLine(lct.PathSum(1, 5));   // 1 + 2 + 3 + 4 + 5 = 15

lct.Cut(3, 4);
Console.WriteLine(lct.Connected(1, 5)); // false

lct.Link(1, 5);
Console.WriteLine(lct.Connected(1, 5)); // true
```

### Сложность

Амортизированное **O(log V)** на каждую операцию:

| Операция | Назначение |
|---|---|
| `AddVertex(v)` | Добавить изолированную вершину |
| `SetValue(v, x)` | Задать вес вершины |
| `MakeRoot(v)` | Сделать вершину корнем её дерева |
| `Link(u, v)` | Добавить ребро между разными деревьями |
| `Cut(u, v)` | Удалить ребро |
| `Connected(u, v)` | Проверить связность |
| `PathSum(u, v)` | Сумма весов на пути |

### Применения

- **Динамические деревья**, где рёбра добавляются и удаляются
- **Maximum spanning forest** в динамическом графе
- **Dynamic connectivity** в специальных случаях
- Задачи с «ссылками» и «разрезами» в реальном времени

### Ограничения

- Размер кода и константа времени выше, чем у HLD.
- **Используйте HLD**, если дерево статическое — он проще и быстрее.
- Реализована только `PathSum` — для других агрегатов (min, max, gcd)
  нужно адаптировать `Node` и метод `Update`.

---

## Сравнение подходов

| Задача | Инструмент | Сложность |
|---|---|---|
| Запросы расстояний | `CentroidDecomposition` | O(log V) |
| LCA + пути (статично) | `HeavyLightDecomposition` | O(log V) |
| Пути + обновления | `HldPathQueries` | O(log² V) |
| Динамические рёбра | `LinkCutTree` | O(log V) амортиз. |

**Рекомендация:** начинайте с `HeavyLightDecomposition` для статических
задач — он проще и предсказуемее. `LinkCutTree` нужен только если
рёбра действительно меняются.

## См. также

- [Алгоритмы на деревьях](trees.md) — базовые: LCA, диаметр, центроид
- [Деревья отрезков и Fenwick](structures.md) — структуры для запросов
- [Производительность](performance.md) — таблица сложностей