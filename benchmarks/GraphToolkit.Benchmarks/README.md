# Benchmarks

Бенчмарки на BenchmarkDotNet для измерения производительности алгоритмов
GraphToolkit на разных размерах входных данных.

## Запуск

```bash
# Все бенчмарки
dotnet run -c Release --project benchmarks/GraphToolkit.Benchmarks

# Только конкретный класс
dotnet run -c Release --project benchmarks/GraphToolkit.Benchmarks -- --filter "*Traversal*"

# С экспортом в markdown
dotnet run -c Release --project benchmarks/GraphToolkit.Benchmarks -- --exporters markdown
```

## Категории

- `TraversalBenchmarks` — BFS, DFS на разреженных и цепочечных графах
- `ShortestPathsBenchmarks` — Дейкстра, Беллман-Форд, Флойд-Уоршелл
- `MstBenchmarks` — Краскал, Прим, Борувка
- `FlowBenchmarks` — Форд-Фалкерсон, Эдмондс-Карп, Диниц
- `StructuresBenchmarks` — Segment Tree vs Fenwick

## Результаты

Последние результаты — в `docs/articles/performance.md`.
