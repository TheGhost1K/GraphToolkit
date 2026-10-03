using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

/// <summary>
/// Тесты граничных случаев и валидации.
/// </summary>
public class EdgeCaseTests
{
    [Fact]
    public void AllCommands_WithMissingFile_ReturnNonZero()
    {
        var root = Program.BuildRootCommand();

        var commands = new[]
        {
            new[] { "info", "missing.dot" },
            new[] { "convert", "missing.dot", "--to", "json" },
            new[] { "visualize", "missing.dot" },
            new[] { "path", "missing.dot", "--from", "A", "--to", "B" },
            new[] { "traversal", "missing.dot", "--algorithm", "bfs",
                    "--start", "A", "--end", "B" },
            new[] { "mst", "missing.dot" },
            new[] { "components", "missing.dot" },
            new[] { "scc", "missing.dot" },
            new[] { "eulerian", "missing.dot" },
            new[] { "coloring", "missing.dot" },
            new[] { "centrality", "missing.dot" },
            new[] { "community", "missing.dot" },
        };

        foreach (var args in commands)
        {
            var (exit, _, _) = TestHelpers.Run(root, args);
            exit.Should().NotBe(0,
                $"команда '{string.Join(' ', args)}' должна вернуть ошибку " +
                $"на отсутствующий файл");
        }
    }

    [Fact]
    public void Root_Help_ShowsAllCommands()
    {
        var root = Program.BuildRootCommand();
        var (_, stdout, _) = TestHelpers.Run(root, "--help");

        var expectedCommands = new[]
        {
            "info", "convert", "visualize",
            "path", "traversal",
            "mst", "components", "scc", "eulerian",
            "flow", "min-cost-flow", "cut",
            "matching", "coloring", "tsp",
            "centrality", "community"
        };

        foreach (var cmd in expectedCommands)
        {
            stdout.Should().Contain(cmd,
                $"команда '{cmd}' должна быть в справке");
        }
    }
}
