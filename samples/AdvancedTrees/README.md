# AdvancedTrees — продвинутые алгоритмы на деревьях

Пример демонстрирует четыре продвинутых алгоритма:

1. **Centroid Decomposition** — разложение дерева по центроидам для
   быстрых запросов расстояний.
2. **Heavy-Light Decomposition (HLD)** — разбиение на тяжёлые пути
   для LCA и запросов на путях.
3. **HLD + Segment Tree** — HLD с запросами `sum`/`min`/`max`
   на путях.
4. **Link-Cut Tree (LCT)** — динамический лес с операциями
   за O(log V).

## Сложности

| Задача | Подход | Сложность |
|---|---|---|
| Расстояние между вершинами | Centroid Decomposition | O(log V) |
| LCA | HLD | O(log V) |
| Путь + обновления | HLD + Segment Tree | O(log² V) |
| Динамические рёбра | Link-Cut Tree | O(log V) амортиз. |

## Запуск

```bash
cd samples/AdvancedTrees
dotnet run
```

## Что демонстрируется

### Centroid Decomposition

```csharp
var cd = new CentroidDecomposition<int>(tree);
Console.WriteLine($"Root: {cd.Root}");
Console.WriteLine($"Distance(1, 7): {cd.Distance(1, 7)}");
foreach (var v in cd.Traverse())
    Console.WriteLine($"{v} depth={cd.Depth(v)}");
```

### HLD

```csharp
var hld = new HeavyLightDecomposition<int>(tree, root: 1);
Console.WriteLine($"LCA(4, 5) = {hld.Lca(4, 5)}");
var path = hld.PathVertices(7, 6);
foreach (var (left, right) in hld.PathSegments(7, 6))
    Console.WriteLine($"Segment [{left}, {right}]");
```

### HLD + запросы

```csharp
var sumQueries = new HldPathQueries<string, long>(
    tree, root: "A",
    valueOf: v => weights[v],
    combine: (a, b) => a + b,
    identity: 0);

Console.WriteLine(sumQueries.Query("D", "F"));   // сумма на пути
sumQueries.Update("B", 100);                     // точечное обновление
```

### Link-Cut Tree

```csharp
var lct = new LinkCutTree<int>();
for (int i = 1; i <= 6; i++) { lct.AddVertex(i); lct.SetValue(i, i * 10); }

lct.Link(1, 2);
lct.Link(2, 3);
Console.WriteLine(lct.PathSum(1, 3));    // сумма весов на пути
lct.Cut(2, 3);
Console.WriteLine(lct.Connected(1, 3));  // false
```

## См. также

- [Продвинутые алгоритмы на деревьях](../../docs/articles/advanced-trees.md)
- [Алгоритмы на деревьях](../../docs/articles/trees.md)
- [Деревья отрезков и Fenwick](../../docs/articles/structures.md)
