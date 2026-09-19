---
uid: articles.flows
title: Максимальный поток
---

# Максимальный поток

## Три алгоритма

| Алгоритм | Сложность | Примечание |
|---|---|---|
| Форд-Фалкерсон | O(E · f) | Классика, зависит от величины потока |
| Эдмондс-Карп | O(V · E²) | BFS-версия Форда-Фалкерсона |
| Диниц | O(V² · E) | Самый быстрый из классических |

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