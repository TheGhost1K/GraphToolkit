using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class TraversalCommandTests
{
    [Fact]
    public void Traversal_Bfs_FindsPath()
    {
        var file = TestHelpers.SimpleDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "traversal", file,
                "--algorithm", "bfs",
                "--start", "A", "--end", "C");

            exit.Should().Be(0);
            stdout.Should().Contain("A");
            stdout.Should().Contain("C");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Traversal_Topo_ReturnsValidOrder()
    {
        var file = TestHelpers.TempFile(@"
            digraph G {
                A -> B;
                A -> C;
                B -> D;
                C -> D;
            }
        ");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "traversal", file, "--algorithm", "topo");

            exit.Should().Be(0);
            int posA = stdout.IndexOf('A');
            int posD = stdout.IndexOf('D');
            posA.Should().BeGreaterThanOrEqualTo(0);
            posD.Should().BeGreaterThanOrEqualTo(0);
            posA.Should().BeLessThan(posD);
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Traversal_Topo_WithCycle_ReturnsError()
    {
        var file = TestHelpers.TempFile(@"
            digraph G {
                A -> B;
                B -> A;
            }
        ");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, _, stderr) = TestHelpers.Run(
                root, "traversal", file, "--algorithm", "topo");

            exit.Should().NotBe(0);
            stderr.Should().Contain("цикл");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Traversal_Levels_ShowsLevels()
    {
        var file = TestHelpers.TempFile(@"
            digraph G {
                A -> B;
                A -> C;
                B -> D;
                C -> D;
            }
        ");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "traversal", file, "--algorithm", "levels");

            exit.Should().Be(0);
            stdout.Should().Contain("Уровень 0");
            stdout.Should().Contain("Уровень 1");
            stdout.Should().Contain("Уровень 2");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }
}
