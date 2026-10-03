---
uid: articles.advanced-topics
title: Продвинутые структуры и алгоритмы
---

# Продвинутые структуры и алгоритмы

Эта статья собирает **дополнительные возможности** библиотеки,
которые не вошли в основные разделы: альтернативы классическим
алгоритмам, специализированные структуры и улучшенные версии.

## Содержание

- [Push-Relabel](#push-relabel) — быстрый max-flow для плотных графов
- [CSR-граф](#csr-граф) — компактное представление для V > 10⁶
- [Взвешенный Label Propagation](#взвешенный-label-propagation) — улучшенное обнаружение сообществ

---

## Push-Relabel

**Push-Relabel** (Goldberg-Tarjan) — альтернатива Диницу для задачи
максимального потока. Работает с **предпотоком**: поддерживает избыток
в каждой вершине и постепенно «проталкивает» его к стоку.

### Пример

```csharp
using GraphToolkit.Core;
using GraphToolkit.Flow;

var net = new FlowNetwork<string>();
net.AddEdge("S", "A", 16);
net.AddEdge("S", "B", 13);
net.AddEdge("A", "B", 10);
net.AddEdge("A", "C", 12);
net.AddEdge("B", "D", 14);
net.AddEdge("C", "B", 9);
net.AddEdge("C", "T", 20);
net.AddEdge("D", "C", 7);
net.AddEdge("D", "T", 4);

double maxFlow = PushRelabel.Compute(net, "S", "T");
Console.WriteLine($"Max flow: {maxFlow}");   // 23
```

### Сравнение с другими алгоритмами

| Алгоритм | Сложность | Когда лучше |
|---|---|---|
| Форд-Фалкерсон | O(E · f) | Малые графы |
| Эдмондс-Карп | O(V · E²) | Средние графы |
| **Диниц** | O(V² · E) | Универсальный |
| **Push-Relabel** | O(V³) / O(V²√E) | **Плотные графы** |

### Когда использовать

- ✅ Плотные графы (E ≈ V²)
- ✅ Графы с большим числом внутренних рёбер
- ✅ Когда Диниц работает медленно
- ❌ Разреженные графы (Диниц быстрее)

### Особенность

**Не модифицирует** переданную сеть — все изменения в локальной копии.
Это отличается от `Dinic.Compute`, который мутирует `FlowNetwork<T>`.

---

## CSR-граф

**Compressed Sparse Row** — компактный формат для **очень больших**
графов (V > 10⁶), не влезающих в стандартное `List<Edge>[]`.

### Структура

CSR использует три массива:

- `rowPtr[n + 1]` — индексы начала рёбер каждой вершины
- `colIdx[m]` — плоский массив соседей
- `weights[m]` — веса рёбер (опционально)

### Пример

```csharp
using GraphToolkit.Core;
using GraphToolkit.Utils;

var graph = new GraphBuilder<int>(isDirected: true)
    .AddEdge(0, 1, 5)
    .AddEdge(0, 2, 3)
    .AddEdge(1, 3, 2)
    .Build();

var csr = new CSRGraph<int>(graph);

// Обход соседей через Span (без аллокаций)
int v = csr.IndexOf(0);
var neighbors = csr.Neighbors(v);
var weights = csr.NeighborWeights(v);

for (int i = 0; i < neighbors.Length; i++)
{
    var neighborVertex = csr.VertexAt(neighbors[i]);
    Console.WriteLine($"{neighborVertex} вес {weights[i]}");
}

// Оценка памяти
Console.WriteLine($"≈ {csr.ApproximateMemoryBytes()} байт");
```

### Преимущества

| | `Graph<T>` | `CSRGraph<T>` |
|---|---|---|
| Память | O(V + E) с overhead | O(V + E) компактно |
| Добавление рёбер | O(1) | Только при построении |
| Обход соседей | O(deg) | O(deg) через `Span` |
| Совместимость | `IGraph<T>` | Своя (нет `IGraph<T>`) |
| Cache-friendly | Средне | **Да** |

### Когда использовать

- ✅ Граф статический (не меняется после построения)
- ✅ V > 10⁶
- ✅ Важна экономия памяти (3–10× меньше)
- ✅ Частые обходы соседей (например, BFS/DFS)
- ❌ Динамические графы с добавлением/удалением рёбер

### Ограничение

`CSRGraph<T>` **не реализует** `IGraph<T>` — потому что работает с
индексами, а не с `T`. Все алгоритмы GraphToolkit принимают
`IGraph<T>`, поэтому для использования CSR нужно либо адаптировать
алгоритм под индексы, либо конвертировать в `Graph<T>` (неэффективно).

---

## Взвешенный Label Propagation

Улучшенная версия Label Propagation, учитывающая **веса рёбер** при
выборе метки соседа.

### Отличие от базовой версии

**Базовая:** метка выбирается по частоте среди соседей (не считая веса).

**Взвешенная:** метка выбирается по **сумме весов** рёбер к соседям с
этой меткой.

### Пример

```csharp
using GraphToolkit.Community;

var graph = new GraphBuilder<int>(isDirected: false)
    // Клика 1
    .AddEdge(1, 2, 5).AddEdge(2, 3, 5).AddEdge(3, 1, 5)
    // Клика 2
    .AddEdge(4, 5, 5).AddEdge(5, 6, 5).AddEdge(6, 4, 5)
    // Слабый мост
    .AddEdge(3, 4, 0.1)
    .Build();

var communities = WeightedLabelPropagation.Compute(graph, seed: 42);
```

### Когда даёт выигрыш

- ✅ Веса рёбер **сильно различаются** (> 10×)
- ✅ Есть чёткие «мосты» между группами
- ✅ Нужно более устойчивое разбиение

### Когда не отличается от базового

- ❌ Все веса равны
- ❌ Граф однородный

### Сложность

**O(iter · E)**, как и базовая версия.

### Не детерминирован

Как и базовый Label Propagation, зависит от порядка обхода.
Используйте `seed` для воспроизводимости.

---

## См. также

- [Максимальный поток](flows.md) — Диниц, Эдмондс-Карп, Форд-Фалкерсон
- [Деревья отрезков и Fenwick](structures.md) — Segment Tree, Fenwick
- [Обнаружение сообществ](community.md) — Louvain, Label Propagation
- [Параллельные алгоритмы](parallel-algorithms.md) — 9 параллельных версий
