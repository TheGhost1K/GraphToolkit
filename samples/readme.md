# GraphToolkit Samples

Коллекция законченных примеров использования библиотеки GraphToolkit.

## Примеры

| Пример | Что демонстрирует | Алгоритмы |
|---|---|---|
| [Routing](Routing/) | Маршрутизация в дорожной сети | Дейкстра, A* |
| [SocialNetwork](SocialNetwork/) | Анализ социального графа | SCC, центроид, мосты |
| [Scheduling](Scheduling/) | Планирование задач с зависимостями | Топосортировка, критический путь |
| [Transportation](Transportation/) | Транспортная задача | Min-cost flow, макс. поток |
| [Clustering](Clustering/) | Кластеризация данных | MST, компоненты связности |
| [Analysis](Analysis/) | Анализ соцсети через центральности | PageRank, Louvain, Betweenness |
| [Flows](Flows/) | Сети потоков | Форд-Фалкерсон, Эдмондс-Карп, Диниц, min-cut |
| [Eulerian](Eulerian/) | Эйлеровы пути | Хиерхольцер, проверка циклов |
| [Hamiltonian](Hamiltonian/) | Гамильтоновы циклы и TSP | Backtracking, ветви и границы, 2-opt |
| [Coloring](Coloring/) | Раскраска графа | Жадная, DSATUR, двудольность |
| [Closure](Closure/) | Транзитивное замыкание | Замыкание, сокращение |
| [Matching](Matching/) | Паросочетания и назначения | Куна, Blossom, Венгерский |
| [AdvancedTrees](AdvancedTrees/) | Продвинутые алгоритмы на деревьях | HLD, LCT, Centroid Decomposition |
| [FileIO](FileIO/) | Работа с форматами | GraphML, GEXF, JSON, CSV, DOT |
| [Structures](Structures/) | Структуры данных | **Segment Tree, Fenwick Tree |

## Запуск

Каждый пример — отдельный консольный проект:

```bash
cd samples/Routing
dotnet run
```

Или все сразу из корня решения:

```bash
dotnet run --project samples/Routing
dotnet run --project samples/SocialNetwork
dotnet run --project samples/Scheduling
dotnet run --project samples/Transportation
dotnet run --project samples/Clustering
dotnet run --project samples/Analysis
dotnet run --project samples/Flows
dotnet run --project samples/Eulerian
dotnet run --project samples/Hamiltonian
dotnet run --project samples/Coloring
dotnet run --project samples/Closure
dotnet run --project samples/AdvancedTrees
dotnet run --project samples/FileIO
dotnet run --project samples/Structures
```

## Требования

- .NET 10.0 SDK
- GraphToolkit (ссылка через ProjectReference)

## Структура примера

Каждый пример содержит:
- `Program.cs` — точка входа с main-логикой
- `<Name>.csproj` — файл проекта со ссылкой на библиотеку
- `README.md` — описание примера
- `data/` — данные (если применимо)
