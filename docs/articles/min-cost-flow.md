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

## Клонирование сети

Как и `FlowNetwork<T>`, класс `CostFlowNetwork<T>` модифицируется
во время работы алгоритмов. Для сравнения разных алгоритмов на
одних данных используйте `Clone()`:

```csharp
var net = new CostFlowNetwork<string>();
net.AddEdge("S", "A", 4, 2);
net.AddEdge("S", "B", 3, 1);
net.AddEdge("A", "B", 1, 1);
net.AddEdge("A", "T", 3, 3);
net.AddEdge("B", "T", 4, 2);

// Оба алгоритма работают на своих копиях
var (f1, c1) = MinCostFlow.Spfa(net.Clone(), "S", "T");
var (f2, c2) = MinCostFlow.DijkstraWithPotentials(net.Clone(), "S", "T");

Console.WriteLine($"SPFA:              flow={f1}, cost={c1}");
Console.WriteLine($"Дейкстра:          flow={f2}, cost={c2}");
Console.WriteLine($"Совпадают:         {f1 == f2 && Math.Abs(c1 - c2) < 0.001}");
```

**Важно:** `Clone()` копирует не только исходные рёбра, но и
**текущее состояние остаточных пропускных способностей**. Это
позволяет продолжить работу с частично «использованной» сетью:

```csharp
// Отправили 2 единицы из 4
MinCostFlow.Spfa(net, "S", "T", maxFlow: 2);

// Клонируем — в копии можно докачать оставшиеся 2
var residual = net.Clone();
var (flow, _) = MinCostFlow.Spfa(residual, "S", "T");
Console.WriteLine(flow);   // 2
```

## Применения

- Транспортная задача (склады → магазины)
- Распределение задач по исполнителям с учётом оплаты
- Балансировка нагрузки с учётом стоимости
- Мультикоммодити-потоки (упрощённые)

## См. также

- [Максимальный поток](flows.md)
- [Паросочетания](matching.md)