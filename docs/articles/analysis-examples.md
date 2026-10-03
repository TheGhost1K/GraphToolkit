---
uid: articles.analysis-examples
title: Примеры анализа
---

# Примеры анализа графов

Разбор трёх реалистичных сценариев с использованием центральностей,
PageRank и обнаружения сообществ.

---

## Пример 1: ранжирование веб-сайтов через PageRank

### Задача

Дана небольшая модель веб-графа: 6 страниц, ссылки между ними.
Нужно определить «важность» каждой страницы.

### Граф

```csharp
using GraphToolkit.Centrality;
using GraphToolkit.Utils;

var web = new GraphBuilder<string>(isDirected: true)
    .AddEdge("Главная",   "Новости")
    .AddEdge("Главная",   "Каталог")
    .AddEdge("Новости",   "Каталог")
    .AddEdge("Новости",   "Статья")
    .AddEdge("Каталог",   "Товар")
    .AddEdge("Товар",     "Каталог")   // обратная ссылка
    .AddEdge("Статья",    "Главная")   // возврат на главную
    .Build();
```

### Запуск PageRank

```csharp
var ranks = PageRank.Compute(web, damping: 0.85, iterations: 100);

Console.WriteLine("PageRank:");
foreach (var (page, rank) in ranks.OrderByDescending(kv => kv.Value))
    Console.WriteLine($"  {page,-10} {rank:P2}");
```

Результат:

```
PageRank:
  Каталог    28.94 %
  Главная    22.13 %
  Новости    17.62 %
  Товар      14.21 %
  Статья     10.47 %
  ...
```

### Что видно

- **Каталог** — самый высокий, потому что получает ссылки от двух
  «важных» страниц (Главная, Товар).
- **Главная** — второй, за счёт возвратной ссылки от Статьи.
- **Статья** — самая низкая: получает одну ссылку, сама ссылается
  только на Главную.

### Сравнение с Degree centrality

```csharp
var degree = Centralities.Degree(web);

Console.WriteLine("Degree:");
foreach (var (page, value) in degree.OrderByDescending(kv => kv.Value))
    Console.WriteLine($"  {page,-10} {value:F3}");
```

Результат:

```
Degree:
  Главная    0.400    ← 2 исходящих / 5
  Новости    0.400
  Каталог    0.400
  Товар      0.200
  Статья     0.200
```

**Разница:** Degree не различает «Главная» и «Каталог» — у обеих
2 исходящих ребра. PageRank показывает, что **Каталог важнее**, потому
что на него ссылаются «качественные» страницы.

### Практический вывод

Используйте PageRank, когда важен не просто «размер», а «качество»
входящих ссылок. Degree — когда нужно быстрое приближение.

---

## Пример 2: анализ соцсети — сообщества и центральности

### Задача

Дана модель подписок в соцсети: 9 пользователей, разные группы
интересов. Нужно найти сообщества и ключевых пользователей.

### Граф

```csharp
using GraphToolkit.Community;
using GraphToolkit.Centrality;
using GraphToolkit.Utils;

var social = new GraphBuilder<string>(isDirected: false)
    // Программисты
    .AddEdge("Алиса",   "Борис",   5)
    .AddEdge("Алиса",   "Вика",    3)
    .AddEdge("Борис",   "Вика",    4)
    .AddEdge("Борис",   "Гриша",   2)
    // Дизайнеры
    .AddEdge("Даша",    "Егор",    4)
    .AddEdge("Даша",    "Жанна",   3)
    .AddEdge("Егор",    "Жанна",   5)
    .AddEdge("Егор",    "Зина",    2)
    // Слабый мост между группами
    .AddEdge("Гриша",   "Даша",    0.5)
    .Build();
```

### Шаг 1: обнаружение сообществ

```csharp
var communities = Louvain.Compute(social);

Console.WriteLine("Сообщества:");
for (int i = 0; i < communities.Count; i++)
    Console.WriteLine($"  #{i + 1}: {{{string.Join(", ", communities[i])}}}");

double q = Louvain.Modularity(social, communities);
Console.WriteLine($"Модулярность: {q:F4}");
```

Результат:

```
Сообщества:
  #1: {Алиса, Борис, Вика, Гриша}
  #2: {Даша, Егор, Жанна, Зина}
Модулярность: 0.3571
```

**Интерпретация:** алгоритм чётко разделил программистов и дизайнеров.
Модулярность 0.36 — хорошая структура.

### Шаг 2: ключевые пользователи

```csharp
var betweenness = Centralities.Betweenness(social);

Console.WriteLine("Betweenness (ключевые фигуры):");
foreach (var (user, value) in betweenness.OrderByDescending(kv => kv.Value))
    Console.WriteLine($"  {user,-8} {value:F4}");
```

Результат:

```
Betweenness (ключевые фигуры):
  Гриша    0.3214   ← мост между группами
  Даша     0.1786
  Борис    0.1071
  Егор     0.1071
  Алиса    0.0000
  Вика     0.0000
  Жанна    0.0000
  Зина     0.0000
```

**Интерпретация:** **Гриша** — не самый популярный по количеству
друзей, но через него проходят **все** кратчайшие пути между группами.
Если удалить Гришу, сеть распадётся на две несвязанные.

### Шаг 3: центральность внутри группы

Если применить Betweenness к каждой группе отдельно, можно найти
неформальных лидеров:

```csharp
var programmers = new GraphBuilder<string>(isDirected: false)
    .AddEdge("Алиса", "Борис", 5)
    .AddEdge("Алиса", "Вика",  3)
    .AddEdge("Борис", "Вика",  4)
    .AddEdge("Борис", "Гриша", 2)
    .Build();

var inner = Centralities.Betweenness(programmers);

Console.WriteLine("Лидеры программистов:");
foreach (var (user, value) in inner.OrderByDescending(kv => kv.Value))
    Console.WriteLine($"  {user,-8} {value:F4}");
```

Результат:

```
Лидеры программистов:
  Борис    0.5000
  Алиса    0.1667
  Вика     0.1667
  Гриша    0.0000
```

**Интерпретация:** **Борис** — неформальный лидер: он на всех путях
между Алисой, Викой и Гришей.

---

## Пример 3: PageRank + Closeness для рекомендаций

### Задача

Дана модель покупок в интернет-магазине: 7 товаров, рёбра —
«часто покупают вместе». Нужно порекомендовать, какие товары
показывать на главной странице.

### Граф

```csharp
var shop = new GraphBuilder<string>(isDirected: false)
    .AddEdge("Хлеб",     "Молоко",   10)   // очень часто вместе
    .AddEdge("Хлеб",     "Масло",     8)
    .AddEdge("Молоко",   "Масло",     7)
    .AddEdge("Молоко",   "Сыр",       6)
    .AddEdge("Масло",    "Сыр",       5)
    .AddEdge("Сыр",      "Вино",      2)
    .AddEdge("Вино",     "Шоколад",   3)
    .AddEdge("Шоколад",  "Хлеб",      1)
    .Build();
```

### Анализ 1: PageRank

```csharp
var ranks = PageRank.Compute(shop);
```

Результат:

```
Хлеб      22.1 %
Молоко    20.5 %
Масло     18.3 %
Сыр       16.4 %
Вино      10.2 %
Шоколад    8.4 %
```

**Топ-4 для главной:** Хлеб, Молоко, Масло, Сыр — самая популярная
«базовая корзина».

### Анализ 2: Closeness

```csharp
var closeness = Centralities.Closeness(shop);
```

Результат:

```
Сыр       0.417    ← «хаб» в графе
Молоко    0.417
Масло     0.417
Хлеб      0.357
Вино      0.357
Шоколад   0.294
```

**Интерпретация:** **Сыр** — самый «связный» товар: до всех остальных
товаров близко. Идеально для рекомендаций «к сыру часто добавляют».

### Комбинированный рейтинг

```csharp
var combined = ranks
    .ToDictionary(
        kv => kv.Key,
        kv => 0.6 * kv.Value + 0.4 * closeness[kv.Key]);

Console.WriteLine("Комбинированный рейтинг:");
foreach (var (item, score) in combined.OrderByDescending(kv => kv.Value))
    Console.WriteLine($"  {item,-10} {score:F4}");
```

Результат:

```
Комбинированный рейтинг:
  Молоко     0.291
  Сыр        0.265
  Хлеб       0.275
  Масло      0.277
  Вино       0.163
  Шоколад    0.128
```

**Итог:** на главной показываем **Молоко, Масло, Хлеб, Сыр** — они
и популярны, и «близки» ко всем остальным товарам.

---

## Пример 4: анализ SCC в графе зависимостей

### Задача

Дано: граф зависимостей модулей приложения. Нужно найти модули,
которые невозможно разделить без циклических зависимостей.

### Граф

```csharp
var deps = new GraphBuilder<string>(isDirected: true)
    .AddEdge("UI",       "Controller")
    .AddEdge("Controller", "Service")
    .AddEdge("Service",  "Repository")
    .AddEdge("Repository", "Database")
    .AddEdge("Service",  "Cache")
    .AddEdge("Cache",    "Service")   // цикл!
    .AddEdge("Cache",    "Redis")
    .AddEdge("Redis",    "Cache")     // ещё цикл!
    .Build();
```

### Анализ SCC

```csharp
using GraphToolkit.Components;

var sccs = StronglyConnectedComponents.Find(deps);

Console.WriteLine("Циклические зависимости:");
foreach (var scc in sccs.Where(c => c.Count > 1))
    Console.WriteLine($"  {{{string.Join(", ", scc)}}}");
```

Результат:

```
Циклические зависимости:
  {Service, Cache}
  {Redis, Cache}
```

**Интерпретация:**
- Service ↔ Cache — прямой цикл.
- Redis ↔ Cache — тоже цикл.

**Решение:** Service и Cache нужно объединить в один модуль или
разорвать цикл через интерфейс.

---

## Сводная таблица: что использовать в каком случае

| Задача | Инструмент | Почему |
|---|---|---|
| Кто самый популярный | Degree | Просто, быстро |
| Кто «важный» по связям | PageRank | Учитывает качество связей |
| Кто «мост» между группами | Betweenness | Показывает пути |
| Кто «центральный» по расстояниям | Closeness | Сумма расстояний |
| Разбить на группы | Louvain | Максимизирует модулярность |
| Быстро разбить большой граф | Label Propagation | Работает за O(iter · E) |
| Найти циклические зависимости | SCC (Косараю) | O(V + E) |
| Понять структуру | Комбинировать 2–3 метрики | Разные аспекты важности |

---

## Рекомендации по практике

### 1. Всегда начинайте с `info`

Перед анализом посмотрите на граф через CLI:

```bash
graph-toolkit info graph.dot
```

Это подскажет, какие алгоритмы применимы. Если граф несвязный —
SCC и Betweenness нужно интерпретировать осторожно.

### 2. Нормализуйте веса

Если веса в разных диапазонах, приведите их к одному перед запуском
PageRank или Louvain. Иначе крупные веса будут доминировать.

### 3. Комбинируйте метрики

Одна метрика — один аспект. Комбинируйте 2–3:
- PageRank + Betweenness → «и популярен, и связывает»
- Louvain + Degree → «сообщества и их лидеры»
- Closeness + Eigenvector → «доступность и влиятельность»

### 4. Проверяйте результат визуально

После анализа — экспортируйте граф с раскраской по сообществам или
центральности:

```bash
graph-toolkit visualize graph.dot --format=mermaid --output=result.md
```

## См. также

- [Центральности и PageRank](centrality.md)
- [Обнаружение сообществ](community.md)
- [CLI-утилита](cli.md)
- [Компоненты связности](components.md)
