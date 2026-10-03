using System.CommandLine;
using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

/// <summary>
/// Проверяет структуру CLI: все подкоманды зарегистрированы.
/// </summary>
public class CommandStructureTests
{
    [Fact]
    public void Root_HasAllExpectedSubcommands()
    {
        var root = Program.BuildRootCommand();

        var expected = new[]
        {
            // Основные
            "info", "convert", "visualize",
            // Пути и обходы
            "path", "traversal",
            // Структура
            "mst", "components", "scc", "eulerian",
            // Потоки и разрезы
            "flow", "min-cost-flow", "cut",
            // Специальные
            "matching", "coloring", "tsp",
            // Анализ
            "centrality", "community"
        };

        var actual = root.Subcommands.Select(c => c.Name).ToList();

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void EverySubcommand_HasDescription()
    {
        var root = Program.BuildRootCommand();

        foreach (var cmd in root.Subcommands)
        {
            cmd.Description.Should().NotBeNullOrEmpty(
                $"у команды '{cmd.Name}' должно быть описание");
        }
    }

    [Fact]
    public void EverySubcommand_HasArgumentsOrOptions()
    {
        var root = Program.BuildRootCommand();

        foreach (var cmd in root.Subcommands)
        {
            bool hasArguments = cmd.Arguments.Count > 0;
            bool hasOptions = cmd.Options.Count > 0;

            (hasArguments || hasOptions).Should().BeTrue(
                $"у команды '{cmd.Name}' должны быть аргументы или опции");
        }
    }
}
