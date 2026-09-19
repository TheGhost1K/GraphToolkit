# GraphToolkit Samples

Коллекция законченных примеров использования библиотеки GraphToolkit
для решения реальных задач.

## Примеры

| Пример | Что демонстрирует | Алгоритмы |
|---|---|---|
| [Routing](Routing/) | Маршрутизация в дорожной сети | Дейкстра, A* |
| [SocialNetwork](SocialNetwork/) | Анализ социального графа | SCC, центроид, мосты |
| [Scheduling](Scheduling/) | Планирование задач с зависимостями | Топосортировка, критический путь |
| [Transportation](Transportation/) | Транспортная задача | Min-cost flow, макс. поток |
| [Clustering](Clustering/) | Кластеризация данных | MST, компоненты связности |

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
```

## Требования

- .NET 10.0 SDK
- GraphToolkit (ссылка через ProjectReference)

## Структура примера

Каждый пример содержит:
- `Program.cs` — точка входа с main-логикой
- `<Name>.csproj` — файл проекта со ссылкой на библиотеку