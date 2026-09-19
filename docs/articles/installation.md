---
uid: articles.installation
title: Установка
---

# Установка

## Требования

- **.NET 10.0** или выше
- **C# 14** или выше

## Через NuGet

```bash
dotnet add package GraphToolkit
```

Или вручную в `.csproj`:

```xml
<PackageReference Include="GraphToolkit" Version="1.0.0" />
```

## Из исходников

```bash
git clone https://github.com/TheGhost1K/GraphToolkit.git
cd GraphToolkit
dotnet build src/GraphToolkit.csproj -c Release
```

## Проверка установки

```csharp
using GraphToolkit.Core;

var g = new Graph<string>(isDirected: true);
g.AddEdge("A", "B", 1.0);
Console.WriteLine($"Vertices: {g.VertexCount}, Edges: {g.EdgeCount}");
// Vertices: 2, Edges: 1
```

## Следующие шаги

- [Быстрый старт](getting-started.md)
- [Ключевые концепции](core-concepts.md)