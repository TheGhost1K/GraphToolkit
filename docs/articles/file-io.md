---
uid: articles.file-io
title: Работа с файлами (IO)
---

# Работа с файлами (IO)

Библиотека поддерживает импорт и экспорт графов в пяти форматах:
**GraphML**, **GEXF**, **JSON**, **CSV**, **DOT**. Это позволяет
обмениваться данными с Gephi, NetworkX, Cytoscape, GraphViz и другими
инструментами.

## Сводная таблица

| Формат | Импорт | Экспорт | Для чего |
|---|---|---|---|
| **GraphML** | ✅ | ✅ | Стандарт для соцсетей, Gephi |
| **GEXF** | ✅ | ✅ | Gephi, Cytoscape |
| **JSON** | ✅ | ✅ | Собственный обмен, API |
| **CSV** | ✅ | ✅ | Excel, SQL, pandas |
| **DOT** | ✅ | ✅ | GraphViz |

---

## GraphML

Стандарт XML для описания графов. Поддерживает веса рёбер через
атрибут `weight`.

### Сохранение

```csharp
using GraphToolkit.IO;

var graph = new GraphBuilder<string>(isDirected: true)
    .AddEdge("A", "B", 4.5)
    .AddEdge("B", "C", 2.0)
    .Build();

GraphIO.SaveGraphML(graph, "graph.graphml");
```

Результат (фрагмент):

```xml
<graphml xmlns="http://graphml.graphdrawing.org/xmlns">
  <key id="w" for="edge" attr.name="weight" attr.type="double"/>
  <graph id="G" edgedefault="directed">
    <node id="A"/>
    <node id="B"/>
    <node id="C"/>
    <edge source="A" target="B">
      <data key="w">4.5</data>
    </edge>
    ...
  </graph>
</graphml>
```

### Загрузка

```csharp
var graph = GraphIO.LoadGraphML("graph.graphml", s => s);
Console.WriteLine(graph.VertexCount);   // 3
Console.WriteLine(graph.EdgeCount);     // 2
```

Вершины парсятся из `id` через функцию `parseVertex`. Для `int`:

```csharp
var intGraph = GraphIO.LoadGraphML("graph.graphml", int.Parse);
```

### Открыть в Gephi

Gephi → **File** → **Open** → выбери `.graphml`. Веса рёбер
подхватятся автоматически.

---

## GEXF

Формат Gephi. Похож на GraphML, но чуть проще.

### Сохранение и загрузка

```csharp
GraphIO.SaveGexf(graph, "graph.gexf");
var loaded = GraphIO.LoadGexf("graph.gexf", s => s);
```

### Открыть в Gephi

Gephi → **File** → **Open** → `.gexf`. Формат нативный для Gephi,
все атрибуты подхватятся.

---

## JSON

Собственный формат для быстрого обмена. Не требует внешних
зависимостей — использует `System.Text.Json`.

### Структура

```json
{
  "directed": true,
  "vertices": ["A", "B", "C"],
  "edges": [
    { "from": "A", "to": "B", "weight": 4.5 },
    { "from": "B", "to": "C", "weight": 2.0 }
  ]
}
```

### Сохранение

```csharp
GraphIO.SaveJson(graph, "graph.json");
```

По умолчанию с отступами. Для компактного вывода:

```csharp
GraphIO.SaveJson(graph, "graph.json", indented: false);
```

### Загрузка

```csharp
var loaded = GraphIO.LoadJson("graph.json", s => s);
```

---

## CSV

Простейший формат: одна строка — одно ребро.

### Структура

```
source,target,weight
A,B,4.5
B,C,2.0
```

### Сохранение

```csharp
GraphIO.SaveCsv(graph, "graph.csv");
```

Опционально без заголовка:

```csharp
GraphIO.SaveCsv(graph, "graph.csv", writeHeader: false);
```

### Загрузка

```csharp
var loaded = GraphIO.LoadCsv("graph.csv", s => s, isDirected: true);
```

**Особенности:**
- Заголовок автоматически определяется по первой строке,
  начинающейся со слова `source`.
- Если `weight` не указан, используется значение `1.0`.
- Запятые в именах вершин **не поддерживаются** (используйте JSON или GraphML).

### Открыть в Excel

CSV открывается «как есть». Можно построить сводные таблицы или
импортировать в SQL.

### Загрузить в pandas

```python
import pandas as pd
import networkx as nx

df = pd.read_csv("graph.csv")
G = nx.from_pandas_edgelist(df, "source", "target", "weight")
```

---

## DOT

Формат GraphViz. Библиотека **экспортирует** в DOT через
`GraphExporters.ToDot` (см. [Визуализацию](visualization.md)) и
**импортирует** простые конструкции.

### Загрузка

```csharp
var graph = GraphIO.LoadDot("graph.dot", s => s);
```

Поддерживаются:
- `digraph` / `graph`
- Рёбра `a -> b` (ориентированные)
- Рёбра `a -- b` (неориентированные)
- Веса: `a -> b [label="5"]`

**Не поддерживается:** атрибуты вершин, подграфы, порты, сложные
стили. Для сложных DOT-файлов используйте специализированные парсеры
или конвертируйте в GraphML через `dot -Tgraphml`.

---

## Round-trip: пример

Полный цикл «сохранил → загрузил → проверил»:

```csharp
var original = new GraphBuilder<string>(isDirected: true)
    .AddEdge("A", "B", 4.5)
    .AddEdge("B", "C", 2.0)
    .AddEdge("C", "A", 1.5)
    .Build();

// Сохраняем во всех форматах
GraphIO.SaveGraphML(original, "g.graphml");
GraphIO.SaveGexf(original, "g.gexf");
GraphIO.SaveJson(original, "g.json");
GraphIO.SaveCsv(original, "g.csv");

// Загружаем обратно
var fromGraphML = GraphIO.LoadGraphML("g.graphml", s => s);
var fromGexf = GraphIO.LoadGexf("g.gexf", s => s);
var fromJson = GraphIO.LoadJson("g.json", s => s);
var fromCsv = GraphIO.LoadCsv("g.csv", s => s, isDirected: true);

// Проверяем, что структура сохранилась
foreach (var g in new[] { fromGraphML, fromGexf, fromJson, fromCsv })
{
    Console.WriteLine($"{g.VertexCount} вершин, {g.EdgeCount} рёбер");
}
```

---

## Обработка ошибок

Все методы загрузки бросают исключения при проблемах:

```csharp
try
{
    var graph = GraphIO.LoadGraphML("missing.graphml", s => s);
}
catch (FileNotFoundException)
{
    Console.WriteLine("Файл не найден");
}
catch (System.Xml.XmlException ex)
{
    Console.WriteLine($"Некорректный XML: {ex.Message}");
}
```

Для JSON:

```csharp
try
{
    var graph = GraphIO.LoadJson("broken.json", s => s);
}
catch (JsonException ex)
{
    Console.WriteLine($"Некорректный JSON: {ex.Message}");
}
```

---

## Выбор формата

```
Куда отправляете данные?
├─ В Gephi / Cytoscape → GraphML или GEXF
├─ В GraphViz → DOT (экспорт через ToDot)
├─ В Excel / pandas / SQL → CSV
├─ В свой API / веб-приложение → JSON
└─ В академическую публикацию → GraphML (стандарт)
```

## См. также

- [Визуализация](visualization.md) — экспорт в DOT и Mermaid
- [Преобразования графа](io-and-conversion.md)
- [Матрица смежности](adjacency-matrix.md)
