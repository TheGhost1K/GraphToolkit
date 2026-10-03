---
uid: articles.csr-graph
title: CSR-граф
---

# CSR-граф для больших данных

**CSR (Compressed Sparse Row)** — компактный формат хранения графа,
использующий два плоских массива вместо списка списков. Даёт **3–10×
экономии памяти** и улучшает cache locality.

## Проблема с обычным `Graph<T>`

`Graph<T>` хранит рёбра как `List<Edge<T>>` для каждой вершины. Для
больших графов это означает:

- **Фрагментация памяти** — миллионы мелких объектов `Edge<T>`
- **GC-давление** — сборщик мусора постоянно работает
- **Плохой cache locality** — обход соседей «прыгает» по памяти

Для V = 10⁶ и E = 10⁷ это создаёт **секунды GC-пауз**.

## Что такое CSR

CSR хранит граф в виде **трёх плоских массивов**:

```
rowPtr: [0, 3, 5, 8, 10]              // индексы начала рёбер
colIdx: [1, 2, 3, 0, 3, 0, 1, 4, 2]   // плоский список соседей
values: [5, 3, 2, 5, 4, 3, 4, 6, 6]   // веса рёбер
```

Для вершины `i` соседи лежат в `colIdx[rowPtr[i]..rowPtr[i+1]]`.

**Память:** `(V + 1) * 4 + E * 4 + E * 8` байт.

Для V = 10⁶, E = 10⁷:
- CSR: ~120 МБ
- Обычный `Graph<T>`: ~500+ МБ

## Создание CSR-графа

```csharp
using GraphToolkit.Core;
using GraphToolkit.Utils;

// Обычный граф
var builder = new GraphBuilder<int>(isDirected: false);
for (int i = 0; i < 1_000_000; i++)
    builder.AddEdge(i, (i + 1) % 1_000_000);
var graph = builder.Build();

// CSR-версия
var csr = new CSRGraph<int>(graph);

Console.WriteLine($"Вершин: {csr.VertexCount}");
Console.WriteLine($"Рёбер:  {csr.EdgeCount}");
Console.WriteLine($"Память: {csr.ApproximateMemoryBytes() / 1024 / 1024} МБ");
```

## Обход соседей через span

Ключевая фича: соседи возвращаются как `ReadOnlySpan<int>` — **без
аллокаций**:

```csharp
int vertexIndex = csr.IndexOf(42);

ReadOnlySpan<int> neighbors = csr.Neighbors(vertexIndex);
foreach (int n in neighbors)
{
    // Быстрый обход без создания временных коллекций
}
```

Это даёт **2–5× ускорение** обхода по сравнению с `List<Edge<T>>`.

## BFS на CSR-графе

Для очень больших графов CSR + параллельный BFS — оптимальная
комбинация:

```csharp
var csr = new CSRGraph<int>(graph);
int source = csr.IndexOf(0);

// BFS вручную на CSR
var visited = new bool[csr.VertexCount];
var queue = new Queue<int>();
queue.Enqueue(source);
visited[source] = true;

var distances = new int[csr.VertexCount];

while (queue.Count > 0)
{
    int u = queue.Dequeue();
    foreach (int v in csr.Neighbors(u))
    {
        if (visited[v]) continue;
        visited[v] = true;
        distances[v] = distances[u] + 1;
        queue.Enqueue(v);
    }
}
```

## Параллельная обработка

CSR хорошо параллелится, потому что каждая вершина хранится
независимо:

```csharp
// Параллельный подсчёт степеней
var degrees = new int[csr.VertexCount];
Parallel.For(0, csr.VertexCount, i =>
{
    degrees[i] = csr.Neighbors(i).Length;
});

// Параллельный подсчёт треугольников
long triangles = 0;
Parallel.For(0, csr.VertexCount, i =>
{
    var neighborsI = csr.Neighbors(i);
    long local = 0;
    foreach (int j in neighborsI)
    {
        if (j <= i) continue;
        var neighborsJ = csr.Neighbors(j);
        // пересечение neighborsI и neighborsJ
        local += neighborsI.ToArray()
            .Intersect(neighborsJ.ToArray()).Count();
    }
    Interlocked.Add(ref triangles, local);
});
```

## Когда использовать CSR

| Случай | Рекомендация |
|---|---|
| V < 10⁴ | Обычный `Graph<T>` — проще |
| V = 10⁵–10⁶ | CSR — экономия памяти |
| V > 10⁶ | CSR **обязателен** |
| Частые изменения структуры | Обычный `Graph<T>` |
| Статический анализ | CSR |
| Итеративные алгоритмы (PageRank) | CSR |

## Ограничения

- **Только чтение** после построения — нельзя добавить/удалить ребро
- **O(V + E)** времени на построение
- **Требует** полного знания структуры заранее
- **Нет интерфейса `IGraph<T>`** — обход вручную через `Neighbors(int)`

## Интеграция с алгоритмами

CSR — низкоуровневая структура. Используйте его для:

- Итеративных алгоритмов (PageRank, Label Propagation)
- Параллельных BFS
- Анализа плотности, треугольников, кластерных коэффициентов

Для высокоуровневых задач (Дейкстра, MST) используйте `IGraph<T>`.

## См. также

- [Параллельные алгоритмы](parallel-algorithms.md)
- [Ключевые концепции](core-concepts.md)
- [Производительность](performance.md)
