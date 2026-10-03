# FileIO — импорт и экспорт графов

Пример демонстрирует работу с пятью форматами через `GraphIO`:

- **GraphML** — стандарт для соцсетей, Gephi
- **GEXF** — формат Gephi
- **JSON** — собственный обмен
- **CSV** — Excel, SQL, pandas
- **DOT** — GraphViz

## Запуск

```bash
cd samples/FileIO
dotnet run
```

## Что демонстрируется

1. **Экспорт** одного графа во все форматы одновременно.
2. **Round-trip**: сохранили → загрузили → проверили структуру.
3. **Разные типы вершин**: `string` и `int`.
4. **Импорт DOT** с весами.
5. **Пайплайн**: DOT → MST → GraphML.

## Пример кода

```csharp
using GraphToolkit.IO;

// Экспорт
GraphIO.SaveGraphML(graph, "graph.graphml");
GraphIO.SaveJson(graph, "graph.json");
GraphIO.SaveCsv(graph, "graph.csv");

// Импорт
var fromGraphML = GraphIO.LoadGraphML("graph.graphml", s => s);
var fromJson = GraphIO.LoadJson("graph.json", int.Parse);
var fromCsv = GraphIO.LoadCsv("graph.csv", s => s, isDirected: true);
var fromDot = GraphIO.LoadDot("graph.dot", s => s);
```

## Форматы

| Формат | Читается | Пишется |
|---|---|---|
| GraphML | ✅ | ✅ |
| GEXF | ✅ | ✅ |
| JSON | ✅ | ✅ |
| CSV | ✅ | ✅ |
| DOT | ✅ | ✅ (через `GraphExporters.ToDot`) |

## Интеграция

- **Gephi**: GraphML / GEXF
- **pandas**: CSV
- **GraphViz**: DOT
- **Excel / SQL**: CSV

## См. также

- [Работа с файлами (IO)](../../docs/articles/file-io.md)
- [Визуализация](../../docs/articles/visualization.md)
