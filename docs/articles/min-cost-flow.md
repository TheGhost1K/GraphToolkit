---
uid: articles.min-cost-flow
title: Поток минимальной стоимости
---

# Поток минимальной стоимости (Min-Cost Max-Flow)

Задача: найти поток заданной величины с минимальной суммарной стоимостью.

## Создание сети

```csharp
using GraphToolkit.Core;
using GraphToolkit.Flow;

var net = new CostFlowNetwork<string>();

// AddEdge(from, to, capacity, cost)
net.AddEdge("S", "A", 4, 2);
net.AddEdge("S", "B", 3, 1);
net.AddEdge("A", "B", 1, 1);
net.AddEdge("A", "T", 3, 3);
net.AddEdge("B", "T", 4, 2);
```

---

## Дейкстра с потенциалами (рекомендуется)

Быстрее на больших графах. Работает с отрицательными стоимостями
благодаря начальным потенциалам через Беллмана-Форда.

```csharp
var (flow, cost) = MinCostFlow.DijkstraWithPotentials(net, "S", "T");
Console.WriteLine($"Flow = {flow}, Cost = {cost}");
// Flow = 6, Cost = 25
```

---

## SPFA

Работает с отрицательными стоимостями без предварительной обработки.

```csharp
var (flow, cost) = MinCostFlow.Spfa(net, "S", "T");
```

---

## Ограничение величины потока

Если нужен не максимальный поток, а фиксированной величины:

```csharp
var (flow, cost) = MinCostFlow.DijkstraWithPotentials(
    net, "S", "T", maxFlow: 5);
```

Алгоритм остановится, как только достигнет 5 единиц потока.

---

## Применения

- Транспортная задача (склады → магазины)
- Распределение задач по исполнителям с учётом оплаты
- Балансировка нагрузки с учётом стоимости
- Мультикоммодити-потоки (упрощённые)

## См. также

- [Максимальный поток](flows.md)
- [Паросочетания](matching.md)