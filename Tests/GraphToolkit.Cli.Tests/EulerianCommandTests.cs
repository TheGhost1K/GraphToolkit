using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class EulerianCommandTests
{
    [Fact]
    public void Eulerian_Check_CycleGraph()
    {
        var file = TestHelpers.TriangleDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "eulerian", file, "--mode", "check");

            exit.Should().Be(0);
            stdout.Should().Contain("Эйлеров цикл: True");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Eulerian_Find_ReturnsPath()
    {
        var file = TestHelpers.TriangleDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "eulerian", file, "--mode", "find");

            exit.Should().Be(0);
            stdout.Should().Contain("->");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }
}
