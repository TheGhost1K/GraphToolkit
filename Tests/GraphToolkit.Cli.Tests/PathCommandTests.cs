using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class PathCommandTests
{
    [Fact]
    public void Path_Dijkstra_FindsShortestPath()
    {
        var file = TestHelpers.SimpleDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "path", file, "--from", "A", "--to", "C");

            exit.Should().Be(0);
            stdout.Should().Contain("A");
            stdout.Should().Contain("C");
            stdout.Should().Contain("->");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Path_Bfs_FindsPath()
    {
        var file = TestHelpers.SimpleDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "path", file,
                "--from", "A", "--to", "C",
                "--algorithm", "bfs");

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
    public void Path_Unreachable_ReturnsError()
    {
        var file = TestHelpers.TempFile(@"
            digraph G {
                A -> B;
                Z;
            }
        ");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, _, stderr) = TestHelpers.Run(
                root, "path", file, "--from", "A", "--to", "Z");

            exit.Should().NotBe(0);
            stderr.Should().Contain("не найден");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Path_MissingRequiredOptions_ReturnsError()
    {
        var file = TestHelpers.SimpleDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, _, _) = TestHelpers.Run(root, "path", file);

            exit.Should().NotBe(0);
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }
}
