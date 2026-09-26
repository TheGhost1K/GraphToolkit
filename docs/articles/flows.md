---
uid: articles.flows
title: Максимальный поток
---

# Максимальный поток

## Три алгоритма

| Алгоритм — что решает | Время | Особенность |
|---|---|---|
| Форд-Фалкерсон — классика (DFS) | O(E · f) | Зависит от величины потока; может быть очень медленным при больших пропускных способностях |
| Эдмондс-Карп — BFS-версия | O(V · E²) | Не зависит от величины потока |
| Диниц — быстрейший из классических | O(V² · E) | **Рекомендуется по умолчанию** |

### Быстрый выбор

```
Пропускные способности большие (> 10⁶)?
├─ Да → Диниц или Эдмондс-Карп
└─ Нет → Любой из трёх
         Рекомендуется: Диниц
```

### Сравнение на классическом примере (CLRS)

Все три алгоритма дают одинаковый результат:

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

// Каждый алгоритм модифицирует сеть, поэтому клонируем её
double ff = FordFulkerson.Compute(net.Clone(), "S", "T");
double ek = EdmondsKarp.Compute(net.Clone(), "S", "T");
double dn = Dinic.Compute(net.Clone(), "S", "T");

Console.WriteLine($"Форд-Фалкерсон: {ff}");    // 23
Console.WriteLine($"Эдмондс-Карп:   {ek}");    // 23
Console.WriteLine($"Диниц:          {dn}");    // 23

// Оригинал net при этом не изменяется
Console.WriteLine(net.GetCapacity("S", "A"));  // 16
```

> [!TIP]
> Метод `Clone()` есть у обеих сетей — `FlowNetwork<T>` и
> `CostFlowNetwork<T>`. Используйте его, когда нужно запустить
> несколько алгоритмов на одних данных, не восстанавливая
> сеть вручную.

---

## Создание сети

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
```

> [!WARNING]
> `FlowNetwork<T>` **модифицируется** во время работы алгоритма.
> Если нужен исходный граф, сделайте копию.

---

## Диниц (рекомендуется)

```csharp
double maxFlow = Dinic.Compute(net, "S", "T");
Console.WriteLine($"Max flow: {maxFlow}");
// Max flow: 23
```

---

## Эдмондс-Карп

```csharp
double maxFlow = EdmondsKarp.Compute(net, "S", "T");
```

---

## Форд-Фалкерсон (DFS)

```csharp
double maxFlow = FordFulkerson.Compute(net, "S", "T");
```

> [!NOTE]
> Ford-Fulkerson может быть очень медленным при больших пропускных
> способностях (например, 10⁹). Используйте Диница или Эдмондса-Карпа.

---

## Минимальный разрез

После выполнения max-flow можно получить минимальный разрез:

```csharp
var sourceSide = Dinic.MinCut(net, "S", "T");
Console.WriteLine("Сторона источника: " + string.Join(", ", sourceSide));
```

## Применения

- Транспортные сети (пропускная способность дорог)
- Телекоммуникации (пропускная способность каналов)
- Расписания (сопоставление задач и ресурсов)
- Двудольные паросочетания (через max-flow)

## См. также

- [Поток минимальной стоимости](min-cost-flow.md)
- [Разрезы](cuts.md)
- [Паросочетания](matching.md)