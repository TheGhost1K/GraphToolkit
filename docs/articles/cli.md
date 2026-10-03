---
uid: articles.cli
title: CLI-утилита
---

# CLI-утилита `graph-toolkit`

Утилита командной строки для работы с графами **без написания кода**.
Позволяет быстро проверить структуру графа, найти путь, построить MST,
выделить сообщества и сконвертировать между форматами.

## Установка

```bash
dotnet tool install -g GraphToolkit.Cli
```

Проверка:

```bash
graph-toolkit --version
graph-toolkit --help
```

Обновление до последней версии:

```bash
dotnet tool update -g GraphToolkit.Cli
```

Удаление:

```bash
dotnet tool uninstall -g GraphToolkit.Cli
```

## Общий синтаксис

```
graph-toolkit <command> <file> [options]
```

Все команды принимают **файл с графом** и опциональные флаги.

## Поддерживаемые форматы ввода

| Расширение | Формат | Читается | Пишется |
|---|---|---|---|
| `.dot`, `.gv` | GraphViz DOT | ✅ | ✅ (visualize) |
| `.graphml` | GraphML | ✅ | ✅ |
| `.gexf` | GEXF (Gephi) | ✅ | ✅ |
| `.json` | JSON | ✅ | ✅ |
| `.csv` | CSV | ✅ | ✅ |

Формат определяется **по расширению** автоматически. Если файл называется
`graph.dot` — будет прочитан как DOT, `graph.graphml` — как GraphML, и т. д.

---

## `info` — информация о графе

Быстрый «паспорт» графа: сколько вершин, рёбер, какая структура.

### Синтаксис

```bash
graph-toolkit info <file>
```

### Пример

Возьмём граф из `samples/Routing`:

```bash
graph-toolkit info city.dot
```

Вывод:

```
Файл:              city.dot
Формат:            dot
Ориентированный:   нет
Вершин:            7
Рёбер:             11
Макс. степень:     4
Мин. степень:      1
Средняя степень:   3.14
Мин. вес:          6
Макс. вес:         35
```

### Что проверять

- **Ориентированный** — важно для выбора алгоритма (например, SCC
  работает только с directed).
- **Средняя степень** — если она ≈ V, граф плотный, используйте
  матрицу смежности. Если ≈ 2–3, разреженный — список смежности.
- **Разброс весов** — если веса в разных диапазонах (например, от 0.001
  до 10000), нормализуйте перед запуском PageRank или Louvain.

---

## `path` — поиск пути

Находит путь между двумя вершинами. Поддерживает пять алгоритмов.

### Синтаксис

```bash
graph-toolkit path <file> --from <A> --to <B> [--algorithm <name>]
```

### Параметры

| Параметр | Обязательный | По умолчанию | Описание |
|---|---|---|---|
| `--from` | ✅ | — | Начальная вершина |
| `--to` | ✅ | — | Конечная вершина |
| `--algorithm` | ❌ | `dijkstra` | Алгоритм поиска |

### Алгоритмы

| Название | Сложность | Требования | Когда использовать |
|---|---|---|---|
| `dijkstra` | O((V+E) log V) | Неотрицательные веса | Взвешенный граф, по умолчанию |
| `bfs` | O(V + E) | Невзвешенный | Нужен кратчайший по рёбрам |
| `dfs` | O(V + E) | Любой | Нужен любой путь (быстрее) |
| `bellman-ford` | O(V · E) | Работает с отрицательными | Граф с отрицательными весами |
| `astar` | Зависит от эвристики | Неотрицательные веса | Есть координаты вершин |

### Пример: сравнение алгоритмов

```bash
# Все три дадут разный результат на одном графе
graph-toolkit path graph.dot --from A --to F
# A -> C -> E -> D -> F          (оптимальный по весу)

graph-toolkit path graph.dot --from A --to F --algorithm=bfs
# A -> B -> D -> F               (оптимальный по числу рёбер)

graph-toolkit path graph.dot --from A --to F --algorithm=dfs
# A -> B -> C -> D -> F          (любой путь)
```

### Обработка ошибок

```bash
$ graph-toolkit path graph.dot --from A --to Z
Путь A -> Z не найден.
$ echo $?
1
```

Возвращает код `1`, если путь не существует. Это удобно для скриптов.

### Реальный кейс: маршрут в дорожной сети

```bash
# Найти оптимальный маршрут
graph-toolkit path roads.dot --from "Центр" --to "Аэропорт"

# Сравнить с маршрутом по числу перекрёстков
graph-toolkit path roads.dot --from "Центр" --to "Аэропорт" --algorithm=bfs
```

---

## `mst` — минимальное остовное дерево

Строит MST для неориентированного взвешенного графа.

### Синтаксис

```bash
graph-toolkit mst <file> [--algorithm <name>] [--output <file>]
```

### Параметры

| Параметр | По умолчанию | Описание |
|---|---|---|
| `--algorithm` | `kruskal` | `kruskal`, `prim`, `boruvka` |
| `--output` | — | Файл для сохранения MST (формат по расширению) |

### Пример

```bash
graph-toolkit mst network.dot
```

Вывод:

```
A — B  (1)
B — C  (2)
C — D  (4)
D — E  (3)

Итого: 4 рёбер, вес = 10
```

### Сохранение в файл

```bash
graph-toolkit mst network.dot --output=mst.graphml
```

MST сохранится в GraphML — можно открыть в Gephi.

### Сравнение алгоритмов

```bash
# Все три дадут одинаковый суммарный вес, но разный набор рёбер
graph-toolkit mst graph.dot --algorithm=kruskal
graph-toolkit mst graph.dot --algorithm=prim
graph-toolkit mst graph.dot --algorithm=boruvka
```

### Реальный кейс: прокладка сети

```bash
# Дано: координаты серверов и стоимость прокладки кабеля между ними
graph-toolkit info servers.dot         # смотрим размер
graph-toolkit mst servers.dot \
    --algorithm=boruvka \
    --output=network-plan.graphml
```

---

## `scc` — компоненты сильной связности

Работает для **ориентированных** графов. Находит группы вершин, где
каждая достижима из каждой.

### Синтаксис

```bash
graph-toolkit scc <file> [--components]
```

### Пример

```bash
graph-toolkit scc web.dot
```

Вывод:

```
SCC: 3
  {Главная, Новости, Каталог}
  {Товар 1, Товар 2}
  {Корзина}
```

### Плюс связные компоненты

```bash
graph-toolkit scc web.dot --components
```

Вывод:

```
SCC: 3
  {Главная, Новости, Каталог}
  {Товар 1, Товар 2}
  {Корзина}

Связные компоненты: 1
  {Главная, Новости, Каталог, Товар 1, Товар 2, Корзина}
```

### Реальный кейс: анализ модулей

```bash
# В графе зависимостей модулей — найти «острова» взаимозависимости
graph-toolkit scc modules.dot
```

Каждая SCC — это группа модулей, которые не могут быть разделены без
циклических зависимостей.

---

## `visualize` — конвертация и экспорт

Преобразует граф в текстовый формат для вставки в документацию или
просмотра.

### Синтаксис

```bash
graph-toolkit visualize <file> [--format <name>] [--output <file>]
```

### Форматы

| Формат | Что выдаёт | Куда вставлять |
|---|---|---|
| `dot` | GraphViz DOT | https://webgraphviz.com |
| `mermaid` | Mermaid | Markdown (GitHub, GitLab, Notion) |
| `matrix` | Матрица смежности | В консоль / документ |
| `list` | Список смежности | В консоль / документ |

### Пример: экспорт в Mermaid

```bash
graph-toolkit visualize graph.dot --format=mermaid
```

Вывод:

```
graph LR
    V0["A"]
    V1["B"]
    V2["C"]
    V0 -->|4| V1
    V0 -->|2| V2
    V1 -->|5| V2
```

Скопируйте в блок с языком `mermaid` в markdown — GitHub отрисует
интерактивную диаграмму.

### Пример: матрица смежности

```bash
graph-toolkit visualize graph.dot --format=matrix
```

Вывод:

```
          A     B     C     D
    A     0     4     2     -
    B     -     0     5    10
    C     -     -     0     -
    D     -     -     -     0
```

### Сохранение в файл

```bash
graph-toolkit visualize graph.dot --format=mermaid --output=graph.md
```

---

## Полный пайплайн: от данных к результату

### Сценарий: анализ социальной сети

Дано: файл `network.gexf` (экспорт из Gephi).

```bash
# 1. Что за граф?
graph-toolkit info network.gexf

# 2. Найти сообщества через SCC
graph-toolkit scc network.gexf

# 3. Построить каркас сети
graph-toolkit mst network.gexf --output=backbone.graphml

# 4. Сконвертировать в markdown
graph-toolkit visualize network.gexf --format=mermaid --output=network.md

# 5. Вставить в README
cat network.md >> README.md
```

### Сценарий: маршрутизация

Дано: `roads.dot` — дорожная сеть города.

```bash
# 1. Информация
graph-toolkit info roads.dot

# 2. Прямой маршрут
graph-toolkit path roads.dot --from "Центр" --to "Аэропорт"

# 3. Альтернативный маршрут (по перекрёсткам)
graph-toolkit path roads.dot --from "Центр" --to "Аэропорт" --algorithm=bfs

# 4. Визуализировать для отчёта
graph-toolkit visualize roads.dot --format=mermaid --output=roads.md
```

### Сценарий: конвертация форматов

```bash
# Gephi → CSV (для Excel)
graph-toolkit visualize network.gexf --format=list > network.csv

# Или конвертация напрямую (пока только через файл)
graph-toolkit mst network.gexf --output=network.graphml
```

---

## Использование в скриптах

CLI возвращает **стандартные коды выхода**:

| Код | Значение |
|---|---|
| `0` | Успех |
| `1` | Путь не найден / ошибка валидации |
| `2` | Ошибка парсинга файла |

### Bash: проверка существования пути

```bash
if graph-toolkit path graph.dot --from A --to B > /dev/null 2>&1; then
    echo "Путь есть"
    graph-toolkit path graph.dot --from A --to B
else
    echo "Путь не найден"
fi
```

### PowerShell: пакетная обработка

```powershell
Get-ChildItem *.dot | ForEach-Object {
    Write-Host "=== $($_.Name) ==="
    graph-toolkit info $_.FullName
}
```

### Make: автоматизация документации

```makefile
docs/graph.md: graphs/*.dot
	graph-toolkit visualize $< --format=mermaid --output=$@
```

---

## Что дальше

- Полная документация по IO: [Работа с файлами](file-io.md)
- Визуализация: [Визуализация](visualization.md)
- Центральности на реальных данных: [Примеры анализа](analysis-examples.md)

## См. также

- [Установка](installation.md)
- [Работа с файлами](file-io.md)
- [Визуализация](visualization.md)
