using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class ColoringCommandTests
{
    [Fact]
    public void Coloring_Greedy_TriangleUses3Colors()
    {
        var file = TestHelpers.TriangleDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "coloring", file, "--algorithm", "greedy");

            exit.Should().Be(0);
            stdout.Should().Contain("Цветов: 3");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Coloring_Exact_TriangleHasChromatic3()
    {
        var file = TestHelpers.TriangleDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "coloring", file, "--algorithm", "exact");

            exit.Should().Be(0);
            stdout.Should().Contain("Хроматическое число");
            stdout.Should().Contain("3");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Coloring_Bipartite_EvenCycle()
    {
        var file = TestHelpers.TempFile(@"
            graph G {
                A -- B;
                B -- C;
                C -- D;
                D -- A;
            }
        ");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "coloring", file, "--algorithm", "bipartite");

            exit.Should().Be(0);
            stdout.Should().Contain("Двудольный: True");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }
}
