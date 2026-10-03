using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class MstCommandTests
{
    [Fact]
    public void Mst_Kruskal_ReturnsMstWithCorrectSize()
    {
        var file = TestHelpers.UndirectedDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(root, "mst", file);

            exit.Should().Be(0);
            stdout.Should().Contain("Итого:");
            stdout.Should().Contain("3 рёбер");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Mst_Prim_SameResultAsKruskal()
    {
        var file = TestHelpers.UndirectedDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "mst", file, "--algorithm", "prim");

            exit.Should().Be(0);
            stdout.Should().Contain("Итого:");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Mst_Boruvka_Works()
    {
        var file = TestHelpers.UndirectedDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "mst", file, "--algorithm", "boruvka");

            exit.Should().Be(0);
            stdout.Should().Contain("Итого:");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Mst_WithOutput_SavesFile()
    {
        var file = TestHelpers.UndirectedDotFile();
        var output = Path.Combine(
            Path.GetTempPath(), $"gt-mst-{Guid.NewGuid():N}.json");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, _, _) = TestHelpers.Run(
                root, "mst", file, "--output", output);

            exit.Should().Be(0);
            File.Exists(output).Should().BeTrue();
        }
        finally
        {
            TestHelpers.SafeDelete(file);
            TestHelpers.SafeDelete(output);
        }
    }
}
