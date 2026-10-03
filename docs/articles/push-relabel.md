---
uid: articles.push-relabel
title: Push-Relabel
---

# Push-Relabel (Goldberg-Tarjan)

Альтернативный алгоритм для максимального потока. Часто **быстрее
Диница** на плотных графах.

## Когда использовать

| Граф | Рекомендация |
|---|---|
| Разреженный (E ≈ V) | Диниц |
| Плотный (E ≈ V²) | **Push-Relabel** |
| Много стоков/истоков | Push-Relabel с super-source |
| Ограничение на память | Диниц (меньше памяти) |

## Пример

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

double flow = PushRelabel.Compute(net, "S", "T");
Console.WriteLine($"Max flow: {flow}");   // 23
```

## Как работает

Алгоритм поддерживает **высоты** вершин и **избыток** потока:

1. **Инициализация:** все рёбра из источника насыщаются, источник
   получает высоту `V`.
2. **Push:** если высота `u = высота v + 1` и есть остаточная
   пропускная способность, поток «проталкивается» из `u` в `v`.
3. **Relabel:** если нельзя протолкнуть ни в одну вершину, `u`
   получает новую высоту `min(высота соседей) + 1`.
4. Повторять, пока есть активные вершины (с избытком).

Алгоритм завершается, когда ни одна промежуточная вершина не имеет
избытка.

## Сравнение с Диницем

| Характеристика | Диниц | Push-Relabel |
|---|---|---|
| Сложность | O(V² · E) | O(V³) базовый / O(V² · √E) с эвристиками |
| Плотные графы | Медленнее | **Быстрее** |
| Разреженные графы | **Быстрее** | Медленнее |
| Память | O(V + E) | O(V²) — матрица |
| Параллелизм | Плохой | Хороший |

## Практические советы

### 1. Плотные графы

```csharp
// Граф с E ≈ V²/2
if (graph.EdgeCount > graph.VertexCount * graph.VertexCount / 4)
{
    flow = PushRelabel.Compute(net, s, t);
}
else
{
    flow = Dinic.Compute(net, s, t);
}
```

### 2. Сравнение производительности

```csharp
var sw = Stopwatch.StartNew();
var pr = PushRelabel.Compute(net.Clone(), "S", "T");
sw.Stop();
Console.WriteLine($"Push-Relabel: {sw.ElapsedMilliseconds} мс");

sw.Restart();
var dn = Dinic.Compute(net.Clone(), "S", "T");
sw.Stop();
Console.WriteLine($"Диниц: {sw.ElapsedMilliseconds} мс");
```

## Ограничения текущей реализации

- Последовательная (не использует все ядра)
- Требует O(V²) памяти для матрицы пропускных способностей
- Для V > 10⁴ память может стать проблемой

## См. также

- [Максимальный поток](flows.md) — обзор всех алгоритмов
- [Разрезы](cuts.md) — минимальный разрез
- [Параллельные алгоритмы](parallel-algorithms.md)
