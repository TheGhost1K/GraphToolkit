# graph-toolkit CLI

Утилита командной строки для работы с графами через GraphToolkit.

## Установка

```bash
dotnet tool install -g GraphToolkit.Cli
```

## Команды

### `info` — информация о графе

```bash
graph-toolkit info graph.dot
```

### `path` — поиск пути

```bash
graph-toolkit path graph.dot --from A --to F
graph-toolkit path graph.dot --from A --to F --algorithm=bfs
graph-toolkit path graph.dot --from A --to F --algorithm=astar
```

### `mst` — минимальное остовное дерево

```bash
graph-toolkit mst graph.dot
graph-toolkit mst graph.dot --algorithm=prim
graph-toolkit mst graph.dot --output=mst.dot
```

### `scc` — компоненты сильной связности

```bash
graph-toolkit scc graph.dot
graph-toolkit scc graph.dot --components    # + связные компоненты
```

### `visualize` — экспорт в другой формат

```bash
graph-toolkit visualize graph.dot --format=mermaid
graph-toolkit visualize graph.dot --format=matrix --output=matrix.txt
```

## Поддерживаемые форматы

| Расширение | Импорт | Экспорт |
|---|---|---|
| `.dot`, `.gv` | ✅ | ✅ (только visualize) |
| `.graphml` | ✅ | ✅ |
| `.gexf` | ✅ | ✅ |
| `.json` | ✅ | ✅ |
| `.csv` | ✅ | ✅ |
